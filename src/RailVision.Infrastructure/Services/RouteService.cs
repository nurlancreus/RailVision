using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions;
using RailVision.Application.DTOs;
using RailVision.Application.DTOs.Route;

namespace RailVision.Infrastructure.Services
{
    public class RouteService(IStationService stationService, IRailwayService railwayService, ILogger<RouteService> logger) : IRouteService
    {
        private readonly IStationService _stationService = stationService;
        private readonly IRailwayService _railwayService = railwayService;
        private readonly ILogger<RouteService> _logger = logger;
        private static readonly Dictionary<long, StationDTO> _stationLookup = [];

        public Task<RouteResponseDTO> DrawOptimalRouteAsync(RouteRequestDTO request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
