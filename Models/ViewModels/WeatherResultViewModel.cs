namespace MeteoApp.Models.ViewModels
{
    public class WeatherResultViewModel
    {
        public string City { get; set; } = string.Empty;
        public string? LocationKey { get; set; }
        public string CurrentConditionsJson { get; set; } = string.Empty;
        public string ForecastJson { get; set; } = string.Empty;
        public float? TemperatureCelsius { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
