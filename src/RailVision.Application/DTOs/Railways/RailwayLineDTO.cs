namespace RailVision.Application.DTOs.Railways
{
    public record RailwayLineDTO
    {
        public long Id { get; set; }
        public IEnumerable<CoordinateDTO> Coordinates { get; set; } = [];
    }

}
