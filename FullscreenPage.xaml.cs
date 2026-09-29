
using CommunityToolkit.Maui.Views;

namespace SkyeShowAndroid
{
    public partial class FullscreenPage : ContentPage
    {
        private readonly Func<Task<string?>> _getNextVideoAsync;
        private readonly string _initialUrl;

        public FullscreenPage(string initialUrl, Func<Task<string?>> getNextVideoAsync)
        {
            InitializeComponent();

            _getNextVideoAsync = getNextVideoAsync;
            _initialUrl = initialUrl;

            Player.MediaEnded += Player_MediaEnded;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            FullscreenHelper.EnterImmersiveMode();
            DeviceDisplay.KeepScreenOn = true;

            await Task.Delay(150);
            Player.Source = MediaSource.FromUri(_initialUrl);
            await Task.Delay(50);
            Player.Play();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            FullscreenHelper.ExitImmersiveMode();
            DeviceDisplay.KeepScreenOn = false;
        }

        private async void Player_MediaEnded(object? sender, EventArgs e)
        {
            await PlayNextVideoAsync();
        }

        private async void OnSwipeNext(object? sender, SwipedEventArgs e)
        {
            await PlayNextVideoAsync();
        }

        private async Task PlayNextVideoAsync()
        {
            var next = await _getNextVideoAsync();
            if (string.IsNullOrEmpty(next))
                return;

            Player.Source = MediaSource.FromUri(next);
            Player.Play();
        }

        private async void OnSingleTap(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(JellyfinPlayer.CurrentFullPath))
                return;

            // 1. Trim path for line 1
            string displayPath = TextHelpers.TrimLeftToFit(
                JellyfinPlayer.CurrentFullPath,
                OverlayLabel
            );

            // 2. Read reliable duration from Jellyfin API metadata (or MediaElement fallback)
            TimeSpan total = (JellyfinPlayer.CurrentVideo?.Duration > TimeSpan.Zero)
                ? JellyfinPlayer.CurrentVideo.Duration
                : Player.Duration;

            string totalStr = total.Hours > 0
                ? total.ToString(@"hh\:mm\:ss")
                : total.ToString(@"mm\:ss");

            // 3. Format overlay string
            string displayTime = total > TimeSpan.Zero ? $"({totalStr})" : string.Empty;
            OverlayLabel.Text = string.IsNullOrEmpty(displayTime)
                ? displayPath
                : $"{displayPath}\n{displayTime}";

            // 4. Animate overlay fade in and out
            OverlayLabel.Opacity = 0;
            OverlayLabel.IsVisible = true;

            await OverlayLabel.FadeToAsync(1, 150);
            await Task.Delay(5000);
            await OverlayLabel.FadeToAsync(0, 150);

            OverlayLabel.IsVisible = false;
        }

        private async void OnDoubleTap(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
