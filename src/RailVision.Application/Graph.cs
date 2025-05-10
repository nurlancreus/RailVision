using Microsoft.AspNetCore.Hosting;
using NetTopologySuite.Geometries;
using RailVision.Application.Abstractions.PathFinding;
using RailVision.Application.DTOs;
using RailVision.Application.DTOs.Route;
using RailVision.Application.Helpers;
using RailVision.Domain;
using RailVision.Domain.Exceptions;

namespace RailVision.Application
{
    public class Graph
    {
        private readonly IEnumerable<PopulationCenterDTO> _populationCenters;
        private readonly RouteRequestDTO _request;
        private double _distanceThreshold = 1;
        private double _thresholdIncrement = 1;

        public Graph(IEnumerable<PopulationCenterDTO> populationCenters, RouteRequestDTO request)
        {
            _populationCenters = populationCenters;
            _request = request;
            Build();
        }

        public Dictionary<string, Coordinate> Nodes { get; } = [];
        public Dictionary<string, List<(string neighbor, double weight, double distance)>> Edges { get; } = [];

        public void AddNode(string key, Coordinate? coord)
        {
            ArgumentNullException.ThrowIfNull(coord, nameof(coord));
            if (!Nodes.ContainsKey(key)) Nodes[key] = coord;
            if (!Edges.ContainsKey(key)) Edges[key] = [];
        }

        public void AddEdge(string from, string to, double weight, double distance)
        {
            Edges[from].Add((to, weight, distance));
            Edges[to].Add((from, weight, distance));
        }

