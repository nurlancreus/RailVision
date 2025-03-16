using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions;
using RailVision.Application.DTOs;
using RailVision.Application.DTOs.Route;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RailVision.Infrastructure.Services
{
    public class RouteService(IStationService stationService, IRailwayService railwayService, ILogger<RouteService> logger) : IRouteService
    {
        private readonly IStationService _stationService = stationService;
        private readonly IRailwayService _railwayService = railwayService;
        private readonly ILogger<RouteService> _logger = logger;

        public async Task<RouteResponseDTO> DrawOptimalRouteAsync(RouteRequestDTO request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Drawing optimal route from station {StartId} to station {EndId}", request.FromStationId, request.ToStationId);

            if (request.FromStationId <= 0 || request.ToStationId <= 0)
            {
                throw new ArgumentException("Station IDs must be greater than 0.");
            }

            var stations = (await _stationService.GetAllAsync(cancellationToken)).ToList();
            var railways = (await _railwayService.GetAllAsync(cancellationToken)).ToList();

            if (stations.Count == 0 || railways.Count == 0)
            {
                throw new InvalidOperationException("Stations or Railways data not available");
            }

            var stationGraph = BuildGraph(stations, railways);

            if (!stationGraph.ContainsKey(request.FromStationId) || !stationGraph.ContainsKey(request.ToStationId))
            {
                throw new InvalidOperationException("Start or end station not found in the graph.");
            }

            var (path, totalDistance) = Dijkstra(request.FromStationId, request.ToStationId, stationGraph);

            if (path.Count == 0)
            {
                throw new InvalidOperationException("No route found between the selected stations.");
            }

            var response = new RouteResponseDTO
            {
                Route = path.Select(id => stations.First(s => s.ElementId == id)).ToList(),
                TotalDistance = totalDistance
            };

            _logger.LogInformation("Route successfully calculated with {Count} stations and total distance {Distance} km.", response.Route.Count(), response.TotalDistance);

            return response;
        }

        private Dictionary<long, List<(long NeighborId, double Distance)>> BuildGraph(List<StationDTO> stations, List<RailwayLineDTO> railways)
        {
            var graph = new Dictionary<long, List<(long, double)>>();

            foreach (var station in stations)
            {
                graph[station.ElementId] = new List<(long, double)>();
            }

            foreach (var railway in railways)
            {
                var connectedStations = FindStationsNearRailway(railway, stations);

                for (int i = 0; i < connectedStations.Count; i++)
                {
                    for (int j = i + 1; j < connectedStations.Count; j++)
                    {
                        var stationA = connectedStations[i];
                        var stationB = connectedStations[j];

                        if (!graph.ContainsKey(stationA.ElementId))
                        {
                            graph[stationA.ElementId] = new List<(long, double)>();
                        }
                        if (!graph.ContainsKey(stationB.ElementId))
                        {
                            graph[stationB.ElementId] = new List<(long, double)>();
                        }

                        var distance = GetDistanceBetweenCoordinates(stationA.Coordinate, stationB.Coordinate);

                        graph[stationA.ElementId].Add((stationB.ElementId, distance));
                        graph[stationB.ElementId].Add((stationA.ElementId, distance));
                    }
                }
            }

            return graph;
        }

        private List<StationDTO> FindStationsNearRailway(RailwayLineDTO railway, List<StationDTO> stations)
        {
            const double MaxDistanceKm = 5.0;
            var nearbyStations = new List<StationDTO>();

            foreach (var station in stations)
            {
                foreach (var railwayCoord in railway.Coordinates)
                {
                    var distance = GetDistanceBetweenCoordinates(station.Coordinate, railwayCoord);
                    if (distance <= MaxDistanceKm)
                    {
                        nearbyStations.Add(station);
                        break;
                    }
                }
            }

            return nearbyStations.Distinct().ToList();
        }

        private static double GetDistanceBetweenCoordinates(CoordinateDTO a, CoordinateDTO b)
        {
            var dLat = (a.Latitude - b.Latitude) * Math.PI / 180.0;
            var dLon = (a.Longitude - b.Longitude) * Math.PI / 180.0;
            var lat1 = a.Latitude * Math.PI / 180.0;
            var lat2 = b.Latitude * Math.PI / 180.0;

            var haversine = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                            Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);

            var c = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));

            return 6371.0 * c; // Earth radius in KM
        }

        private static (List<long> Path, double TotalDistance) Dijkstra(long startId, long endId, Dictionary<long, List<(long NeighborId, double Distance)>> graph)
        {
            var distances = new Dictionary<long, double>();
            var previous = new Dictionary<long, long?>();
            var nodes = new PriorityQueue<long, double>();

            foreach (var vertex in graph.Keys)
            {
                distances[vertex] = double.PositiveInfinity;
                previous[vertex] = null;
            }

            distances[startId] = 0;
            nodes.Enqueue(startId, 0);

            while (nodes.Count > 0)
            {
                var current = nodes.Dequeue();

                if (!graph.ContainsKey(current))
                {
                    continue;
                }

                if (current == endId)
                {
                    break;
                }

                foreach (var (neighbor, distance) in graph[current])
                {
                    var alt = distances[current] + distance;

                    if (alt < distances[neighbor])
                    {
                        distances[neighbor] = alt;
                        previous[neighbor] = current;
                        nodes.Enqueue(neighbor, alt);
                    }
                }
            }

            var path = new List<long>();
            var u = endId;

            if (previous[u] != null || u == startId)
            {
                while (u != null)
                {
                    path.Insert(0, u);
                    u = previous[u] ?? default;
                    if (u == default && path.First() != startId)
                    {
                        return (new List<long>(), double.PositiveInfinity);
                    }
                }
            }

            return (path, distances[endId]);
        }
    }
}
