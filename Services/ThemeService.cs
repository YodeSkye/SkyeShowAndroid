
namespace SkyeShowAndroid.Services
{
    public class ThemeService
    {
        public SkyeTheme CurrentTheme { get; private set; } = SkyeTheme.Magenta;

        public event Action<SkyeTheme>? ThemeChanged;

        public void SetTheme(SkyeTheme theme)
        {
            CurrentTheme = theme;
            ThemeChanged?.Invoke(theme);
        }
    }
}
