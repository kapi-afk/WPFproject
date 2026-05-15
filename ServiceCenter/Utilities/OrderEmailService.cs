using MailKit.Security;
using MimeKit;
using ServiceCenter.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ServiceCenter.Utilities
{
    public static class OrderEmailService
    {
        public static bool TrySendPasswordRecoveryCode(User user, string recoveryCode, DateTime expiresAt)
        {
            try
            {
                if (user == null)
                {
                    TraceEmailIssue("Password recovery email skipped: user is null.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    TraceEmailIssue($"Password recovery email skipped: user #{user.Id} has empty email.");
                    return false;
                }

                var subject = "Код восстановления пароля";
                var body =
                    $"Здравствуйте, {user.Name ?? user.Login}!{Environment.NewLine}{Environment.NewLine}" +
                    $"Ваш код восстановления пароля: {recoveryCode}{Environment.NewLine}" +
                    $"Срок действия кода: до {expiresAt:dd.MM.yyyy HH:mm}.{Environment.NewLine}{Environment.NewLine}" +
                    "Если вы не запрашивали восстановление пароля, просто проигнорируйте это письмо.";

                return TrySendPlainTextEmail(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                TraceEmailIssue($"Password recovery email failed. Recipient={user?.Email}; Exception={ex}");
                return false;
            }
        }

        public static void TrySendOrderStatusChangedEmail(Order order, OrderStatus previousStatus)
        {
            try
            {
                if (order == null)
                {
                    TraceEmailIssue("Order email notification skipped: order is null.");
                    return;
                }

                if (order.User == null)
                {
                    TraceEmailIssue($"Order email notification skipped: order #{order.Id} has no loaded User.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(order.User.Email))
                {
                    TraceEmailIssue($"Order email notification skipped: order #{order.Id} user has empty email.");
                    return;
                }

                if (previousStatus == order.Status)
                {
                    TraceEmailIssue($"Order email notification skipped: order #{order.Id} status did not change.");
                    return;
                }

                var settings = GetNotificationSettings();
                if (!HasConfiguredSmtp(settings))
                {
                    if (!TrySendWebhookEmail(order, settings.SenderEmail, settings.WebhookUrl))
                    {
                        TraceEmailIssue("Order email notification skipped: SMTP settings are not configured and webhook fallback is unavailable.");
                    }

                    return;
                }

                EnsureModernTls();
                TraceEmailIssue(
                    $"Order email notification started. OrderId={order.Id}; PublicNumber={order.DisplayNumber}; " +
                    $"Recipient={order.User.Email}; OldStatus={previousStatus}; NewStatus={order.Status}");

                if (TrySendWebhookEmail(order, settings.SenderEmail, settings.WebhookUrl))
                {
                    return;
                }

                var message = BuildMessage(order, settings.SenderEmail);
                var attempts = BuildConnectionAttempts(settings.SmtpHost, settings.SmtpPort, settings.EnableSsl);
                Exception lastException = null;

                foreach (var attempt in attempts)
                {
                    try
                    {
                        using (var client = CreateMailKitClient(attempt.Host))
                        {
                            client.Connect(attempt.Host, attempt.Port, attempt.SocketOptions);
                            client.Authenticate(settings.SenderEmail, settings.SenderPassword);
                            client.Send(message);
                            client.Disconnect(true);
                            TraceEmailIssue(
                                $"Order email notification sent via MailKit. Host={attempt.Host}; Port={attempt.Port}; Recipient={order.User.Email}");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        TraceEmailIssue(
                            $"Order email attempt failed. Host={attempt.Host}; Port={attempt.Port}; " +
                            $"SocketOptions={attempt.SocketOptions}; Recipient={order?.User?.Email}; Exception={ex.Message}");
                    }
                }

                TrySendLegacySmtpEmail(
                    order,
                    settings.SenderEmail,
                    settings.SenderPassword,
                    settings.SmtpHost,
                    settings.SmtpPort,
                    settings.EnableSsl,
                    lastException);
            }
            catch (Exception ex)
            {
                TraceEmailIssue(
                    $"Order email notification failed. Host={Properties.Settings.Default.NotificationSmtpHost}; " +
                    $"Port={Properties.Settings.Default.NotificationSmtpPort}; " +
                    $"Ssl={Properties.Settings.Default.NotificationEnableSsl}; " +
                    $"Recipient={order?.User?.Email}; Exception={ex}");
            }
        }

        private static string BuildStatusChangedBody(Order order)
        {
            var customerName = string.IsNullOrWhiteSpace(order.User?.Name)
                ? "\u043a\u043b\u0438\u0435\u043d\u0442"
                : order.User.Name;
            var deviceName = string.Join(" ", new[] { order.DeviceType, order.DeviceBrand, order.DeviceModel }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
            var masterName = order.AssignedMaster?.Name ?? "\u0435\u0449\u0435 \u043d\u0435 \u043d\u0430\u0437\u043d\u0430\u0447\u0435\u043d";
            var orderNumber = string.IsNullOrWhiteSpace(order.DisplayNumber) ? order.Id.ToString() : order.DisplayNumber;

            var body =
                $"\u0417\u0434\u0440\u0430\u0432\u0441\u0442\u0432\u0443\u0439\u0442\u0435, {customerName}!{Environment.NewLine}{Environment.NewLine}" +
                $"\u0421\u0442\u0430\u0442\u0443\u0441 \u0432\u0430\u0448\u0435\u0439 \u0437\u0430\u044f\u0432\u043a\u0438 {orderNumber} \u0438\u0437\u043c\u0435\u043d\u0438\u043b\u0441\u044f.{Environment.NewLine}" +
                $"\u0423\u0441\u0442\u0440\u043e\u0439\u0441\u0442\u0432\u043e: {deviceName}{Environment.NewLine}" +
                $"\u0421\u0442\u0430\u043b\u043e: {GetStatusText(order.Status)}{Environment.NewLine}" +
                $"\u041c\u0430\u0441\u0442\u0435\u0440: {masterName}";

            if (order.EstimatedRepairCost > 0)
            {
                var culture = CultureInfo.GetCultureInfo("ru-BY");
                body +=
                    $"{Environment.NewLine}\u041f\u0440\u0438\u043c\u0435\u0440\u043d\u0430\u044f \u0441\u0443\u043c\u043c\u0430 \u0437\u0430 \u0440\u0435\u043c\u043e\u043d\u0442: {order.EstimatedRepairCost.ToString("N2", culture)} BYN" +
                    $"{Environment.NewLine}\u0421\u0443\u043c\u043c\u0430 \u043f\u0440\u0435\u0434\u0432\u0430\u0440\u0438\u0442\u0435\u043b\u044c\u043d\u0430\u044f \u0438 \u043c\u043e\u0436\u0435\u0442 \u0443\u0442\u043e\u0447\u043d\u044f\u0442\u044c\u0441\u044f \u043f\u043e \u0445\u043e\u0434\u0443 \u0440\u0435\u043c\u043e\u043d\u0442\u0430.";
            }

            return body;
        }

        private static string GetStatusText(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Created:
                    return "\u041e\u0431\u0440\u0430\u0431\u0430\u0442\u044b\u0432\u0430\u0435\u0442\u0441\u044f";
                case OrderStatus.Assigned:
                    return "\u041d\u0430\u0437\u043d\u0430\u0447\u0435\u043d \u043c\u0430\u0441\u0442\u0435\u0440";
                case OrderStatus.Diagnosing:
                    return "\u0414\u0438\u0430\u0433\u043d\u043e\u0441\u0442\u0438\u043a\u0430";
                case OrderStatus.WaitingForParts:
                    return "\u041e\u0436\u0438\u0434\u0430\u043d\u0438\u0435 \u0434\u0435\u0442\u0430\u043b\u0435\u0439";
                case OrderStatus.InProgress:
                    return "\u0412 \u0440\u0435\u043c\u043e\u043d\u0442\u0435";
                case OrderStatus.ReadyForPickup:
                    return "\u0413\u043e\u0442\u043e\u0432\u0430 \u043a \u0432\u044b\u0434\u0430\u0447\u0435";
                case OrderStatus.Completed:
                    return "\u0417\u0430\u0432\u0435\u0440\u0448\u0435\u043d";
                case OrderStatus.Cancelled:
                    return "\u041e\u0442\u043c\u0435\u043d\u0435\u043d";
                default:
                    return status.ToString();
            }
        }

        private static MimeMessage BuildMessage(Order order, string senderEmail)
        {
            return CreatePlainTextMimeMessage(
                senderEmail,
                order.User.Email,
                BuildStatusChangedSubject(order),
                BuildStatusChangedBody(order));
        }

        private static string BuildStatusChangedSubject(Order order)
        {
            var orderNumber = string.IsNullOrWhiteSpace(order.DisplayNumber) ? order.Id.ToString() : order.DisplayNumber;
            return $"\u0421\u0442\u0430\u0442\u0443\u0441 \u0437\u0430\u044f\u0432\u043a\u0438 {orderNumber} \u0438\u0437\u043c\u0435\u043d\u0435\u043d";
        }

        private static bool TrySendPlainTextEmail(string recipientEmail, string subject, string body)
        {
            var settings = GetNotificationSettings();

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                TraceEmailIssue("Generic email send skipped: recipient email is empty.");
                return false;
            }

            if (!HasConfiguredSmtp(settings))
            {
                TraceEmailIssue("Generic email send skipped: SMTP settings are not configured.");
                return false;
            }

            EnsureModernTls();
            var attempts = BuildConnectionAttempts(settings.SmtpHost, settings.SmtpPort, settings.EnableSsl);
            Exception lastException = null;

            foreach (var attempt in attempts)
            {
                try
                {
                    using (var message = CreatePlainTextMimeMessage(settings.SenderEmail, recipientEmail, subject, body))
                    using (var client = CreateMailKitClient(attempt.Host))
                    {
                        client.Connect(attempt.Host, attempt.Port, attempt.SocketOptions);
                        client.Authenticate(settings.SenderEmail, settings.SenderPassword);
                        client.Send(message);
                        client.Disconnect(true);
                        TraceEmailIssue($"Generic email sent via MailKit. Host={attempt.Host}; Port={attempt.Port}; Recipient={recipientEmail}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    TraceEmailIssue(
                        $"Generic email attempt failed. Host={attempt.Host}; Port={attempt.Port}; " +
                        $"SocketOptions={attempt.SocketOptions}; Recipient={recipientEmail}; Exception={ex.Message}");
                }
            }

            try
            {
                TrySendLegacyPlainTextEmail(
                    recipientEmail,
                    subject,
                    body,
                    settings.SenderEmail,
                    settings.SenderPassword,
                    settings.SmtpHost,
                    settings.SmtpPort,
                    settings.EnableSsl,
                    lastException);
                return true;
            }
            catch (Exception ex)
            {
                TraceEmailIssue($"Generic email failed after fallback. Recipient={recipientEmail}; Exception={ex}");
                return false;
            }
        }

        private static void TrySendLegacySmtpEmail(
            Order order,
            string senderEmail,
            string senderPassword,
            string smtpHost,
            int smtpPort,
            bool enableSsl,
            Exception mailKitException)
        {
            try
            {
                RunWithTemporaryCertificateValidationBypass(() =>
                {
                    using (var message = new MailMessage())
                    using (var client = new SmtpClient())
                    {
                        message.From = new MailAddress(senderEmail, Properties.Settings.Default.NotificationEmailDisplayName);
                        message.To.Add(order.User.Email);
                        message.Subject = BuildStatusChangedSubject(order);
                        message.Body = BuildStatusChangedBody(order);
                        message.BodyEncoding = Encoding.UTF8;
                        message.SubjectEncoding = Encoding.UTF8;

                        ConfigureLegacySmtpClient(client, senderEmail, senderPassword, smtpHost, smtpPort, enableSsl);
                        client.Send(message);
                    }
                });

                TraceEmailIssue(
                    $"Order email notification sent via legacy SMTP fallback. Recipient={order.User.Email}");
            }
            catch (Exception fallbackException)
            {
                throw new AggregateException(
                    "MailKit and legacy SMTP sending failed.",
                    mailKitException ?? new InvalidOperationException("MailKit attempts failed."),
                    fallbackException);
            }
        }

        private static void TrySendLegacyPlainTextEmail(
            string recipientEmail,
            string subject,
            string body,
            string senderEmail,
            string senderPassword,
            string smtpHost,
            int smtpPort,
            bool enableSsl,
            Exception mailKitException)
        {
            try
            {
                RunWithTemporaryCertificateValidationBypass(() =>
                {
                    using (var message = new MailMessage())
                    using (var client = new SmtpClient())
                    {
                        message.From = new MailAddress(senderEmail, Properties.Settings.Default.NotificationEmailDisplayName);
                        message.To.Add(recipientEmail);
                        message.Subject = subject;
                        message.Body = body;
                        message.BodyEncoding = Encoding.UTF8;
                        message.SubjectEncoding = Encoding.UTF8;

                        ConfigureLegacySmtpClient(client, senderEmail, senderPassword, smtpHost, smtpPort, enableSsl);
                        client.Send(message);
                    }
                });

                TraceEmailIssue($"Generic email sent via legacy SMTP fallback. Recipient={recipientEmail}");
            }
            catch (Exception fallbackException)
            {
                throw new AggregateException(
                    "MailKit and legacy SMTP sending failed.",
                    mailKitException ?? new InvalidOperationException("MailKit attempts failed."),
                    fallbackException);
            }
        }

        private static SecureSocketOptions GetSocketOptions(int smtpPort, bool enableSsl)
        {
            if (!enableSsl)
            {
                return SecureSocketOptions.None;
            }

            return smtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;
        }

        private static List<SmtpConnectionAttempt> BuildConnectionAttempts(string smtpHost, int smtpPort, bool enableSsl)
        {
            var attempts = new List<SmtpConnectionAttempt>
            {
                new SmtpConnectionAttempt(smtpHost, smtpPort, GetSocketOptions(smtpPort, enableSsl))
            };

            if (string.Equals(smtpHost, "smtp.gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                AddAttemptIfMissing(attempts, smtpHost, 465, SecureSocketOptions.SslOnConnect);
                AddAttemptIfMissing(attempts, smtpHost, 587, SecureSocketOptions.StartTls);
                AddAttemptIfMissing(attempts, smtpHost, 587, SecureSocketOptions.Auto);
            }

            return attempts;
        }

        private static void AddAttemptIfMissing(List<SmtpConnectionAttempt> attempts, string host, int port, SecureSocketOptions socketOptions)
        {
            if (attempts.Any(item =>
                string.Equals(item.Host, host, StringComparison.OrdinalIgnoreCase) &&
                item.Port == port &&
                item.SocketOptions == socketOptions))
            {
                return;
            }

            attempts.Add(new SmtpConnectionAttempt(host, port, socketOptions));
        }

        private static NotificationEmailSettings GetNotificationSettings()
        {
            return new NotificationEmailSettings(
                Properties.Settings.Default.NotificationEmailAddress,
                NormalizeSmtpPassword(Properties.Settings.Default.NotificationEmailPassword),
                Properties.Settings.Default.NotificationSmtpHost,
                Properties.Settings.Default.NotificationSmtpPort,
                Properties.Settings.Default.NotificationEnableSsl,
                Properties.Settings.Default.NotificationWebhookUrl);
        }

        private static bool HasConfiguredSmtp(NotificationEmailSettings settings)
        {
            return !string.IsNullOrWhiteSpace(settings.SenderEmail) &&
                   !string.IsNullOrWhiteSpace(settings.SenderPassword) &&
                   !IsPlaceholderSmtpPassword(settings.SenderPassword);
        }

        private static MimeMessage CreatePlainTextMimeMessage(string senderEmail, string recipientEmail, string subject, string body)
        {
            return new MimeMessage
            {
                From = { new MailboxAddress(Properties.Settings.Default.NotificationEmailDisplayName, senderEmail) },
                To = { MailboxAddress.Parse(recipientEmail) },
                Subject = subject,
                Body = new TextPart("plain") { Text = body }
            };
        }

        private static MailKit.Net.Smtp.SmtpClient CreateMailKitClient(string host)
        {
            return new MailKit.Net.Smtp.SmtpClient
            {
                Timeout = 15000,
                CheckCertificateRevocation = false,
                ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) =>
                        ValidateServerCertificate(host, certificate, sslPolicyErrors)
            };
        }

        private static void ConfigureLegacySmtpClient(
            SmtpClient client,
            string senderEmail,
            string senderPassword,
            string smtpHost,
            int smtpPort,
            bool enableSsl)
        {
            client.Host = smtpHost;
            client.Port = GetLegacySmtpPort(smtpPort);
            client.EnableSsl = enableSsl;
            client.Credentials = new NetworkCredential(senderEmail, senderPassword);
            client.Timeout = 15000;
        }

        private static int GetLegacySmtpPort(int smtpPort)
        {
            return smtpPort == 465 ? 587 : smtpPort;
        }

        private static void RunWithTemporaryCertificateValidationBypass(Action sendAction)
        {
            var previousCertificateCallback = ServicePointManager.ServerCertificateValidationCallback;

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (_, __, ___, ____) => true;
                sendAction();
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = previousCertificateCallback;
            }
        }

        private static string NormalizeSmtpPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return string.Empty;
            }

            return new string(password
                .Where(character => !char.IsWhiteSpace(character))
                .ToArray());
        }

        private static bool IsPlaceholderSmtpPassword(string password)
        {
            return string.Equals(password, "CHANGE_ME", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TrySendWebhookEmail(Order order, string senderEmail, string webhookUrl)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                return false;
            }

            try
            {
                var payload = BuildWebhookPayload(order, senderEmail);
                var request = (HttpWebRequest)WebRequest.Create(webhookUrl);
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Timeout = 15000;

                using (var requestStream = new StreamWriter(request.GetRequestStream(), Encoding.UTF8))
                {
                    requestStream.Write(payload);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    TraceEmailIssue(
                        $"Order email notification sent via webhook. Recipient={order.User.Email}; StatusCode={(int)response.StatusCode}");
                }

                return true;
            }
            catch (Exception ex)
            {
                TraceEmailIssue(
                    $"Order email webhook attempt failed. Recipient={order?.User?.Email}; Url={webhookUrl}; Exception={ex.Message}");
                return false;
            }
        }

        private static string BuildWebhookPayload(Order order, string senderEmail)
        {
            var subject = BuildStatusChangedSubject(order);
            var body = BuildStatusChangedBody(order);
            var displayName = Properties.Settings.Default.NotificationEmailDisplayName;

            return "{" +
                   $"\"to\":\"{EscapeJson(order.User.Email)}\"," +
                   $"\"subject\":\"{EscapeJson(subject)}\"," +
                   $"\"text\":\"{EscapeJson(body)}\"," +
                   $"\"fromName\":\"{EscapeJson(displayName)}\"," +
                   $"\"fromEmail\":\"{EscapeJson(senderEmail)}\"" +
                   "}";
        }

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length + 16);

            foreach (var character in value)
            {
                switch (character)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(character))
                        {
                            builder.Append("\\u");
                            builder.Append(((int)character).ToString("x4"));
                        }
                        else
                        {
                            builder.Append(character);
                        }

                        break;
                }
            }

            return builder.ToString();
        }

        private static void EnsureModernTls()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        private static bool ValidateServerCertificate(string host, X509Certificate certificate, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            if (!string.Equals(host, "smtp.gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return certificate != null;
        }

        private static void TraceEmailIssue(string message)
        {
            Debug.WriteLine(message);

            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email-notification.log");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}", Encoding.UTF8);
            }
            catch
            {
                // Intentionally ignore logging failures so they do not affect the app flow.
            }
        }

        private sealed class SmtpConnectionAttempt
        {
            public SmtpConnectionAttempt(string host, int port, SecureSocketOptions socketOptions)
            {
                Host = host;
                Port = port;
                SocketOptions = socketOptions;
            }

            public string Host { get; }
            public int Port { get; }
            public SecureSocketOptions SocketOptions { get; }
        }

        private sealed class NotificationEmailSettings
        {
            public NotificationEmailSettings(
                string senderEmail,
                string senderPassword,
                string smtpHost,
                int smtpPort,
                bool enableSsl,
                string webhookUrl)
            {
                SenderEmail = senderEmail;
                SenderPassword = senderPassword;
                SmtpHost = smtpHost;
                SmtpPort = smtpPort;
                EnableSsl = enableSsl;
                WebhookUrl = webhookUrl;
            }

            public string SenderEmail { get; }
            public string SenderPassword { get; }
            public string SmtpHost { get; }
            public int SmtpPort { get; }
            public bool EnableSsl { get; }
            public string WebhookUrl { get; }
        }
    }
}
