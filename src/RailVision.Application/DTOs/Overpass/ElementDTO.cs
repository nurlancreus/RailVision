using System.Text.Json.Serialization;

namespace RailVision.Application.DTOs.Overpass
{
    public record ElementDTO
    {
        public string Type { get; set; } = string.Empty;
        public long Id { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public Dictionary<string, double> Bounds { get; set; } = [];
        public ICollection<GeometryDTO> Geometry { get; set; } = [];
        public Dictionary<string, string> Tags { get; set; } = [];
        public List<long> Nodes { get; set; } = [];
    }
}
