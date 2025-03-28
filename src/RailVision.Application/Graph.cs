using RailVision.Application.Abstractions.PathFinding;
using RailVision.Application.DTOs;
using RailVision.Application.DTOs.Route;
using RailVision.Application.Helpers;

namespace RailVision.Application
{
    public class Graph
    {
        private readonly IEnumerable<StationDTO> _stations;
        private readonly IEnumerable<PopulationCenterDTO> _populationCenters;
        private readonly RouteRequestDTO _request;
        private const int DISTANCE_THRESHOLD = 30;
        private const double ALPHA = 1.0;
        private const double BETA = 0.01;

        public Graph(IEnumerable<StationDTO> stations, IEnumerable<PopulationCenterDTO> populationCenters, RouteRequestDTO request)
        {
            _stations = stations;
            _populationCenters = populationCenters;
            _request = request;

            Build(DISTANCE_THRESHOLD);
        }

        public Dictionary<string, CoordinateDTO> Nodes { get; } = [];
        public Dictionary<string, List<(string, double)>> Edges { get; } = [];

        public void AddNode(string key, CoordinateDTO? coord)
        {
            ArgumentNullException.ThrowIfNull(coord, nameof(coord));

            if (!Nodes.ContainsKey(key))
            {
                Nodes[key] = coord;
                Edges[key] = [];
            }
        }

        public void AddEdge(string from, string to, double weight)
        {
            if (!Edges.ContainsKey(from)) Edges[from] = [];
            if (!Edges.ContainsKey(to)) Edges[to] = [];

            Edges[from].Add((to, weight));
            Edges[to].Add((from, weight));
        }

        private void Build(int distanceThreshold)
        {
            Nodes.Clear();
            Edges.Clear();

            var startStation = _stations.FirstOrDefault(s => s.ElementId == _request.FromStationId);
            var endStation = _stations.FirstOrDefault(s => s.ElementId == _request.ToStationId);
            if (startStation == null || endStation == null) throw new Exception("Invalid station ID");

            var sourceCoord = startStation.Coordinate;
            var targetCoord = endStation.Coordinate;

            var intermediateCenters = _populationCenters
                .Where(pc => IsWithinBoundingBox(pc.Coordinate, sourceCoord, targetCoord))
                .OrderBy(pc => Algorithms.GetDistance(sourceCoord, pc.Coordinate))
                .ThenBy(pc => Algorithms.GetDistance(targetCoord, pc.Coordinate))
                .ToList();

            AddNode("source", sourceCoord);
            AddNode("target", targetCoord);

            foreach (var center in intermediateCenters)
            {
                var key = $"{center.Name}_{GetKey(center.Coordinate)}";
                AddNode(key, center.Coordinate);
            }

            var orderedPath = new List<string> { "source" }
                .Concat(intermediateCenters.Select(center => $"{center.Name}_{GetKey(center.Coordinate)}"))
                .Concat(new List<string> { "target" })
                .ToList();

            int maxPopulation = intermediateCenters.Max(pc => pc.Population);

            for (int i = 0; i < orderedPath.Count; i++)
            {
                for (int j = i + 1; j < orderedPath.Count; j++)
                {
                    var fromKey = orderedPath[i];
                    var toKey = orderedPath[j];

                    var fromCoord = Nodes[fromKey];
                    var toCoord = Nodes[toKey];

                    var distance = Algorithms.GetDistance(fromCoord, toCoord);
                    var fromPop = intermediateCenters.FirstOrDefault(pc => $"{pc.Name}_{GetKey(pc.Coordinate)}" == fromKey)?.Population ?? 0;
                    var toPop = intermediateCenters.FirstOrDefault(pc => $"{pc.Name}_{GetKey(pc.Coordinate)}" == toKey)?.Population ?? 0;

                    double normalizedPop = maxPopulation > 0 ? (fromPop + toPop) / 2.0 / maxPopulation : 0;
                    double weight = ALPHA * distance - BETA * normalizedPop;

                    if (distance < distanceThreshold)
                    {
                        AddEdge(fromKey, toKey, weight);
                    }
                }
            }
        }

        public List<string> FindRouteWithDynamicThreshold(IPathFindingStrategy pathfindingStrategy, int maxThresholdLimit = 200)
        {
            int currentThreshold = DISTANCE_THRESHOLD;
            List<string> path = [];

            while (currentThreshold <= maxThresholdLimit)
            {
                Build(currentThreshold);
                path = pathfindingStrategy.FindPath(this, "source", "target");

                if (path.Count != 0) break;  // Stop if a path is found
                currentThreshold += 20; // Increase threshold and retry
            }

            return path;
        }


        private static bool IsWithinBoundingBox(CoordinateDTO? coord, CoordinateDTO? source, CoordinateDTO? target)
        {

            ArgumentNullException.ThrowIfNull(coord, nameof(coord));
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(target, nameof(target));

            return coord.Latitude >= Math.Min(source.Latitude, target.Latitude) &&
                   coord.Latitude <= Math.Max(source.Latitude, target.Latitude) &&
                   coord.Longitude >= Math.Min(source.Longitude, target.Longitude) &&
                   coord.Longitude <= Math.Max(source.Longitude, target.Longitude);
        }

        private static string GetKey(CoordinateDTO? coord)
        {
            ArgumentNullException.ThrowIfNull(coord, nameof(coord));

            return $"{coord.Latitude:F6}_{coord.Longitude:F6}";
        }
    }

}
