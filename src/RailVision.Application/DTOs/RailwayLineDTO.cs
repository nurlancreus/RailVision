namespace RailVision.Application.DTOs
{
    public record RailwayLineDTO
    {
        public Guid Id { get; set; }
        public long ElementId { get; set; }
        public IEnumerable<CoordinateDTO> Coordinates { get; set; } = [];
    }

}
