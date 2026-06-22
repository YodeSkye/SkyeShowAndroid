
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;

namespace SkyeShowAndroid
{
    public class JellyfinItem
    {
        public string? Id { get; set; }
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

        public static async Task<List<string>> GetVideoIdsAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-Emby-Token", ApiKey);

                string url = $"{Server}/Users/{UserId}/Items?IncludeItemTypes=Video&Recursive=true";

                var response = await client.GetAsync(url);

                var json = await response.Content.ReadAsStringAsync();
                try
                {
                    var result = JsonSerializer.Deserialize<JellyfinItemsResponse>(json);

                    if (result?.Items == null)
                        return [];

                    return [.. result.Items
                        .Where(i => i.Id != null)
                        .Select(i => i.Id!)];
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
        public static async Task<string?> GetRandomVideoUrlAsync()
        {
            try
            {
                // ⭐ READ SETTINGS INSIDE METHOD — NEVER IN STATIC PROPERTIES ⭐
                string ip = Preferences.Get("ServerIP", "");
                string port = Preferences.Get("ServerPort", "");
                string apiKey = Preferences.Get("JellyfinApiKey", "");

                if (!await CanReachHost(ip, int.Parse(port)))
                {
                    await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlertAsync("Connection Error", "Cannot reach the Jellyfin server.", "OK"));
                    return null;
                }

                var ids = await JellyfinClient.GetVideoIdsAsync() ?? [];

                if (ids.Count == 0)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                        Shell.Current.DisplayAlertAsync("No Videos Found",
                            "Jellyfin returned no playable videos.",
                            "OK")
                    );
                    return null;
                }

                var randomId = ids[Random.Shared.Next(ids.Count)];

                string server = $"http://{ip}:{port}";

                return $"{server}/Videos/{randomId}/stream.mp4" +
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