        private void Build(bool dynamic = false)
        {
            Nodes.Clear();
            Edges.Clear();

            Coordinate? sourceCoord = null;
            Coordinate? targetCoord = null;

            if (_request.FromId != null && _request.ToId != null)
            {
                var start = _populationCenters.FirstOrDefault(s => s.ElementId == _request.FromId);
                var end = _populationCenters.FirstOrDefault(s => s.ElementId == _request.ToId);
                if (start == null || end == null || start.Coordinate == null || end.Coordinate == null) throw new Exception("Invalid ID");

                sourceCoord = new Coordinate { X = start.Coordinate.Longitude, Y = start.Coordinate.Latitude };
                targetCoord = new Coordinate { X = end.Coordinate.Longitude, Y = end.Coordinate.Latitude };
            }
            else if (_request.FromCoordinate != null && _request.ToCoordinate != null)
            {
                sourceCoord = new Coordinate { X = _request.FromCoordinate.Longitude, Y = _request.FromCoordinate.Latitude };
                targetCoord = new Coordinate { X = _request.ToCoordinate.Longitude, Y = _request.ToCoordinate.Latitude };
            }
            else
            {
                throw new AppException("Invalid coordinates");
            }

            if (sourceCoord.Equals2D(targetCoord))
                throw new SameCoordinateException("Source and target coordinates are the same.");

            AddNode(Constants.SourceNode, sourceCoord);
            AddNode(Constants.TargetNode, targetCoord);

            bool isClose = false;
            if (!dynamic)
            {
                const byte CLOSE_DISTANCE = 10; // km
                var routeDistance = Algorithms.GetDistance(sourceCoord, targetCoord);
                if (routeDistance < CLOSE_DISTANCE) isClose = true;


                else
                {
                    _distanceThreshold = _thresholdIncrement = CalculateThreshhold(routeDistance);
                }
            }

            const int POPULATION_THRESHOLD = 5000;

            var intermediateCenters = _populationCenters
                .Where(pc => pc.Population > POPULATION_THRESHOLD && IsWithinBoundingBox(pc.Coordinate, sourceCoord, targetCoord, _distanceThreshold))
                .ToList();

            foreach (var center in intermediateCenters)
            {
                var key = GetKey(center);
                AddNode(key, new Coordinate
                {
                    X = center.Coordinate?.Longitude ?? 0,
                    Y = center.Coordinate?.Latitude ?? 0
                });
            }

            int maxPopulation = intermediateCenters.Count != 0 ? intermediateCenters.Max(pc => pc.Population) : 1;

            var keyPopMap = intermediateCenters
                .ToDictionary(GetKey, g => g.Population);

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

                    if (distance > _distanceThreshold && !isClose) continue;

                    var fromPop = keyPopMap.GetValueOrDefault(fromKey, 0);
                    var toPop = keyPopMap.GetValueOrDefault(toKey, 0);
                    double normalizedPop = Algorithms.NormalizePopulation(fromPop, toPop, maxPopulation);
                    double weight = Algorithms.GetWeightFromDistanceAndPopulation(distance, normalizedPop);

                    AddEdge(fromKey, toKey, weight, distance);
                }
            }
        }

        private static double CalculateThreshhold(double distanceBetweenSourceAndTarget)
        {
            const int FAR_DISTANCE = 250; // km
            const int MIDDLE_DISTANCE = 200; // km

            if (distanceBetweenSourceAndTarget > FAR_DISTANCE)
                return distanceBetweenSourceAndTarget / 15;
            else if (distanceBetweenSourceAndTarget > MIDDLE_DISTANCE)
                return distanceBetweenSourceAndTarget / 10;

            return distanceBetweenSourceAndTarget / 5;
        }

        public List<(string node, double distance)> FindRouteWithDynamicThreshold(IPathFindingStrategy pathfindingStrategy, IWebHostEnvironment webHostEnvironment, double maxThresholdLimit = 200)
        {
            List<(string node, double distance)> path = [];

            while (_distanceThreshold <= maxThresholdLimit)
            {
                Build(true);
                path = pathfindingStrategy.FindPath(this, Constants.SourceNode, Constants.TargetNode, webHostEnvironment);
                if (path.Count != 0) break;
                _distanceThreshold += _thresholdIncrement * 2;
            }

            return path;
        }

        //private static bool IsWithinBoundingBox(CoordinateDTO? coord, Coordinate? source, Coordinate? target, double bufferKm = 5.0)
        //{
        //    ArgumentNullException.ThrowIfNull(coord);
        //    ArgumentNullException.ThrowIfNull(source);
        //    ArgumentNullException.ThrowIfNull(target);

        //    double bufferDeg = bufferKm * 0.009;

        //    bool withinLat = coord.Latitude >= Math.Min(source.Y, target.Y) - bufferDeg &&
        //                     coord.Latitude <= Math.Max(source.Y, target.Y) + bufferDeg;

        //    bool withinLon = coord.Longitude >= Math.Min(source.X, target.X) - bufferDeg &&
        //                     coord.Longitude <= Math.Max(source.X, target.X) + bufferDeg;

        //    return withinLat && withinLon;
        //}

        private static bool IsWithinBoundingBox(CoordinateDTO? coord, Coordinate? sourceCoord, Coordinate? targetCoord, double bufferKm = 10)
        {
            ArgumentNullException.ThrowIfNull(coord);
            ArgumentNullException.ThrowIfNull(sourceCoord);
            ArgumentNullException.ThrowIfNull(targetCoord);

            // Rough conversion: 1 km ≈ 0.009 degrees
            const double KM_IN_DEGREE = 0.009;

            double bufferDeg = bufferKm * KM_IN_DEGREE;

            var envelope = new Envelope(
                Math.Min(sourceCoord.X, targetCoord.X) - bufferDeg,
                Math.Max(sourceCoord.X, targetCoord.X) + bufferDeg,
                Math.Min(sourceCoord.Y, targetCoord.Y) - bufferDeg,
                Math.Max(sourceCoord.Y, targetCoord.Y) + bufferDeg
            );

            return envelope.Contains(new Coordinate(coord.Longitude, coord.Latitude));
        }

        private static string GetKey(PopulationCenterDTO populationCenter) => populationCenter.Name;
    }
}
