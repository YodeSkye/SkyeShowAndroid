
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

            // SAVE SELECTED USER
            UserPicker.SelectedIndexChanged += async (s, e) =>
            {
                if (UserPicker.SelectedItem is JellyfinUser user)
                {
                    Preferences.Set("JellyfinUserId", user.Id);
                    Preferences.Set("JellyfinUsername", user.Name);
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
            await LoadJellyfinUsersAsync();
        }

        private void ThemePicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedTheme = (SkyeTheme)ThemePicker.SelectedIndex;

            _themeService.SetTheme(selectedTheme);
            Preferences.Set("AppTheme", ThemePicker.SelectedIndex);
        }
        private async void OnSaveConnectionClicked(object? sender, EventArgs e)
        {
            Preferences.Set("ServerIP", IpEntry.Text);
            Preferences.Set("ServerPort", PortEntry.Text);
            Preferences.Set("JellyfinApiKey", ApiKeyEntry.Text);

            await DisplayAlertAsync("Saved", "Connection settings updated.", "OK");
            await LoadJellyfinUsersAsync();
        }

        private void OnThemeChanged(SkyeTheme theme)
        {
            ((App)Application.Current!).ApplyTheme(theme);
        }

        private async Task LoadJellyfinUsersAsync()
        {
            System.Diagnostics.Debug.WriteLine("JF DEBUG: ---- LoadJellyfinUsersAsync START ----");

            try
            {
                // READ FROM PREFERENCES (not UI)
                string ip = Preferences.Get("ServerIP", "");
                string port = Preferences.Get("ServerPort", "");
                string apiKey = Preferences.Get("JellyfinApiKey", "");

               // System.Diagnostics.Debug.WriteLine($"JF DEBUG: ip={ip}, port={port}, key={apiKey}");

                if (string.IsNullOrWhiteSpace(ip) ||
                    string.IsNullOrWhiteSpace(port) ||
                    string.IsNullOrWhiteSpace(apiKey))
                {
                    //System.Diagnostics.Debug.WriteLine("JF DEBUG: Missing IP/Port/Key — aborting user load");
                    return;
                }

                string url = $"http://{ip}:{port}/Users?api_key={apiKey}";
                //System.Diagnostics.Debug.WriteLine($"JF DEBUG URL: {url}");

                using var client = new HttpClient();

                string json;
                try
                {
                    json = await client.GetStringAsync(url);
                }
                catch (Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine($"JF DEBUG HTTP ERROR: {ex}");
                    await DisplayAlertAsync("Error", $"HTTP error: {ex.Message}", "OK");
                    return;
                }

                //System.Diagnostics.Debug.WriteLine("JF DEBUG: Raw JSON received:");
                //System.Diagnostics.Debug.WriteLine(json.Length > 300 ? json.Substring(0, 300) + "..." : json);

                List<JellyfinUser>? users = null;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Try list
                try
                {
                    users = JsonSerializer.Deserialize<List<JellyfinUser>>(json, options);
                    //System.Diagnostics.Debug.WriteLine($"JF DEBUG: Parsed list, count={users?.Count ?? 0}");
                }
                catch (Exception exList)
                {
                    System.Diagnostics.Debug.WriteLine($"JF DEBUG: List parse failed: {exList.Message}");
                }

                // Try single object
                if (users == null || users.Count == 0)
                {
                    try
                    {
                        var single = JsonSerializer.Deserialize<JellyfinUser>(json, options);
                        if (single != null)
                        {
                            users = [single];
                            //System.Diagnostics.Debug.WriteLine("JF DEBUG: Parsed single user object");
                        }
                    }
                    catch (Exception exSingle)
                    {
                        System.Diagnostics.Debug.WriteLine($"JF DEBUG: Single parse failed: {exSingle.Message}");
                    }
                }

                // Still nothing?
                if (users == null || users.Count == 0)
                {
                    //System.Diagnostics.Debug.WriteLine("JF DEBUG: No users returned — clearing picker");

                    UserPicker.ItemsSource = null;
                    UserPicker.SelectedItem = null;
                    UserPicker.SelectedIndex = -1;

                    Preferences.Remove("JellyfinUserId");
                    Preferences.Remove("JellyfinUsername");

                    await DisplayAlertAsync("Error", "No users returned from Jellyfin.", "OK");
                    return;
                }

                // SUCCESS
                //System.Diagnostics.Debug.WriteLine($"JF DEBUG: SUCCESS — Loaded {users.Count} users");

                UserPicker.ItemsSource = users;
                UserPicker.ItemDisplayBinding = new Binding("Name");

                string savedId = Preferences.Get("JellyfinUserId", "");
                if (!string.IsNullOrEmpty(savedId))
                {
                    var match = users.FirstOrDefault(u => u.Id == savedId);
                    if (match != null)
                    {
                        UserPicker.SelectedItem = match;
                        //System.Diagnostics.Debug.WriteLine($"JF DEBUG: Auto-selected saved user: {match.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine($"JF DEBUG: OUTER EXCEPTION: {ex}");

                UserPicker.ItemsSource = null;
                UserPicker.SelectedItem = null;
                UserPicker.SelectedIndex = -1;

                Preferences.Remove("JellyfinUserId");
                Preferences.Remove("JellyfinUsername");

                await DisplayAlertAsync("Error", $"Failed to load users: {ex.Message}", "OK");
            }

            //System.Diagnostics.Debug.WriteLine("JF DEBUG: ---- LoadJellyfinUsersAsync END ----");
        }
    }
    public class JellyfinUser
    {
        public string Name { get; set; } = "";
        public string Id { get; set; } = "";
    }
}
