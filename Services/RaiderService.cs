
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

        public RaiderService(string accessKey, string season, string region)
        {
            _accessKey = accessKey;
            _season = season;
            _region = region;
            _httpClient = new HttpClient();
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

    }
}


