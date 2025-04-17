using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeteoApp.Services
{
    public class AccuWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AccuWeatherService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = "OcDJL5wmFL8EcjEQ4oJQwpqsw3AWn63c"; 
        }

        public async Task<string?> GetLocationKeyAsync(string city)
        {
            var url = $"http://dataservice.accuweather.com/locations/v1/cities/search?apikey={_apiKey}&q={city}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return doc.RootElement[0].GetProperty("Key").GetString();
        }

        public async Task<string> GetCurrentConditionsAsync(string locationKey)
        {
            var url = $"http://dataservice.accuweather.com/currentconditions/v1/{locationKey}?apikey={_apiKey}&details=true";
            var response = await _httpClient.GetStringAsync(url);
            return response;
        }

        public async Task<string> GetForecastAsync(string locationKey)
        {
            var url = $"http://dataservice.accuweather.com/forecasts/v1/daily/5day/{locationKey}?apikey={_apiKey}&metric=true";
            var response = await _httpClient.GetStringAsync(url);
            return response;
        }
    }
}
