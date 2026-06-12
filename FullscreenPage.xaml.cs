
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

            // Give Android time to create the SurfaceView
            await Task.Delay(150);

            Player.Source = MediaSource.FromUri(_initialUrl);

            // Give MediaElement time to bind to the surface
            await Task.Delay(50);

            Player.Play();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            FullscreenHelper.ExitImmersiveMode();
            DeviceDisplay.KeepScreenOn = false;  // Allow sleep again
        }

        private async void Player_MediaEnded(object? sender, EventArgs e)
        {
            var next = await _getNextVideoAsync();
            if (string.IsNullOrEmpty(next))
                return;

            Player.Source = MediaSource.FromUri(next);
            Player.Play();
        }
        private async void OnExitTapped(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        private async void OnSwipeNext(object sender, SwipedEventArgs e)
        {
            var next = await _getNextVideoAsync();
            if (string.IsNullOrEmpty(next))
                return;

            Player.Source = MediaSource.FromUri(next);
            Player.Play();
        }
    }
}
