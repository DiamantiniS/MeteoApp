namespace MeteoApp.Models
{
    public class WeatherData
    {
        public int Id { get; set; }
        public string? LocationName { get; set; }
        public DateTime Timestamp { get; set; }
        public double CurrentTemperature { get; set; }

        public string? Username { get; set; }
    }
}
