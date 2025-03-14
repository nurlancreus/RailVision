namespace RailVision.Application.DTOs
{
    public record RailwayLineDTO
    {
        public long ElementId { get; set; }
        public IEnumerable<CoordinateDTO> Coordinates { get; set; } = [];
    }

}
