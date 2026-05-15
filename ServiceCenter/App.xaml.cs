using ServiceCenter.Properties;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace ServiceCenter
{
    public partial class App : Application
    {
        public static event EventHandler LanguageChanged;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Сначала загружаем настройки
            string savedTheme = Settings.Default.AppTheme;
            string savedLanguage = Settings.Default.AppLanguage;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            try
            {
                Repositories.RepositoryManager.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(); 
                return;
            }
            // Применяем локализацию и тему
            ApplyLanguage(savedLanguage);
            ApplyTheme(savedTheme);

            base.OnStartup(e);
        }

        public static void ApplyTheme(string theme)
        {
            var normalizedTheme = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase)
                ? "Dark"
                : "Light";

            var newTheme = new ResourceDictionary
            {
                Source = new Uri($"Themes/{normalizedTheme}Theme.xaml", UriKind.Relative)
            };

            var oldThemes = Current.Resources.MergedDictionaries
                .Where(d => d.Source != null && d.Source.OriginalString.Contains("Themes/"))
                .ToList();

            foreach (var oldTheme in oldThemes)
            {
                Current.Resources.MergedDictionaries.Remove(oldTheme);
            }

            Current.Resources.MergedDictionaries.Add(newTheme);

            Current.Properties["Theme"] = normalizedTheme;
            Settings.Default.AppTheme = normalizedTheme;
            Settings.Default.Save();
        }

        public static void ApplyLanguage(string languageCode)
        {
            var culture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Удаляем старый словарь локализации, если есть
            var dictionaries = Current.Resources.MergedDictionaries;
            var oldDict = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Localization/Strings."));
            if (oldDict != null)
                dictionaries.Remove(oldDict);

            // Добавляем новый словарь локализации
            var newDict = new ResourceDictionary
            {
                Source = new Uri($"Localization/Strings.{languageCode}.xaml", UriKind.Relative)
            };
            dictionaries.Add(newDict);

            // Сохраняем выбор языка
            Settings.Default.AppLanguage = languageCode;
            Settings.Default.Save();

            // Можно сохранить язык в Application.Current.Properties для доступа из других частей приложения
            Current.Properties["Language"] = languageCode;
            LanguageChanged?.Invoke(Current, EventArgs.Empty);
        }

        public static string GetString(string key, string fallback = "")
        {
            if (Current == null)
            {
                return fallback;
            }

            return Current.TryFindResource(key)?.ToString() ?? fallback;
        }
    }
}
