
using CommunityToolkit.Maui.Views;

namespace SkyeShowAndroid
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void PlayClicked(object sender, EventArgs e)
        {
            //await DisplayAlertAsync("Test", "PLAY CLICKED fired.", "OK");

            Console.WriteLine("PLAY CLICKED");

            //string url = "/storage/emulated/0/Download/MarlingYoga 133.mp4";
            string url = "/storage/emulated/0/Download/LiveMe SunnyGirl 20220419.mp4";
            Player.Source = MediaSource.FromUri(url);
            await Task.Delay(50);
            Player.Play();
        }

    }
}
