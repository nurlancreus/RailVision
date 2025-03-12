namespace RailVision.Application.DTOs
{
    public record RailwayLineDTO
    {
        public long Id { get; set; }
        public IEnumerable<CoordinateDTO> Coordinates { get; set; } = [];
    }

}
