
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;

namespace SkyeShowAndroid;

public class JellyfinItem
{
    public string? Id { get; set; }
    public string? Path { get; set; }
    public string? Name { get; set; }

    // Jellyfin time units (10,000 Ticks = 1 Millisecond)
    public long RunTimeTicks { get; set; }

    // Helper property to convert Ticks to System.TimeSpan
    public TimeSpan Duration => TimeSpan.FromTicks(RunTimeTicks);
}

public class JellyfinItemsResponse
{
    public List<JellyfinItem>? Items { get; set; }
}

public static class JellyfinClient
{
    private static readonly HttpClient _httpClient = new();

    private static string Server =>
        $"http://{Preferences.Get("ServerIP", "")}:{Preferences.Get("ServerPort", "")}";

    private static string ApiKey =>
        Preferences.Get("JellyfinApiKey", "");

    private static string UserId =>
        Preferences.Get("JellyfinUserId", "");

    public static async Task<List<JellyfinItem>> GetVideosAsync()
    {
        if (JellyfinPlayer._cachedVideos != null && JellyfinPlayer._cachedVideos.Count > 0)
            return JellyfinPlayer._cachedVideos;

        try
        {
            // Simplified API call: only requesting Path and RunTimeTicks
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"{Server}/Users/{UserId}/Items?IncludeItemTypes=Video&Recursive=true&Fields=Path,RunTimeTicks");

            request.Headers.Add("X-Emby-Token", ApiKey);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<JellyfinItemsResponse>(json);

            JellyfinPlayer._cachedVideos = result?.Items ?? [];

            return JellyfinPlayer._cachedVideos;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine("NETWORK ERROR: " + ex.Message);

            await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayAlertAsync("Connection Error", "Could not reach the Jellyfin server. Check your IP and port.", "OK")
            );

            JellyfinPlayer._cachedVideos = [];
            return JellyfinPlayer._cachedVideos;
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayAlertAsync("General Error", ex.Message, "OK")
            );

            JellyfinPlayer._cachedVideos = [];
            return JellyfinPlayer._cachedVideos;
        }
    }
}

public static class JellyfinPlayer
{
    internal static List<JellyfinItem>? _cachedVideos;

    public static JellyfinItem? CurrentVideo { get; private set; }
    public static string? CurrentFullPath => CurrentVideo?.Path;

    public static event Action<JellyfinItem?>? VideoChanged;

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
                    Shell.Current.DisplayAlertAsync("Connection Error", "Cannot reach the Jellyfin server.", "OK")
                );
                return null;
            }

            var videos = await JellyfinClient.GetVideosAsync();

            if (videos == null || videos.Count == 0)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlertAsync("No Videos Found", "Jellyfin returned no playable videos.", "OK")
                );
                return null;
            }

            CurrentVideo = videos[Random.Shared.Next(videos.Count)];

            VideoChanged?.Invoke(CurrentVideo);

            string server = $"http://{ip}:{port}";

            return $"{server}/Videos/{CurrentVideo.Id}/stream.mp4" +
                   $"?container=mp4&videoCodec=h264&audioCodec=aac" +
                   $"&maxVideoBitrate=50000000&api_key={apiKey}";
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayAlertAsync("Playback Error", ex.Message, "OK")
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
