
using CommunityToolkit.Maui.Views;
using SkyeShowAndroid.Services;

namespace SkyeShowAndroid
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // Format: "Skye Show Android v1.0"
            AppHeaderLabel.Text = $"Skye Show Android v{AppInfo.Current.VersionString} b{AppInfo.Current.BuildString}";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            JellyfinPlayer.VideoChanged += OnVideoChanged;
        }

        protected override void OnDisappearing()
        {
            JellyfinPlayer.VideoChanged -= OnVideoChanged;
            base.OnDisappearing();
        }

        private void OnVideoChanged(JellyfinItem? item)
        {
            if (item == null || string.IsNullOrEmpty(item.Path))
                return;

            var display = TextHelpers.TrimLeftToFit(item.Path, NowPlayingLabel);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                NowPlayingLabel.Text = display;
                NowPlayingLabel.IsVisible = true;
            });
        }

        private async void OnPlayClicked(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PLAY BUTTON CLICKED");

            var url = await JellyfinPlayer.GetRandomVideoUrlAsync();
            if (url == null)
                return;

            System.Diagnostics.Debug.WriteLine("Jellyfin URL: " + url);

            Player.Source = MediaSource.FromUri(url);

            await Task.Delay(50);

            Player.Play();
        }

        private async void OnFullscreenClicked(object? sender, EventArgs e)
        {
            if (Player.Source is UriMediaSource uri && uri.Uri is not null)
            {
                Player.Pause();

                string currentUrl = uri.Uri.ToString();

                await Navigation.PushAsync(new FullscreenPage(currentUrl, JellyfinPlayer.GetRandomVideoUrlAsync));
            }
        }

        private async void OnSettingsClicked(object? sender, EventArgs e)
        {
            var settingsPage = ServiceHelper.GetService<SettingsPage>();
            await Navigation.PushAsync(settingsPage);
        }
    }
}
