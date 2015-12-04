using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

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
                if (string.IsNullOrWhiteSpace(themeId) == false)
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
                    string.Format(
                        @"Software\Microsoft\VisualStudio\{0}\ApplicationPrivateSettings\Microsoft\VisualStudio",
                        version);

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
                {
                    if (key != null)
                    {
                        var keyText = (string)key.GetValue("ColorTheme", string.Empty);

                        if (!string.IsNullOrEmpty(keyText))
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
                string keyName = string.Format(@"Software\Microsoft\VisualStudio\{0}\{1}", version, CategoryName);

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
                {
                    if (key != null)
                    {
                        return (string)key.GetValue(ThemePropertyName, string.Empty);
                    }
                }

                return null;
            }
        }
    }
}
