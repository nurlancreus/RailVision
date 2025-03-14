using System.Text.Json.Serialization;

namespace RailVision.Application.DTOs.Overpass
{
    public class OverpassResponseDTO
    {
        public double Version { get; set; }
        public string Generator { get; set; } = string.Empty;
        public Osm3sDTO Osm3s { get; set; } = null!;
        public ICollection<ElementDTO> Elements { get; set; } = [];
    }
}
