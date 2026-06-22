
using SkyeShowAndroid.Services;

namespace SkyeShowAndroid
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = default!;
        private readonly ThemeService _themeService;

        public App(IServiceProvider services, ThemeService themeService)
        {
            InitializeComponent();

            Services = services;
            _themeService = themeService;

            int saved = Preferences.Get("AppTheme", (int)SkyeTheme.Magenta);
            _themeService.SetTheme((SkyeTheme)saved);

            // Apply theme at startup
            ApplyTheme(_themeService.CurrentTheme);

            // Listen for theme changes
            _themeService.ThemeChanged += ApplyTheme;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(new MainPage()));
        }

        public void ApplyTheme(SkyeTheme theme)
        {
            switch (theme)
            {
                case SkyeTheme.Light:
                    Resources["BackgroundColor"] = Colors.White;
                    Resources["TextColor"] = Colors.Black;
                    Resources["ButtonBackgroundColor"] = Color.FromArgb("#E0E0E0");
                    Resources["ButtonTextColor"] = Colors.Black;
                    break;

                case SkyeTheme.Dark:
                    Resources["BackgroundColor"] = Colors.Black;
                    Resources["TextColor"] = Colors.White;
                    Resources["ButtonBackgroundColor"] = Color.FromArgb("#333333");
                    Resources["ButtonTextColor"] = Colors.White;
                    break;

                case SkyeTheme.Magenta:
                    Resources["BackgroundColor"] = Color.FromArgb("#FF2ED1");
                    Resources["TextColor"] = Colors.White;
                    Resources["ButtonBackgroundColor"] = Color.FromArgb("#D600A8");
                    Resources["ButtonTextColor"] = Colors.White;
                    break;

                case SkyeTheme.System:
                    var system = Application.Current?.RequestedTheme;
                    ApplyTheme(system == AppTheme.Dark ? SkyeTheme.Dark : SkyeTheme.Light);
                    break;
            }
        }
    }
}
