using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Services
{
    public class RaiderDataProcessor
    {
        private readonly string _accessKey;
        private readonly string _season;
        private readonly string _region;
        private readonly string _dungeon;
        private readonly int _page;
        private readonly HttpClient _httpClient;

        public RaiderDataProcessor(string accessKey, string season, string region, string dungeon, int page)
        {
            _accessKey = accessKey;
            _season = season;
            _region = region;
            _dungeon = dungeon;
            _page = page;
            _httpClient = new HttpClient();
        }

        public Dictionary<string, int> GetTopSpecs(string jsonData)
        {
            var specCounts = new Dictionary<string, int>();

            var root = JObject.Parse(jsonData);
            var rankings = root["rankings"];

            if (rankings == null || !rankings.HasValues)
                return specCounts;

            // Process only the top 20 runs (if available)
            foreach (var run in rankings.Take(20))
            {
                var roster = run["run"]?["roster"];
                if (roster != null)
                {
                    foreach (var member in roster)
                    {
                        string? className = member["character"]?["class"]?["name"]?.Value<string>();
                        string? specName = member["character"]?["spec"]?["name"]?.Value<string>();

                        if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(specName))
                        {
                            string comboKey = $"{className} - {specName}";
                            if (specCounts.ContainsKey(comboKey))
                                specCounts[comboKey]++;
                            else
                                specCounts[comboKey] = 1;
                        }
                    }
                }
            }

            return specCounts;
        }

        public string FormatMostPopularGroup(Dictionary<string, int> specCounts)
        {
            var sorted = specCounts
                .OrderByDescending(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key);

            var lines = new List<string>
            {
                "**Spec frequency in top 20 runs:**" // your header
            };

            foreach (var kvp in sorted)
            {
                var parts = kvp.Key.Split(" - ", StringSplitOptions.RemoveEmptyEntries);
                var spec = (parts.Length > 1) ? parts[1] : parts[0];
                lines.Add($"`{spec}: {kvp.Value}`");
            }

            // Join them with a newline
            return string.Join("\n", lines);
        }

        public async Task<string> GetFormattedTopSpecsAsync()
        {
            var url = $"https://raider.io/api/v1/mythic-plus/runs?access_key={_accessKey}&season={_season}&region={_region}&dungeon={_dungeon}&page={_page}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string jsonData = await response.Content.ReadAsStringAsync();

            var specCounts = GetTopSpecs(jsonData);
            return FormatMostPopularGroup(specCounts);
        }
    }
}
