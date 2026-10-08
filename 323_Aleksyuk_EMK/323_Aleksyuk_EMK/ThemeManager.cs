using System;
using System.Windows;

namespace _323_Aleksyuk_EMK   // замените на namespace вашего проекта
{
    public static class ThemeManager
    {
        // MergedDictionaries[0] в App.xaml — словарь темы, [1] — общие стили
        public static void Apply(string theme)
        {
            string path = theme == "Dark" ? "Themes/Dictionary2.xaml" : "Themes/Dictionary.xaml";
            Application.Current.Resources.MergedDictionaries[0] =
                new ResourceDictionary { Source = new Uri(path, UriKind.Relative) };
        }

        public static void Toggle()
        {
            var s = Properties.Settings.Default;
            s.Theme = s.Theme == "Dark" ? "Light" : "Dark";
            s.Save();               // тема сохраняется между запусками
            Apply(s.Theme);
        }
    }
}
