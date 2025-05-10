using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Route;
using RailVision.Application.Abstractions.PathFinding;
using RailVision.Application;
using RailVision.Application.Abstractions.Cache;
using RailVision.Domain;
using Microsoft.AspNetCore.Hosting;

namespace RailVision.Infrastructure.Services
{
    public class RouteService(IPopulationCenterService populationCenterService, IPathFindingStrategy pathFindingStrategy, ICacheManagement cacheManagement, IWebHostEnvironment webHostEnvironment) : IRouteService
    {
        private readonly IPopulationCenterService _populationCenterService = populationCenterService;
        private readonly IPathFindingStrategy _pathfindingStrategy = pathFindingStrategy;
        private readonly ICacheManagement _cacheManagement = cacheManagement;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

        public async Task<RouteResponseDTO> DrawOptimalRouteAsync(RouteRequestDTO request, CancellationToken cancellationToken = default)
        {
            var cacheKey = string.Empty;

            if (request.FromCoordinate != null && request.ToCoordinate != null)
            {
                cacheKey = $"Route_{request.FromCoordinate}_{request.ToCoordinate}";
            }
            else if (request.FromId != null && request.ToId != null)
            {
                cacheKey = $"Route_{request.FromId}_{request.ToId}";
            }

            var cachedResponse = await _cacheManagement.GetCachedDataByKeyAsync<RouteResponseDTO>(cacheKey, cancellationToken);

            if (cachedResponse != null) return cachedResponse;

            var populationCenters = await _populationCenterService.GetAllAsync(searchQuery: null, minPopulation: null, maxPopulation: null, isDistinct: true, cancellationToken);

            var graph = new Graph(populationCenters, request);

            var path = _pathfindingStrategy.FindPath(graph, Constants.SourceNode, Constants.TargetNode, _webHostEnvironment);

            if (path.Count == 0)
            {
                path = graph.FindRouteWithDynamicThreshold(_pathfindingStrategy, _webHostEnvironment);
            }

            if (path.Count == 0) throw new Exception("No path found");

            var populationCentersInPath = path
                .Where(k => k.node != Constants.SourceNode && k.node != Constants.TargetNode)
                .Select(k => new PathDTO
                {
                    Name = k.node,
                    Distance = k.distance,
                    Coordinate = new Application.DTOs.CoordinateDTO
                    {
                        Longitude = graph.Nodes[k.node].X,
                        Latitude = graph.Nodes[k.node].Y
                    }
                }).ToList();

            var distance = path.Select(k => k.distance).Sum();

            const double AVERAGE_SPEED_MKH = 100;

            double durationHours = distance / AVERAGE_SPEED_MKH;

            TimeSpan duration = TimeSpan.FromHours(durationHours);

            var response = new RouteResponseDTO
            {
                Route = path.Select(k => new Application.DTOs.CoordinateDTO { Longitude = graph.Nodes[k.node].X, Latitude = graph.Nodes[k.node].Y }).ToList(),
                Path = populationCentersInPath,
                Distance = distance,
                ApproximateDuration = duration
            };

            await _cacheManagement.SetDataAsync(cacheKey, response, absoluteExpirationRelativeToNow: TimeSpan.FromDays(1), cancellationToken: cancellationToken);

            return response;
        }
    }
}
