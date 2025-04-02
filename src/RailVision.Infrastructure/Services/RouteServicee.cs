//using RailVision.Application.Abstractions;
//using RailVision.Application.DTOs.Route;
//using RailVision.Application.DTOs;

//namespace RailVision.Infrastructure.Services
//{
//    public class RouteServicee(IRailwayService railwayService, IStationService stationService, ITerrainService terrainService) : IRouteService
//    {
//        private readonly IRailwayService _railwayService = railwayService;
//        private readonly IStationService _stationService = stationService;
//        private readonly ITerrainService _terrainService = terrainService;

//        public async Task<object> DrawOptimalRouteAsync(RouteRequestDTO request, CancellationToken cancellationToken = default)
//        {
//            var stationData = await _stationService.GetStationsDataAsync(cancellationToken);
//            var railwaysData = await _railwayService.GetRailwaysDataAsync(cancellationToken);

//            var stations = stationData.Elements.Select(e => new StationDTO
//            {
//                Id = Guid.NewGuid(),
//                ElementId = e.Id,
//                Name = e.Tags.TryGetValue("name", out var name) ? name : "Unknown",
//                Coordinate = new CoordinateDTO
//                {
//                    Latitude = e.Lat ?? 0,
//                    Longitude = e.Lon ?? 0
//                }
//            }).ToList();

//            var nodeLookup = railwaysData.Elements
//                .Where(e => e.Type == "node")
//                .ToDictionary(n => n.Id, n => new CoordinateDTO
//                {
//                    Latitude = n.Lat ?? 0,
//                    Longitude = n.Lon ?? 0
//                });

//            var railways = railwaysData.Elements
//                .Where(e => e.Type == "way")
//                .Select(e => new RailwayLineDTO
//                {
//                    Id = Guid.NewGuid(),
//                    ElementId = e.Id,
//                    Coordinates = e.Nodes.Select(n => new CoordinateDTO
//                    {
//                        Latitude = nodeLookup[n].Latitude,
//                        Longitude = nodeLookup[n].Longitude
//                    }).ToList()
//                }).ToList();

//            // --- Build Graph ---
//            var graph = new Dictionary<string, List<(string, double)>>();
//            var nodes = new Dictionary<string, CoordinateDTO>();

//            foreach (var line in railways)
//            {
//                for (int i = 0; i < line.Coordinates.Count - 1; i++)
//                {
//                    var fromCoord = line.Coordinates[i];
//                    var toCoord = line.Coordinates[i + 1];

//                    var fromKey = GetCoordKey(fromCoord);
//                    var toKey = GetCoordKey(toCoord);

//                    nodes[fromKey] = fromCoord;
//                    nodes[toKey] = toCoord;

//                    var distance = GetDistance(fromCoord, toCoord);

//                    if (!graph.ContainsKey(fromKey)) graph[fromKey] = [];
//                    if (!graph.ContainsKey(toKey)) graph[toKey] = [];

//                    graph[fromKey].Add((toKey, distance));
//                    graph[toKey].Add((fromKey, distance)); // undirected graph
//                }
//            }

//            // --- Snap Source and Target to graph ---

//            var startStation = stations.FirstOrDefault(s => s.ElementId == request.FromStationId);

//            var endStation = stations.FirstOrDefault(s => s.ElementId == request.ToStationId);

//            if (startStation == null || endStation == null)
//                return new { Success = false, Message = "Invalid station ID." };

//            var sourceCoord = startStation.Coordinate;
//            var targetCoord = endStation.Coordinate;

//            var sourceKey = "source";
//            var targetKey = "target";

//            nodes[sourceKey] = sourceCoord;
//            nodes[targetKey] = targetCoord;

//            ConnectToNearestNode(sourceKey, sourceCoord, nodes, graph);
//            ConnectToNearestNode(targetKey, targetCoord, nodes, graph);

//            // --- Run Dijkstra ---
//            var path = Dijkstra(graph, sourceKey, targetKey);

//            if (path == null || path.Count == 0)
//                return new { Success = false, Message = "No route found." };

//            var routeCoordinates = path.Select(key => nodes[key]).ToList();

//            return new
//            {
//                Path = routeCoordinates
//            };
//        }

//        private static void ConnectToNearestNode(string nodeKey, CoordinateDTO coord, Dictionary<string, CoordinateDTO> nodes, Dictionary<string, List<(string, double)>> graph)
//        {
//            //var a = nodes
//            //    .Where(n => n.Key != nodeKey)
//            //    .Select(n => GetDistance(coord, n.Value));

//            var nearestKey = nodes
//                .Where(n => n.Key != nodeKey)
//                .OrderBy(n => GetDistance(coord, n.Value))
//                .First().Key;

//            var distance = GetDistance(coord, nodes[nearestKey]);

//            if (!graph.ContainsKey(nodeKey)) graph[nodeKey] = [];
//            graph[nodeKey].Add((nearestKey, distance));

//            if (!graph.ContainsKey(nearestKey)) graph[nearestKey] = [];
//            graph[nearestKey].Add((nodeKey, distance));
//        }

//        private static string GetCoordKey(CoordinateDTO coord) => $"{coord.Latitude:F6}_{coord.Longitude:F6}";

//        private static double GetDistance(CoordinateDTO a, CoordinateDTO b) => Haversine(a, b);

//        private static double Haversine(CoordinateDTO? coordSrc, CoordinateDTO? coordDest)
//        {
//            ArgumentNullException.ThrowIfNull(coordSrc, nameof(coordSrc));
//            ArgumentNullException.ThrowIfNull(coordDest, nameof(coordDest));

//            const double EarthRadius = 6371.0;

//            double lat1 = DegreesToRadians(coordSrc.Latitude);
//            double lon1 = DegreesToRadians(coordSrc.Longitude);
//            double lat2 = DegreesToRadians(coordDest.Latitude);
//            double lon2 = DegreesToRadians(coordDest.Longitude);

//            double dLat = lat2 - lat1;
//            double dLon = lon2 - lon1;

//            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//                       Math.Cos(lat1) * Math.Cos(lat2) *
//                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

//            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

//            return EarthRadius * c;
//        }

//        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

//        private static List<string> Dijkstra(Dictionary<string, List<(string, double)>> graph, string startKey, string endKey)
//        {
//            var distances = new Dictionary<string, double>();
//            var previous = new Dictionary<string, string>();
//            var unvisited = new HashSet<string>(graph.Keys);

//            foreach (var node in graph.Keys)
//            {
//                distances[node] = double.MaxValue;
//            }
//            distances[startKey] = 0;

//            while (unvisited.Count > 0)
//            {
//                var current = unvisited.OrderBy(n => distances[n]).First();
//                unvisited.Remove(current);

//                if (current == endKey) break;

//                foreach (var (neighbor, weight) in graph[current])
//                {
//                    if (!unvisited.Contains(neighbor)) continue;

//                    var tentativeDist = distances[current] + weight;
//                    if (tentativeDist < distances[neighbor])
//                    {
//                        distances[neighbor] = tentativeDist;
//                        previous[neighbor] = current;
//                    }
//                }
//            }

//            var path = new List<string>();
//            var currentKey = endKey;

//            while (previous.ContainsKey(currentKey))
//            {
//                path.Insert(0, currentKey);
//                currentKey = previous[currentKey];
//            }
//            if (path.Count > 0) path.Insert(0, startKey);

//            return path;
//        }
//    }
//}
