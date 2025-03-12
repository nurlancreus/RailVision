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

        public async Task<RouteResponseDTO> CalculateRouteAsync(RouteRequestDTO request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Calculating route from station {FromStationId} to station {ToStationId}", request.FromStationId, request.ToStationId);

            var stations = (await _stationService.GetStationCoordsAsync(cancellationToken)).ToList();
            var railways = (await _railwayService.GetRailwayLinesAsync(cancellationToken)).ToList();

            _stationLookup.Clear();
            stations.ForEach(s => _stationLookup[s.Id] = s);

            _logger.LogInformation("Built graph with {StationCount} stations and {RailwayCount} railway lines", stations.Count, railways.Count);

            var graph = BuildGraph(stations, railways);

            var startId = request.FromStationId;
            var endId = request.ToStationId;

            var path = AStar(graph, startId, endId, _logger);
            //var path = Dijkstra(graph, startId, endId, _logger);

            var routeStations = path
                .Select(id => _stationLookup[id])
                .OrderBy(s => Heuristic(s.Id, endId))
                .ToList();

            var totalDistance = CalculatePathDistance(routeStations);

            _logger.LogInformation("Route calculated with {RouteStationCount} stations and total distance of {TotalDistance} km", routeStations.Count, totalDistance);

            return new RouteResponseDTO
            {
                Route = routeStations,
                TotalDistance = totalDistance
            };
        }

        private static Dictionary<long, List<(long neighborId, double distance)>> BuildGraph(IEnumerable<StationDTO> stations, IEnumerable<RailwayLineDTO> railways)
        {
            var graph = new Dictionary<long, List<(long, double)>>();

            foreach (var station in stations)
            {
                graph[station.Id] = [];
            }

            foreach (var line in railways)
            {
                var lineStations = line.Coordinates
                    .Select(coord => stations.FirstOrDefault(s => AreClose(s.Coordinate!, coord)))
                    .Where(s => s != null)
                    .Distinct()
                    .ToList();

                for (int i = 0; i < lineStations.Count - 1; i++)
                {
                    var from = lineStations[i]!;
                    var to = lineStations[i + 1]!;

                    var distance = CalculateHaversineDistance(from.Coordinate, to.Coordinate);

                    graph[from.Id].Add((to.Id, distance));
                    graph[to.Id].Add((from.Id, distance));
                }
            }

            return graph;
        }

        private static bool AreClose(CoordinateDTO c1, CoordinateDTO c2, double threshold = 0.1)
        {
            var dLat = Math.Abs(c1.Latitude - c2.Latitude);
            var dLon = Math.Abs(c1.Longitude - c2.Longitude);
            return dLat < threshold && dLon < threshold;
        }

        private static double CalculatePathDistance(IEnumerable<StationDTO> path)
        {
            var list = path.ToList();
            double distance = 0;
            for (int i = 0; i < list.Count - 1; i++)
            {
                distance += CalculateHaversineDistance(list[i].Coordinate, list[i + 1].Coordinate);
            }
            return distance;
        }

        private static List<long> AStar(Dictionary<long, List<(long neighborId, double distance)>> graph, long startId, long endId, ILogger<RouteService> logger)
        {
            logger.LogInformation("Running A* algorithm to find the best path from {StartStationId} to {EndStationId}", startId, endId);

            var openSet = new SortedSet<(double fScore, long node)>(Comparer<(double fScore, long node)>.Create((a, b) => a.fScore == b.fScore ? a.node.CompareTo(b.node) : a.fScore.CompareTo(b.fScore)));

            var cameFrom = new Dictionary<long, long?>();
            var gScore = graph.Keys.ToDictionary(n => n, n => double.MaxValue);
            var fScore = graph.Keys.ToDictionary(n => n, n => double.MaxValue);

            gScore[startId] = 0;
            fScore[startId] = Heuristic(startId, endId);

            openSet.Add((fScore[startId], startId));

            while (openSet.Count != 0)
            {
                var current = openSet.Min.node;
                if (current == endId)
                    return ReconstructPath(cameFrom, current, startId);

                openSet.Remove(openSet.Min);

                foreach (var (neighbor, dist) in graph[current])
                {
                    var tentativeGScore = gScore[current] + dist;

                    if (tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + Heuristic(neighbor, endId);

                        openSet.Add((fScore[neighbor], neighbor));
                    }
                }
            }

            return [];
        }

        private static List<long> Dijkstra(Dictionary<long, List<(long neighborId, double distance)>> graph, long startId, long endId, ILogger<RouteService> logger)
        {
            logger.LogInformation("Running Dijkstra algorithm to find the best path from {StartStationId} to {EndStationId}", startId, endId);

            var dist = new Dictionary<long, double>();
            var prev = new Dictionary<long, long?>();
            var unvisited = new HashSet<long>(graph.Keys);

            foreach (var node in graph.Keys)
            {
                dist[node] = double.MaxValue;
                prev[node] = null;
            }

            dist[startId] = 0;

            while (unvisited.Count > 0)
            {
                var current = unvisited.OrderBy(n => dist[n]).First();
                if (current == endId) break;

                unvisited.Remove(current);

                foreach (var (neighbor, d) in graph[current])
                {
                    if (!unvisited.Contains(neighbor)) continue;

                    var alt = dist[current] + d;
                    if (alt < dist[neighbor])
                    {
                        dist[neighbor] = alt;
                        prev[neighbor] = current;
                    }
                }
            }

            var path = new List<long>();
            var u = endId;
            while (prev[u] != null)
            {
                path.Insert(0, u);
                u = prev[u] ?? 0;
            }
            path.Insert(0, startId);

            return path;
        }

        private static double Heuristic(long currentId, long goalId)
        {
            if (!_stationLookup.TryGetValue(currentId, out StationDTO? current) || !_stationLookup.TryGetValue(goalId, out StationDTO? goal))
                return 0;

            return CalculateHaversineDistance(current.Coordinate, goal.Coordinate);
        }

        private static List<long> ReconstructPath(Dictionary<long, long?> cameFrom, long current, long startId)
        {
            var path = new List<long> { current };

            while (cameFrom.TryGetValue(current, out var prev) && prev != null)
            {
                current = prev.Value;
                path.Insert(0, current);
            }

            if (path.First() != startId) return [];
            return path;
        }

        private static double CalculateHaversineDistance(CoordinateDTO? a, CoordinateDTO? b)
        {
            if (a == null || b == null) return 0;

            const double R = 6371;
            var lat1 = DegreesToRadians(a.Latitude);
            var lon1 = DegreesToRadians(a.Longitude);
            var lat2 = DegreesToRadians(b.Latitude);
            var lon2 = DegreesToRadians(b.Longitude);

            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;

            var h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));

            return R * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
