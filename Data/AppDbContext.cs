using MeteoApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MeteoApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<WeatherData> WeatherData { get; set; }
    }
}
