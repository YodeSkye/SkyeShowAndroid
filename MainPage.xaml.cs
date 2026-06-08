
using CommunityToolkit.Maui.Views;

namespace SkyeShowAndroid
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnPlayClicked(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PLAY BUTTON CLICKED");

            // Ask Jellyfin for a random video URL
            var url = await JellyfinPlayer.GetRandomVideoUrlAsync();
            if (url == null)
                return;

            System.Diagnostics.Debug.WriteLine("Jellyfin URL: " + url);

            // Assign to your MediaElement
            Player.Source = MediaSource.FromUri(url);

            // Give the player a moment to initialize
            await Task.Delay(50);

            Player.Play();
        }

    }
}
