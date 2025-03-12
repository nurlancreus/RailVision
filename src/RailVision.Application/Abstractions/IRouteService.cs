using RailVision.Application.DTOs.Route;

namespace RailVision.Application.Abstractions
{
    public interface IRouteService
    {
        Task<RouteResponseDTO> CalculateRouteAsync(RouteRequestDTO request, CancellationToken cancellationToken = default);
    }
}
