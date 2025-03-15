namespace RailVision.Application.DTOs
{
    public record ObstacleDTO
    {
        public Guid Id { get; set; }
        public long ElementId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; 
        public CoordinateDTO? Coordinate { get; set; }
        public IEnumerable<CoordinateDTO> Coordinates { get; set; } = [];
    }
}
