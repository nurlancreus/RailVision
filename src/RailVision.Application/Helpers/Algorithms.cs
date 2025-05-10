using Microsoft.AspNetCore.Hosting;
using NetTopologySuite.Geometries;
using RailVision.Application.DTOs;
using RailVision.Domain.Exceptions;

namespace RailVision.Application.Helpers
{
    public static class Algorithms
    {
        public static double NormalizePopulation(double fromPopulation, double toPopulation, double maxPopulation)
        {
            return maxPopulation > 0 ? (fromPopulation + toPopulation) / 2 / maxPopulation : 0;
        }
        public static double GetWeightFromDistanceAndPopulation(double distance, double population)
        {
            const double ALPHA = 1.0;
            const double BETA = 0.01;

            return ALPHA * distance - BETA * population;
        }

        //public static List<string> Dijkstra(Dictionary<string, List<(string, double)>> graph, string startKey, string endKey)
        //{
        //    var distances = new Dictionary<string, double>();
        //    var previous = new Dictionary<string, string>();
        //    var unvisited = new HashSet<string>(graph.Keys);

        //    foreach (var node in graph.Keys)
        //    {
        //        distances[node] = double.MaxValue;
        //    }
        //    distances[startKey] = 0;

        //    while (unvisited.Count > 0)
        //    {
        //        var current = unvisited.OrderBy(n => distances[n]).First();
        //        unvisited.Remove(current);

        //        if (current == endKey) break;

        //        foreach (var (neighbor, weight) in graph[current])
        //        {
        //            if (!unvisited.Contains(neighbor)) continue;

        //            var tentativeDist = distances[current] + weight;
        //            if (tentativeDist < distances[neighbor])
        //            {
        //                distances[neighbor] = tentativeDist;
        //                previous[neighbor] = current;
        //            }
        //        }
        //    }

        //    var path = new List<string>();
        //    var currentKey = endKey;

        //    while (previous.ContainsKey(currentKey))
        //    {
        //        path.Insert(0, currentKey);
        //        currentKey = previous[currentKey];
        //    }

        //    if (path.Count > 0) path.Insert(0, startKey);

        //    return path;
        //}


        public static List<(string node, double distance)> Dijkstra(Graph graph, string startKey, string endKey, IWebHostEnvironment webHostEnvironment)
        {
            var azerbaijanGeometry = GeoJsonService.LoadAzerbaijanGeoData(webHostEnvironment);
            var nakhchivanGeometry = GeoJsonService.LoadNakhchivanGeoData(webHostEnvironment);

            var distances = new Dictionary<string, (double weight, double distance)>();
            var previous = new Dictionary<string, string>();
            var unvisited = new HashSet<string>(graph.Edges.Keys);

            // Check if target is in nakhchivan
            var sourceCoord = graph.Nodes[startKey];
            var targetCoord = graph.Nodes[endKey];

            var sourcePoint = new Point(sourceCoord);
            var targetPoint = new Point(targetCoord);

            var isSourceInNakhchivan = sourcePoint.Within(nakhchivanGeometry);
            var isTargetInNakhchivan = targetPoint.Within(nakhchivanGeometry);

            if ((!isSourceInNakhchivan && isTargetInNakhchivan) || (isSourceInNakhchivan && !isTargetInNakhchivan)) throw new RouteIsNotPossibleException("Route is not possible between Nakhchivan and Azerbaijan main land.");

            foreach (var node in graph.Edges.Keys)
                distances[node] = (double.MaxValue, 0);

            distances[startKey] = (0, 0);

            while (unvisited.Count > 0)
            {
                var current = unvisited.OrderBy(n => distances[n]).First();
                unvisited.Remove(current);

                if (current == endKey)
                    break;

                foreach (var (neighbor, weight, distance) in graph.Edges[current])
                {
                    if (!unvisited.Contains(neighbor)) continue;

                    var currentCoord = graph.Nodes[current];
                    var neighborCoord = graph.Nodes[neighbor];

                    var isLinePoint = currentCoord.CoordinateValue.Equals2D(neighborCoord.CoordinateValue);

                    var line = new LineString([currentCoord, neighborCoord]);

                    if (line.Within(azerbaijanGeometry) || isLinePoint)
                    {
                        var tentativeDist = distances[current].weight + weight;
                        if (tentativeDist < distances[neighbor].weight)
                        {
                            distances[neighbor] = (weight: tentativeDist, distance);
                            previous[neighbor] = current;
                        }
                    }
                    else
                    {
                        var alternativeNode = FindAlternativeNode(azerbaijanGeometry, current, neighbor, graph.Nodes);
                        if (alternativeNode != null && graph.Edges[current].Any(x => x.neighbor == alternativeNode))
                        {
                            var alternativeWeight = graph.Edges[current].First(x => x.neighbor == alternativeNode).weight;
                            var tentativeDist = distances[current].weight + alternativeWeight;

                            if (tentativeDist < distances[alternativeNode].weight)
                            {
                                distances[alternativeNode] = (weight: tentativeDist, distances[alternativeNode].distance);
                                previous[alternativeNode] = current;
                            }

                            unvisited.Remove(neighbor); // avoid revisiting invalid neighbor
                        }
                    }
                }
            }

            var path = new List<(string node, double distance)>();
            var currentNode = endKey;

            if (!previous.ContainsKey(currentNode) && currentNode != startKey)
                return path; // unreachable

            while (previous.ContainsKey(currentNode))
            {
                var distance = distances[currentNode].distance;
                path.Insert(0, (currentNode, distance));
                currentNode = previous[currentNode];
            }

            if (path.Count > 0 || currentNode == startKey)
            {
                var distance = distances[currentNode].distance;
                path.Insert(0, (startKey, distance));
            }

            return path;
        }

