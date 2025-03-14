namespace RailVision.Application.DTOs
{
    public record ObstacleDTO
    {
        public long ElementId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; 
        public CoordinateDTO? Coordinate { get; set; }
        public ICollection<CoordinateDTO> Coordinates { get; set; } = [];
    }
}
