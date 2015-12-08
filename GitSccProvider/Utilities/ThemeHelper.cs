using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Win32;
using Color = System.Drawing.Color;

namespace GitSccProvider.Utilities
{
    public enum VsTheme
    {
        Unknown = 0,
        Light,
        Dark,
        Blue
    }




    public static class ThemeHelper
    {
        public static System.Windows.Media.Brush ToBrush(this System.Drawing.Color color)
        {
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private static VsTheme _currentTheme = VsTheme.Unknown;

        private static readonly IDictionary<string, VsTheme> Themes = new Dictionary<string, VsTheme>()
             {
        { "de3dbbcd-f642-433c-8353-8f1df4370aba", VsTheme.Light },
        { "1ded0138-47ce-435e-84ef-9ec1f439b749", VsTheme.Dark },
        { "a4d6a176-b948-4b29-8c66-53c97a1ed7d0", VsTheme.Blue }
            };

        public static VsTheme GetCurrentTheme(bool force = false)
        {
            if (_currentTheme == VsTheme.Unknown || force)
            {

                string themeId = GetThemeId();
                if (String.IsNullOrWhiteSpace(themeId) == false)
                {
                    VsTheme theme;
                    if (Themes.TryGetValue(themeId, out theme))
                    {
                        _currentTheme = theme;
                    }
                }
            }

            return _currentTheme;
        }

        public static string GetThemeId()
        {

            var version = SolutionExtensions.GetActiveIDE().Version;

            const string CategoryName = "General";
            const string ThemePropertyName = "CurrentTheme";

            if (version == "14.0")
            {
                string keyName =
                    String.Format(
                        @"Software\Microsoft\VisualStudio\{0}\ApplicationPrivateSettings\Microsoft\VisualStudio",
                        version);

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
                {
                    if (key != null)
                    {
                        var keyText = (string)key.GetValue("ColorTheme", String.Empty);

                        if (!String.IsNullOrEmpty(keyText))
                        {
                            var keyTextValues = keyText.Split('*');
                            if (keyTextValues.Length > 2)
                            {
                                return keyTextValues[2];
                            }
                        }
                    }
                }

                return null;
            }
            else
            {
                string keyName = String.Format(@"Software\Microsoft\VisualStudio\{0}\{1}", version, CategoryName);

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
                {
                    if (key != null)
                    {
                        return (string)key.GetValue(ThemePropertyName, String.Empty);
                    }
                }

                return null;
            }
        }
    }
}
