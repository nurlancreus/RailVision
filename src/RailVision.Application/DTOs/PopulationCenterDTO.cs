namespace RailVision.Application.DTOs
{
    public record PopulationCenterDTO
    {
        public Guid Id { get; set; }
        public long ElementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Population { get; set; }
        public CoordinateDTO? Coordinate { get; set; }
    }
}
