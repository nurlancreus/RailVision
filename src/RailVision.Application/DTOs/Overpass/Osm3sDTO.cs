using System.Text.Json.Serialization;

namespace RailVision.Application.DTOs.Overpass
{
    public record Osm3sDTO
    {
        [JsonPropertyName("timestamp_osm_base")]
        public string TimestampOsmBase { get; set; } = string.Empty;

        [JsonPropertyName("timestamp_areas_base")]
        public string TimestampAreasBase { get; set; } = string.Empty;
        public string Copyright { get; set; } = string.Empty;
    }
}
