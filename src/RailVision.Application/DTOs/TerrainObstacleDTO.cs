namespace RailVision.Application.DTOs
{
    public record TerrainObstacleDTO
    {
        public long NodeId { get; set; }
        public string Type { get; set; } = string.Empty;
        public IEnumerable<CoordinateDTO> Coordinates { get; set; } = [];
    }
}
