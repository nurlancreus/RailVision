namespace RailVision.Application.DTOs.Route
{
    public record RouteRequestDTO
    {
        public long FromStationId { get; set; }
        public long ToStationId { get; set; }
    }
}
