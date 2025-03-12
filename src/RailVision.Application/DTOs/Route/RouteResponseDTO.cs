namespace RailVision.Application.DTOs.Route
{
    public record RouteResponseDTO
    {
        public IEnumerable<StationDTO> Route { get; set; } = [];
        public double TotalDistance { get; set; }
    }
}
