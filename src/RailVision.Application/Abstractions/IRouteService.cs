using RailVision.Application.DTOs.Route;

namespace RailVision.Application.Abstractions
{
    public interface IRouteService
    {
        Task<RouteResponseDTO> DrawOptimalRouteAsync(RouteRequestDTO request, CancellationToken cancellationToken = default);
    }
}
