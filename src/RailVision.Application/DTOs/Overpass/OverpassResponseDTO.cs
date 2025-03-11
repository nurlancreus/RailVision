using System.Text.Json.Serialization;

namespace RailVision.Application.DTOs.Overpass
{
    public class OverpassResponseDTO
    {
        [JsonPropertyName("version")]
        public double Version { get; set; }
        [JsonPropertyName("generator")]
        public string Generator { get; set; } = string.Empty;
        [JsonPropertyName("osm3s")]
        public Osm3sDTO Osm3s { get; set; } = null!;
        [JsonPropertyName("elements")]
        public List<ElementDTO> Elements { get; set; } = [];
    }

}
