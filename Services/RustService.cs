using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;


namespace DiscordBot.Services
{
    public class RustService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKeyRust;
        private const string BaseUrl = "https://api.battlemetrics.com";



        public RustService(string apiKeyRust)
        {
            _apiKeyRust = apiKeyRust;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKeyRust);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));
        }




        public async Task<int> GetPlayerCountFromSteviousMain(long serverId)
        {
            var endpoint = $"{BaseUrl}/servers/{serverId}";
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(json);

            var players = root["data"]?["attributes"]?["players"]?.Value<int>() ?? -1;
            return players;
        }
    }
}

// https://www.battlemetrics.com/servers/rust/2569573