using Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CyFi.Models
{
    public class CyFiGameSettings : AppSettings
    {
        [JsonPropertyName("Levels")]
        public List<Map> Levels { get; set; }

        [JsonPropertyName("NumberOfPlayers")]
        [Range(1, 4, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int NumberOfPlayers { get; set; }

        [JsonPropertyName("TickTimer")]
        [Range(1, 10000, ErrorMessage = "Value for {0} must be between {1} and {2} seconds.")]
        public int TickTimer { get; set; }

        [JsonPropertyName("Max Ticks")]
        [Range(1, 500000, ErrorMessage = "Value for {0} must be between {1} and {2} seconds.")]
        public int MaxTicks { get; set; }

        [JsonPropertyName("Collectables")]
        [Range(1, 4, ErrorMessage = "Value for {0} must be between {1} and {2} seconds.")]
        public int[] Collectables { get; set; }
    }

    public class Map
    {
        [JsonPropertyName("Width")]
        public int Width { get; set; }

        [JsonPropertyName("Height")]
        public int Height { get; set; }

        [JsonPropertyName("Seed")]
        public int Seed { get; set; }
    }
}
