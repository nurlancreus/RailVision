using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Route;
using RailVision.Application.DTOs;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Application.Abstractions.PathFinding;
using RailVision.Application;

namespace RailVision.Infrastructure.Services
{
    public class RouteService(IRailwayService railwayService, IStationService stationService, IPopulationCenterService populationCenterService, ITerrainService terrainService, IPathFindingStrategy pathFindingStrategy) : IRouteService
    {
        private readonly IRailwayService _railwayService = railwayService;
        private readonly IStationService _stationService = stationService;
        private readonly IPopulationCenterService _populationCenterService = populationCenterService;
        private readonly ITerrainService _terrainService = terrainService;
        private readonly IPathFindingStrategy _pathfindingStrategy = pathFindingStrategy;

        public async Task<object> DrawOptimalRouteAsync(RouteRequestDTO request, CancellationToken cancellationToken = default)
        {
            var stations = await _stationService.GetAllAsync(cancellationToken);
            var populationCenters = await _populationCenterService.GetAllAsync(searchQuery: null, minPopulation: null, maxPopulation: null, cancellationToken);

            var graph = new Graph(stations, populationCenters, request);

            var path = graph.FindRouteWithDynamicThreshold(_pathfindingStrategy);

            if (path.Count == 0) return new { Success = false, Message = "No path found." };

            var populationCentersInPath = path
                .Where(k => k != "source" && k != "target")
                .Select(k => new
                {
                    Name = k,
                    Coordinate = graph.Nodes[k]
                }).ToList();

            return new
            {
                Path = path.Select(k => graph.Nodes[k]).ToList(),
                PopulationCenters = populationCentersInPath
            };
        }
    }
}
