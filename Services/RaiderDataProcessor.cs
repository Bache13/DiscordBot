using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
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

        public async Task<string> GetAllPagesAndMerge()
        {
            var mergedRankings = new JArray();

            for (int page = 0; page < _page; page++)
            {
                var url = $"https://raider.io/api/v1/mythic-plus/runs?access_key={_accessKey}&season={_season}&region={_region}&dungeon={_dungeon}&page={page}";
                var response = await _httpClient.GetStringAsync(url);
                var jsonObject = JObject.Parse(response);
                var rankings = jsonObject["rankings"] as JArray;
                if (rankings != null)
                {
                    mergedRankings.Merge(rankings);
                }
            }

            var mergedObject = new JObject { ["rankings"] = mergedRankings };
            return mergedObject.ToString();
        }



        public Dictionary<string, int> GetTopSpecs(string jsonData)
        {
            var specCounts = new Dictionary<string, int>();

            var root = JObject.Parse(jsonData);
            var rankings = root["rankings"];

            if (rankings == null || !rankings.HasValues)
                return specCounts;

            // Process only the top 20 runs (if available)
            foreach (var run in rankings.Take(100))
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
                            string comboKey = $"{specName} {className}";
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

        public async Task<Dictionary<string, int>> GetTopSpecsFromAllPagesAsync()
        {
            string mergedJson = await GetAllPagesAndMerge();
            return GetTopSpecs(mergedJson);
        }

        public string FormatMostPopularGroup(Dictionary<string, int> specCounts)
        {
            var sorted = specCounts
                .OrderByDescending(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key);

            var lines = new List<string>
            {
                "**Spec frequency in top 100 runs:**" // your header
            };

            var sb = new StringBuilder();
            sb.AppendLine("**Spec frequency in top 100 runs EU:**");

            foreach (var kvp in sorted)
            {
                sb.AppendLine($"`{kvp.Key}: {kvp.Value}`");
            }

            return sb.ToString();
        }


        // Original
        // public async Task<string> GetFormattedTopSpecsAsync()
        // {
        //     var url = $"https://raider.io/api/v1/mythic-plus/runs?access_key={_accessKey}&season={_season}&region={_region}&dungeon={_dungeon}&page={_page}";
        //     var response = await _httpClient.GetAsync(url);
        //     response.EnsureSuccessStatusCode();
        //     string jsonData = await response.Content.ReadAsStringAsync();

        //     var specCounts = GetTopSpecs(jsonData);
        //     return FormatMostPopularGroup(specCounts);
        // }



        // public async Task<string> GetFormattedTopSpecsAsync()
        // {
        //     string AllPages = "{\"rankings\":[";
        //     for (int page = 0; page <= _page; page++)
        //     {
        //         var url = $"https://raider.io/api/v1/mythic-plus/runs?access_key={_accessKey}&season={_season}&region={_region}&dungeon={_dungeon}&page={page}";
        //         var response = await _httpClient.GetAsync(url);
        //         response.EnsureSuccessStatusCode();
        //         string AllPagesIngenAning = await response.Content.ReadAsStringAsync();
        //         AllPagesIngenAning = AllPagesIngenAning.Remove(0, 13);
        //         AllPagesIngenAning = AllPagesIngenAning.Remove(AllPagesIngenAning.Length - 1, 1);
        //         AllPages += "," + AllPagesIngenAning;
        //     }
        //     AllPages = AllPages.Remove(13, 1);
        //     AllPages += "}";

        //     System.Console.WriteLine(AllPages);
        //     return "Hej";

        //     // var specCounts = GetTopSpecs(AllPages);
        //     // return FormatMostPopularGroup(specCounts);
        // }
    }
}


// {"rankings":[