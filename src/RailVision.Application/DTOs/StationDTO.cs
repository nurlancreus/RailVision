namespace RailVision.Application.DTOs
{
    public record StationDTO
    {
        public Guid Id { get; set; }
        public long ElementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public CoordinateDTO? Coordinate { get; set; }
    }
}
