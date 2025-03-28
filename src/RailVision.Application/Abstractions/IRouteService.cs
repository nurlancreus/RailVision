using RailVision.Application.DTOs.Route;

namespace RailVision.Application.Abstractions
{
    public interface IRouteService
    {
        Task<object> DrawOptimalRouteAsync(RouteRequestDTO request, CancellationToken cancellationToken = default);
    }
}
