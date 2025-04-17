using System.ComponentModel.DataAnnotations;

namespace MeteoApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public required string Nome { get; set; }

        [Required]
        public required string Cognome { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
