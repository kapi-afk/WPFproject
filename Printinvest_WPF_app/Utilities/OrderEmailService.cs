using MailKit.Security;
using MimeKit;
using Printinvest_WPF_app.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Printinvest_WPF_app.Utilities
{
    public static class OrderEmailService
    {
        public static void TrySendOrderStatusChangedEmail(Order order, OrderStatus previousStatus)
        {
            try
            {
                if (order?.User == null ||
                    string.IsNullOrWhiteSpace(order.User.Email) ||
                    previousStatus == order.Status)
                {
                    return;
                }

                var senderEmail = Properties.Settings.Default.NotificationEmailAddress;
                var senderPassword = NormalizeSmtpPassword(Properties.Settings.Default.NotificationEmailPassword);
                var smtpHost = Properties.Settings.Default.NotificationSmtpHost;
                var smtpPort = Properties.Settings.Default.NotificationSmtpPort;
                var enableSsl = Properties.Settings.Default.NotificationEnableSsl;
                if (string.IsNullOrWhiteSpace(senderEmail) ||
                    string.IsNullOrWhiteSpace(senderPassword) ||
                    IsPlaceholderSmtpPassword(senderPassword))
                {
                    TraceEmailIssue("Order email notification skipped: SMTP settings are not configured.");
                    return;
                }

                EnsureModernTls();

                var message = BuildMessage(order, previousStatus, senderEmail);
                var attempts = BuildConnectionAttempts(smtpHost, smtpPort, enableSsl);
                Exception lastException = null;

                foreach (var attempt in attempts)
                {
                    try
                    {
                        using (var client = new MailKit.Net.Smtp.SmtpClient())
                        {
                            client.Timeout = 15000;
                            client.CheckCertificateRevocation = false;
                            client.ServerCertificateValidationCallback =
                                (sender, certificate, chain, sslPolicyErrors) =>
                                    ValidateServerCertificate(attempt.Host, certificate, sslPolicyErrors);
                            client.Connect(attempt.Host, attempt.Port, attempt.SocketOptions);
                            client.Authenticate(senderEmail, senderPassword);
                            client.Send(message);
                            client.Disconnect(true);
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

                throw lastException ?? new InvalidOperationException("SMTP connection attempts failed.");
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

        private static string BuildStatusChangedBody(Order order, OrderStatus previousStatus)
        {
            var customerName = string.IsNullOrWhiteSpace(order.User?.Name)
                ? "\u043a\u043b\u0438\u0435\u043d\u0442"
                : order.User.Name;
            var deviceName = string.Join(" ", new[] { order.DeviceType, order.DeviceBrand, order.DeviceModel }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
            var masterName = order.AssignedMaster?.Name ?? "\u0435\u0449\u0435 \u043d\u0435 \u043d\u0430\u0437\u043d\u0430\u0447\u0435\u043d";

            return
                $"\u0417\u0434\u0440\u0430\u0432\u0441\u0442\u0432\u0443\u0439\u0442\u0435, {customerName}!{Environment.NewLine}{Environment.NewLine}" +
                $"\u0421\u0442\u0430\u0442\u0443\u0441 \u0432\u0430\u0448\u0435\u0439 \u0437\u0430\u044f\u0432\u043a\u0438 \u2116{order.Id} \u0438\u0437\u043c\u0435\u043d\u0438\u043b\u0441\u044f.{Environment.NewLine}" +
                $"\u0423\u0441\u0442\u0440\u043e\u0439\u0441\u0442\u0432\u043e: {deviceName}{Environment.NewLine}" +
                $"\u0411\u044b\u043b\u043e: {GetStatusText(previousStatus)}{Environment.NewLine}" +
                $"\u0421\u0442\u0430\u043b\u043e: {GetStatusText(order.Status)}{Environment.NewLine}" +
                $"\u041c\u0430\u0441\u0442\u0435\u0440: {masterName}{Environment.NewLine}{Environment.NewLine}" +
                "\u042d\u0442\u043e \u0430\u0432\u0442\u043e\u043c\u0430\u0442\u0438\u0447\u0435\u0441\u043a\u043e\u0435 \u0443\u0432\u0435\u0434\u043e\u043c\u043b\u0435\u043d\u0438\u0435 \u0441\u0435\u0440\u0432\u0438\u0441\u043d\u043e\u0433\u043e \u0446\u0435\u043d\u0442\u0440\u0430.";
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

        private static MimeMessage BuildMessage(Order order, OrderStatus previousStatus, string senderEmail)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(Properties.Settings.Default.NotificationEmailDisplayName, senderEmail));
            message.To.Add(MailboxAddress.Parse(order.User.Email));
            message.Subject = $"\u0421\u0442\u0430\u0442\u0443\u0441 \u0437\u0430\u044f\u0432\u043a\u0438 \u2116{order.Id} \u0438\u0437\u043c\u0435\u043d\u0435\u043d";
            message.Body = new TextPart("plain")
            {
                Text = BuildStatusChangedBody(order, previousStatus)
            };
            return message;
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

            if (certificate == null)
            {
                return false;
            }

            return sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors;
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
    }
}
