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
        private double _distanceThreshold = 1;
        private double _thresholdIncrement = 1;

        public Graph(IEnumerable<StationDTO> stations, IEnumerable<PopulationCenterDTO> populationCenters, RouteRequestDTO request)
        {
            _stations = stations;
            _populationCenters = populationCenters;
            _request = request;

            Build();
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

        private void Build(bool dynamic = false)
        {
            Nodes.Clear();
            Edges.Clear();

            CoordinateDTO? sourceCoord = null;
            CoordinateDTO? targetCoord = null;

            if (_request.FromId != null && _request.ToId != null)
            {
                var start = _populationCenters.FirstOrDefault(s => s.ElementId == _request.FromId);

                var end = _populationCenters.FirstOrDefault(s => s.ElementId == _request.ToId);

                if (start == null || end == null) throw new Exception("Invalid ID");

                sourceCoord = start.Coordinate;
                targetCoord = end.Coordinate;
            }
            else if (_request.FromCoordinate != null && _request.ToCoordinate != null)
            {
                sourceCoord = _request.FromCoordinate;
                targetCoord = _request.ToCoordinate;
            }

            //var sourceCoord = new CoordinateDTO
            //{
            //    Latitude = 40.4615983,
            //    Longitude = 49.9214189
            //};

            //var targetCoord = new CoordinateDTO
            //{
            //    Latitude = 40.14202,
            //    Longitude = 48.07276
            //};

            // var sourceCoord = startStation.Coordinate;
            // var targetCoord = endStation.Coordinate;

            var intermediateCenters = _populationCenters
                .Where(pc => IsWithinBoundingBox(pc.Coordinate, sourceCoord, targetCoord))
                .OrderBy(pc => Algorithms.GetDistance(sourceCoord, pc.Coordinate))
                .ThenBy(pc => Algorithms.GetDistance(targetCoord, pc.Coordinate))
                .ToList();

            AddNode("source", sourceCoord);
            AddNode("target", targetCoord);

            foreach (var center in intermediateCenters)
            {
                var key = GetKey(center);
                AddNode(key, center.Coordinate);
            }

            //var orderedPath = new List<string> { "source" }
            //    .Concat(intermediateCenters.Select(GetKey))
            //    .Concat(new List<string> { "target" })
            //    .ToList();

            int maxPopulation = intermediateCenters.Max(pc => pc.Population);

            if (!dynamic)
            {
                var routeDistance = Algorithms.GetDistance(sourceCoord, targetCoord);
                _distanceThreshold = _thresholdIncrement = routeDistance / 10;
            }

            var nodeKeys = Nodes.Keys.ToArray();

            for (int i = 0; i < nodeKeys.Length; i++)
            {
                for (int j = i + 1; j < nodeKeys.Length; j++)
                {
                    var fromKey = nodeKeys[i];
                    var toKey = nodeKeys[j];

                    var fromCoord = Nodes[fromKey];
                    var toCoord = Nodes[toKey];

                    var distance = Algorithms.GetDistance(fromCoord, toCoord);

                    var fromPop = intermediateCenters.FirstOrDefault(pc => GetKey(pc) == fromKey)?.Population ?? 0;
                    var toPop = intermediateCenters.FirstOrDefault(pc => GetKey(pc) == toKey)?.Population ?? 0;

                    double normalizedPop = Algorithms.NormalizePopulation(fromPop, toPop, maxPopulation);

                    double weight = Algorithms.GetWeightByDistanceAndPopulation(distance, normalizedPop);

                    if (distance < _distanceThreshold)
                    {
                        AddEdge(fromKey, toKey, weight);
                    }
                }
            }
        }

        public List<string> FindRouteWithDynamicThreshold(IPathFindingStrategy pathfindingStrategy, double maxThresholdLimit = 200)
        {
            List<string> path = [];

            while (_distanceThreshold <= maxThresholdLimit)
            {
                Build(true);
                path = pathfindingStrategy.FindPath(this, "source", "target");

                if (path.Count != 0) break;  // Stop if a path is found
                _distanceThreshold += _thresholdIncrement; // Increase threshold and retry
            }

            return path;
        }

        private static bool IsWithinBoundingBox(CoordinateDTO? coord, CoordinateDTO? source, CoordinateDTO? target)
        {

            ArgumentNullException.ThrowIfNull(coord, nameof(coord));
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(target, nameof(target));

            return coord.Latitude > Math.Min(source.Latitude, target.Latitude) &&
                   coord.Latitude < Math.Max(source.Latitude, target.Latitude) &&
                   coord.Longitude > Math.Min(source.Longitude, target.Longitude) &&
                   coord.Longitude < Math.Max(source.Longitude, target.Longitude);
        }

        private static string GetKey(PopulationCenterDTO populationCenter)
        {

            ArgumentNullException.ThrowIfNull(populationCenter.Coordinate, nameof(populationCenter.Coordinate));

            return $"{populationCenter.Name}_{populationCenter.Coordinate.Latitude:F6}_{populationCenter.Coordinate.Longitude:F6}";
        }
    }

}
