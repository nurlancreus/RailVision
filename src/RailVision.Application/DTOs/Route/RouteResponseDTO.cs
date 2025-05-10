using NetTopologySuite.Geometries;

namespace RailVision.Application.DTOs.Route
{
    public record RouteResponseDTO
    {
        public IEnumerable<CoordinateDTO> Route { get; set; } = [];
        public IEnumerable<PathDTO> Path { get; set; } = [];
        public TimeSpan ApproximateDuration { get; set; }
        public double Distance { get; set; }
    }

    public record PathDTO
    {
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        public CoordinateDTO Coordinate { get; set; } = null!;
    }
}
