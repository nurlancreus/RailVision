using System.Text.Json.Serialization;

namespace RailVision.Application.DTOs.Overpass
{
    public record ElementDTO
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("lat")]
        public double? Lat { get; set; }
        [JsonPropertyName("lon")]
        public double? Lon { get; set; }
        [JsonPropertyName("tags")]
        public Dictionary<string, string> Tags { get; set; } = [];
        [JsonPropertyName("nodes")]
        public List<long> Nodes { get; set; } = [];
    }

}
