
using SkyeShowAndroid.Services;

namespace SkyeShowAndroid
{
    public partial class SettingsPage : ContentPage
    {
        private readonly ThemeService _themeService;

        public SettingsPage(ThemeService themeService)
        {
            InitializeComponent();

            _themeService = themeService;

            // 1. Attach event FIRST
            ThemePicker.SelectedIndexChanged += ThemePicker_SelectedIndexChanged;

            // 2. THEN load saved theme
            ThemePicker.SelectedIndex = (int)_themeService.CurrentTheme;

            // 3. Listen for theme changes
            _themeService.ThemeChanged += OnThemeChanged;

            // 4. Apply theme immediately
            ((App)Application.Current!).ApplyTheme(_themeService.CurrentTheme);
        }

        private void ThemePicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedTheme = (SkyeTheme)ThemePicker.SelectedIndex;

            _themeService.SetTheme(selectedTheme);
            Preferences.Set("AppTheme", ThemePicker.SelectedIndex);
        }

        private void OnThemeChanged(SkyeTheme theme)
        {
            ((App)Application.Current!).ApplyTheme(theme);
        }
    }
}
