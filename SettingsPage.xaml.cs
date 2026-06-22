
using SkyeShowAndroid.Services;
using System.Text.Json;

namespace SkyeShowAndroid
{
    public partial class SettingsPage : ContentPage
    {
        private readonly ThemeService _themeService;

        public SettingsPage(ThemeService themeService)
        {
            InitializeComponent();

            _themeService = themeService;

            // THEME HANDLING
            ThemePicker.SelectedIndexChanged += ThemePicker_SelectedIndexChanged;
            ThemePicker.SelectedIndex = (int)_themeService.CurrentTheme;
            _themeService.ThemeChanged += OnThemeChanged;
            ((App)Application.Current!).ApplyTheme(_themeService.CurrentTheme);

            // LOAD USERS AFTER PAGE LOADS
            //Loaded += async (_, __) => await LoadJellyfinUsersAsync();

            // SAVE SELECTED USER
            UserPicker.SelectedIndexChanged += async (s, e) =>
            {
                if (UserPicker.SelectedItem is JellyfinUser user)
                {
                    Preferences.Set("JellyfinUserId", user.Id);
                    Preferences.Set("JellyfinUsername", user.Name);
                    //// ⭐ THIS IS THE TEST YOU WANT ⭐
                    //await DisplayAlertAsync("DEBUG STORED ID", Preferences.Get("JellyfinUserId", "NOT FOUND"), "OK");
                    //await DisplayAlertAsync("DEBUG STORED UserName", Preferences.Get("JellyfinUsername", "NOT FOUND"), "OK");
                }
            };
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
           
            // LOAD SAVED CONNECTION SETTINGS
            IpEntry.Text = Preferences.Get("ServerIP", "");
            PortEntry.Text = Preferences.Get("ServerPort", "");
            ApiKeyEntry.Text = Preferences.Get("JellyfinApiKey", "");
            // Debug: prove they are NOT empty
            //await DisplayAlertAsync("DEBUG", $"IP='{IpEntry.Text}' PORT='{PortEntry.Text}' API='{ApiKeyEntry.Text}'", "OK");
            await LoadJellyfinUsersAsync();
        }

        private void ThemePicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedTheme = (SkyeTheme)ThemePicker.SelectedIndex;

            _themeService.SetTheme(selectedTheme);
            Preferences.Set("AppTheme", ThemePicker.SelectedIndex);
        }
        private void OnSaveConnectionClicked(object? sender, EventArgs e)
        {
            Preferences.Set("ServerIP", IpEntry.Text);
            Preferences.Set("ServerPort", PortEntry.Text);
            Preferences.Set("JellyfinApiKey", ApiKeyEntry.Text);

            DisplayAlertAsync("Saved", "Connection settings updated.", "OK");
        }
        private void OnThemeChanged(SkyeTheme theme)
        {
            ((App)Application.Current!).ApplyTheme(theme);
        }
        private async Task LoadJellyfinUsersAsync()
        {

            try
            {
                string ip = IpEntry.Text;
                string port = PortEntry.Text;
                string apiKey = ApiKeyEntry.Text;

               if (string.IsNullOrWhiteSpace(ip) ||
                    string.IsNullOrWhiteSpace(port) ||
                    string.IsNullOrWhiteSpace(apiKey))
                    return;

               string url = $"http://{ip}:{port}/Users?api_key={apiKey}";

                using var client = new HttpClient();
                var json = await client.GetStringAsync(url);

                // ADD THIS
                //await DisplayAlertAsync("DEBUG JSON", json, "OK");

                List<JellyfinUser>? users = null;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Try to parse as a list first
                try
                {
                    users = JsonSerializer.Deserialize<List<JellyfinUser>>(json, options);
                }
                catch
                {
                    // If that fails, try single object
                    var single = JsonSerializer.Deserialize<JellyfinUser>(json, options);
                    if (single != null)
                        users = new List<JellyfinUser> { single };
                }

                // If STILL null, bail out
                if (users == null || users.Count == 0)
                {
                    await DisplayAlertAsync("Error", "No users returned from Jellyfin.", "OK");
                    return;
                }

                UserPicker.ItemsSource = users;
                UserPicker.ItemDisplayBinding = new Binding("Name");

                // Auto-select saved user if exists
                string savedId = Preferences.Get("JellyfinUserId", "");
                if (!string.IsNullOrEmpty(savedId))
                {
                    var match = users.FirstOrDefault(u => u.Id == savedId);
                    if (match != null)
                        UserPicker.SelectedItem = match;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to load users: {ex.Message}", "OK");
            }
        }
    }
    public class JellyfinUser
    {
        public string Name { get; set; } = "";
        public string Id { get; set; } = "";
    }

}
