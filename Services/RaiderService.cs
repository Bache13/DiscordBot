
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Services
{
    public class RaiderService
    {
        private readonly string _accessKey;
        private readonly string _season;
        private readonly string _region;
        private readonly HttpClient _httpClient;
        private readonly string _dungeon;
        private readonly int _page;

        public RaiderService(string accessKey, string season, string region)
        {
            _accessKey = accessKey;
            _season = season;
            _region = region;
            _httpClient = new HttpClient();

            _dungeon = "all";
            _page = 0;
        }


        public RaiderService(string accessKey, string season, string region, string dungeon, int page)
            : this(accessKey, season, region)
        {
            _dungeon = dungeon;
            _page = page;
        }


        public async Task<double?> GetCutOffAsync()
        {
            var url = $"https://raider.io/api/v1/mythic-plus/season-cutoffs?access_key={_accessKey}&season={_season}&region={_region}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var root = JObject.Parse(json);

            var allFactionsValue = (double?)root.SelectToken("cutoffs.p999.all.quantileMinValue");

            return allFactionsValue;

        }

        public async Task<(string DungeonName, int MythicLevel)> GetHighestKeyCompletedAsync()
        {
            var url = $"https://raider.io/api/v1/mythic-plus/runs?access_key={_accessKey}&season={_season}&region={_region}&dungeon={_dungeon}&page={_page}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(json);

            // Check if "rankings" exists and has values
            var rankings = root["rankings"];
            if (rankings == null || !rankings.HasValues)
            {
                return ("Unknown Dungeon", 0);
            }

            var highestKey = rankings.FirstOrDefault();
            if (highestKey == null)
            {
                return ("Unknown Dungeon", 0);
            }

            string dungeonName = highestKey["run"]?["dungeon"]?["name"]?.Value<string>() ?? "Unknown Dungeon";
            int mythicLevel = highestKey["run"]?["mythic_level"]?.Value<int>() ?? 0;

            return (dungeonName, mythicLevel);
        }


    }
}


