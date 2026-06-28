
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using static Android.Provider.MediaStore;

namespace SkyeShowAndroid
{
    public class JellyfinItem
    {
        public string? Id { get; set; }
        public string? Path { get; set; }
        public string? Name { get; set; }
    }
    public class JellyfinItemsResponse
    {
        public List<JellyfinItem>? Items { get; set; }
    }

    public static class JellyfinClient
    {
        private static string Server =>
            $"http://{Preferences.Get("ServerIP", "")}:{Preferences.Get("ServerPort", "")}";

        private static string ApiKey =>
            Preferences.Get("JellyfinApiKey", "");

        private static string UserId =>
            Preferences.Get("JellyfinUserId", "");

        // ⬇️ NOW RETURNS FULL ITEMS, NOT JUST IDS
        public static async Task<List<JellyfinItem>> GetVideosAsync()
        {
            // STEP 1: return cached list if we already fetched it
            if (JellyfinPlayer._cachedVideos != null && JellyfinPlayer._cachedVideos.Count > 0)
                return JellyfinPlayer._cachedVideos;
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-Emby-Token", ApiKey);

                //string url = $"{Server}/Users/{UserId}/Items?IncludeItemTypes=Video&Recursive=true";
                string url = $"{Server}/Users/{UserId}/Items?IncludeItemTypes=Video&Recursive=true&Fields=Path,MediaSources";

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine("JELLYFIN JSON:\n" + json);

                try
                {
                    var result = JsonSerializer.Deserialize<JellyfinItemsResponse>(json);

                    if (result?.Items == null)
                        return [];

                    JellyfinPlayer._cachedVideos = result.Items;
                    return JellyfinPlayer._cachedVideos;
                }
                catch
                {
                    return [];
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine("NETWORK ERROR: " + ex.Message);
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlertAsync("Connection Error", "Could not reach the Jellyfin server. Check your IP and port.", "OK")
                );
                return [];
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlertAsync("General Error", ex.Message, "OK")
                );
                return [];
            }
        }
    }

    public static class JellyfinPlayer
    {
        public static List<JellyfinItem>? _cachedVideos;
        // ⬇️ GLOBAL CURRENT PATH YOU CAN USE FOR LABELS/OVERLAY
        public static string? CurrentFullPath { get; private set; }

        // OPTIONAL: event so MainPage can react when video changes
        public static event Action<string?>? VideoChanged;

        public static async Task<string?> GetRandomVideoUrlAsync()
        {
            try
            {
                string ip = Preferences.Get("ServerIP", "");
                string port = Preferences.Get("ServerPort", "");
                string apiKey = Preferences.Get("JellyfinApiKey", "");

                if (!await CanReachHost(ip, int.Parse(port)))
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                        Shell.Current.DisplayAlertAsync("Connection Error", "Cannot reach the Jellyfin server.", "OK"));
                    return null;
                }

                // ⬇️ GET FULL ITEMS, NOT JUST IDS
                var videos = await JellyfinClient.GetVideosAsync();

                if (videos.Count == 0)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                        Shell.Current.DisplayAlertAsync("No Videos Found",
                            "Jellyfin returned no playable videos.",
                            "OK")
                    );
                    return null;
                }

                var video = videos[Random.Shared.Next(videos.Count)];

                // ⬇️ STORE REAL PATH FOR LABELS/OVERLAY
                CurrentFullPath = video.Path;
                
                // FIRE EVENT SO MAINPAGE / FULLSCREEN CAN UPDATE
                VideoChanged?.Invoke(CurrentFullPath);

                string server = $"http://{ip}:{port}";

                // ⬇️ USE video.Id FOR STREAM URL
                return $"{server}/Videos/{video.Id}/stream.mp4" +
                       $"?container=mp4&videoCodec=h264&audioCodec=aac" +
                       $"&maxVideoBitrate=50000000&api_key={apiKey}";
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlertAsync("Playback Error",
                        ex.ToString(),
                        "OK")
                );
                return null;
            }
        }

        public static async Task<bool> CanReachHost(string ip, int port)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, port);
                var timeoutTask = Task.Delay(1500);

                var completed = await Task.WhenAny(connectTask, timeoutTask);

                return completed == connectTask && client.Connected;
            }
            catch
            {
                return false;
            }
        }
    }
}