        private static string FindAlternativeNode(Geometry azerbaijanGeometry, string fromNode, string toNode, Dictionary<string, Coordinate> graphNodes)
        {
            var fromCoordinate = graphNodes[fromNode];
            var toCoordinate = graphNodes[toNode];

            var line = new LineString([fromCoordinate, toCoordinate]);

            if (!line.Within(azerbaijanGeometry))
            {
                // Logic to find a new target node that stays inside Azerbaijan
                var nearestNode = graphNodes
                    .Where(g => azerbaijanGeometry.Contains(new Point(g.Value)) && g.Key != fromNode && g.Key != toNode)
                    .OrderBy(g => GetDistance(fromCoordinate, g.Value))
                    .FirstOrDefault();

                if (nearestNode.Key != toNode) // Ensure the alternative node is not the same as the original node
                {
                    return nearestNode.Key;
                }
            }
            else
            {
                // If the line is within Azerbaijan, return the original node
                return toNode;
            }

            throw new Exception("New alternative route is not found.");
        }


        public static List<(string node, double distance)> AStar(Graph graph, string startKey, string endKey, Dictionary<string, Coordinate> graphNodes)
        {
            var openSet = new HashSet<string> { startKey };
            var cameFrom = new Dictionary<string, string>();
            var gScore = graph.Edges.Keys.ToDictionary(node => node, node => double.MaxValue);
            var fScore = graph.Edges.Keys.ToDictionary(node => node, node => double.MaxValue);

            gScore[startKey] = 0;
            fScore[startKey] = Haversine(graphNodes[startKey], graphNodes[endKey]);

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(n => fScore[n]).First();
                openSet.Remove(current);

                if (current == endKey)
                    // return ReconstructPath(cameFrom, current);

                    foreach (var (neighbor, weight, distance) in graph.Edges[current])
                    {
                        var tentativeGScore = gScore[current] + weight;
                        if (tentativeGScore < gScore[neighbor])
                        {
                            cameFrom[neighbor] = current;
                            gScore[neighbor] = tentativeGScore;
                            fScore[neighbor] = gScore[neighbor] + Haversine(graphNodes[neighbor], graphNodes[endKey]);
                            openSet.Add(neighbor);
                        }
                    }
            }
            return [];
        }

        //private static List<(string node, double distance)> ReconstructPath(Dictionary<string, string> cameFrom, string current)
        //{
        //    var path = new List<(string node, double distance)> { current };
        //    while (cameFrom.ContainsKey(current))
        //    {
        //        current = cameFrom[current];
        //        path.Insert(0, current);
        //    }
        //    return path;
        //}

        public static double GetDistance(Coordinate? a, Coordinate? b) => Haversine(a, b);

        private static double Haversine(Coordinate? coordSrc, Coordinate? coordDest)
        {
            ArgumentNullException.ThrowIfNull(coordSrc);
            ArgumentNullException.ThrowIfNull(coordDest);

            const double EarthRadius = 6371.0;

            double lat1 = DegreesToRadians(coordSrc.Y);
            double lon1 = DegreesToRadians(coordSrc.X);
            double lat2 = DegreesToRadians(coordDest.Y);
            double lon2 = DegreesToRadians(coordDest.X);

            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadius * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
    }
}
