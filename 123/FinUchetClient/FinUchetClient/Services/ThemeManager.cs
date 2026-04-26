using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FinUchetClient.Services
{
    public static class ThemeManager
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FinUchetClient", "theme.xml");

        public static bool IsDarkTheme { get; private set; }

        public static void ApplyTheme(bool isDark)
        {
            IsDarkTheme = isDark;
            SaveThemePreference(isDark);

            var app = Application.Current;
            if (app == null) return;

            // Находим и удаляем старые словари темы
            var toRemove = app.Resources.MergedDictionaries
                .Where(d => d.Source != null && d.Source.ToString().Contains("Theme"))
                .ToList();

            foreach (var dict in toRemove)
                app.Resources.MergedDictionaries.Remove(dict);

            // Создаем новый словарь ресурсов с цветами
            var themeDict = new ResourceDictionary();

            if (isDark)
            {
                themeDict.Add("BackgroundColor", new SolidColorBrush(Color.FromRgb(45, 45, 48)));
                themeDict.Add("ForegroundColor", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                themeDict.Add("CardBackground", new SolidColorBrush(Color.FromRgb(62, 62, 66)));
                themeDict.Add("SidebarBackground", new SolidColorBrush(Color.FromRgb(30, 30, 35)));
                themeDict.Add("BorderColor", new SolidColorBrush(Color.FromRgb(85, 85, 85)));
                themeDict.Add("HeaderBackground", new SolidColorBrush(Color.FromRgb(37, 37, 38)));
            }
            else
            {
                themeDict.Add("BackgroundColor", new SolidColorBrush(Color.FromRgb(245, 247, 250)));
                themeDict.Add("ForegroundColor", new SolidColorBrush(Color.FromRgb(44, 62, 80)));
                themeDict.Add("CardBackground", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                themeDict.Add("SidebarBackground", new SolidColorBrush(Color.FromRgb(44, 62, 80)));
                themeDict.Add("BorderColor", new SolidColorBrush(Color.FromRgb(224, 224, 224)));
                themeDict.Add("HeaderBackground", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
            }

            app.Resources.MergedDictionaries.Add(themeDict);

            // Обновляем все открытые окна
            UpdateAllWindows();
        }

        private static void UpdateAllWindows()
        {
            foreach (Window window in Application.Current.Windows)
            {
                UpdateWindowColors(window);

                // Если это MainWindow, обновляем боковую панель
                if (window is Views.MainWindow mainWin)
                {
                    var sidebar = mainWin.FindName("SidebarPanel") as System.Windows.Controls.Border;
                    if (sidebar != null)
                    {
                        sidebar.Background = IsDarkTheme ?
                            new SolidColorBrush(Color.FromRgb(30, 30, 35)) :
                            new SolidColorBrush(Color.FromRgb(44, 62, 80));
                    }
                }
            }
        }

        private static void UpdateWindowColors(Window window)
        {
            if (IsDarkTheme)
            {
                window.Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));
                window.Foreground = Brushes.White;
            }
            else
            {
                window.Background = new SolidColorBrush(Color.FromRgb(245, 247, 250));
                window.Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80));
            }
        }

        public static void LoadThemePreference()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var content = File.ReadAllText(SettingsPath);
                    IsDarkTheme = content.Contains("Dark");
                }
                else
                {
                    IsDarkTheme = false;
                }
                ApplyTheme(IsDarkTheme);
            }
            catch
            {
                ApplyTheme(false);
            }
        }

        private static void SaveThemePreference(bool isDark)
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllText(SettingsPath, isDark ? "Dark" : "Light");
            }
            catch { }
        }
    }
}