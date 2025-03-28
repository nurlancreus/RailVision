namespace RailVision.Application.DTOs
{
    public record RailwayLineDTO
    {
        public Guid Id { get; set; }
        public long ElementId { get; set; }
        public List<CoordinateDTO> Coordinates { get; set; } = [];
    }

}
