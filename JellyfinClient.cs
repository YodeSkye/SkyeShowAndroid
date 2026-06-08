
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SkyeShowAndroid
{
    public static class JellyfinClient
    {
        private static readonly string Server = "http://10.0.0.212:8096";
        private static readonly string ApiKey = "43b8598fee8d4cd59f6bd29b905c1ac5";
        private static readonly string UserId = "57519fba855148cdae76a7efb6fe8eba";

        public static async Task<List<string>> GetVideoIdsAsync()
        {
            System.Diagnostics.Debug.WriteLine("ENTER GetVideoIdsAsync");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Emby-Token", ApiKey);

            string url = $"{Server}/Users/{UserId}/Items?IncludeItemTypes=Video&Recursive=true";
            System.Diagnostics.Debug.WriteLine("REQUEST: " + url);

            var response = await client.GetAsync(url);
            System.Diagnostics.Debug.WriteLine("STATUS: " + (int)response.StatusCode + " " + response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine("BODY: " + json);

            try
            {
                var result = JsonSerializer.Deserialize<JellyfinItemsResponse>(json);
                var ids = result?.Items?
                    .Where(i => i.Id != null)
                    .Select(i => i.Id!)
                    .ToList()
                    ?? [];

                System.Diagnostics.Debug.WriteLine("PARSED IDS: " + ids.Count);
                return ids;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("JSON ERROR: " + ex);
                return [];
            }
        }
    }
    public class JellyfinItemsResponse
    {
        public List<JellyfinItem>? Items { get; set; }
    }

    public class JellyfinItem
    {
        public string? Id { get; set; }
    }
    public static class JellyfinPlayer
    {
        private static readonly string Server = "http://10.0.0.212:8096";
        private static readonly string ApiKey = "43b8598fee8d4cd59f6bd29b905c1ac5";

        public static async Task<string?> GetRandomVideoUrlAsync()
        {
            var ids = await JellyfinClient.GetVideoIdsAsync();
            if (ids.Count == 0)
                return null;

            var randomId = ids[Random.Shared.Next(ids.Count)];

            // Build the direct stream URL
            return $"{Server}/Videos/{randomId}/stream.mp4?container=mp4&videoCodec=h264&audioCodec=aac&api_key={ApiKey}";
        }
    }
}
