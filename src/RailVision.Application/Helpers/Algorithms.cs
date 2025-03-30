using RailVision.Application.DTOs;

namespace RailVision.Application.Helpers
{
    public static class Algorithms
    {
        public static double NormalizePopulation(double fromPopulation, double toPopulation, double maxPopulation)
        {
            return maxPopulation > 0 ? (fromPopulation + toPopulation) / 2 / maxPopulation : 0;
        }
        public static double GetWeightByDistanceAndPopulation(double distance, double population)
        {
            const double ALPHA = 1.0;
            const double BETA = 0.01;

            return ALPHA * distance - BETA * population;
        }
        public static List<string> Dijkstra(Dictionary<string, List<(string, double)>> graph, string startKey, string endKey)
        {
            var distances = new Dictionary<string, double>();
            var previous = new Dictionary<string, string>();
            var unvisited = new HashSet<string>(graph.Keys);

            foreach (var node in graph.Keys)
            {
                distances[node] = double.MaxValue;
            }
            distances[startKey] = 0;

            while (unvisited.Count > 0)
            {
                var current = unvisited.OrderBy(n => distances[n]).First();
                unvisited.Remove(current);

                if (current == endKey) break;

                foreach (var (neighbor, weight) in graph[current])
                {
                    if (!unvisited.Contains(neighbor)) continue;

                    var tentativeDist = distances[current] + weight;
                    if (tentativeDist < distances[neighbor])
                    {
                        distances[neighbor] = tentativeDist;
                        previous[neighbor] = current;
                    }
                }
            }

            var path = new List<string>();
            var currentKey = endKey;

            while (previous.ContainsKey(currentKey))
            {
                path.Insert(0, currentKey);
                currentKey = previous[currentKey];
            }

            if (path.Count > 0) path.Insert(0, startKey);

            return path;
        }

        public static List<string> AStar(Dictionary<string, List<(string, double)>> graph, string startKey, string endKey, Dictionary<string, CoordinateDTO> graphNodes)
        {
            var openSet = new HashSet<string> { startKey };
            var cameFrom = new Dictionary<string, string>();
            var gScore = graph.Keys.ToDictionary(node => node, node => double.MaxValue);
            var fScore = graph.Keys.ToDictionary(node => node, node => double.MaxValue);

            gScore[startKey] = 0;
            fScore[startKey] = Haversine(graphNodes[startKey], graphNodes[endKey]);

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(n => fScore[n]).First();
                openSet.Remove(current);

                if (current == endKey)
                    return ReconstructPath(cameFrom, current);

                foreach (var (neighbor, weight) in graph[current])
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

        private static List<string> ReconstructPath(Dictionary<string, string> cameFrom, string current)
        {
            var path = new List<string> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        public static double GetDistance(CoordinateDTO? a, CoordinateDTO? b) => Haversine(a, b);

        private static double Haversine(CoordinateDTO? coordSrc, CoordinateDTO? coordDest)
        {
            ArgumentNullException.ThrowIfNull(coordSrc);
            ArgumentNullException.ThrowIfNull(coordDest);

            const double EarthRadius = 6371.0;

            double lat1 = DegreesToRadians(coordSrc.Latitude);
            double lon1 = DegreesToRadians(coordSrc.Longitude);
            double lat2 = DegreesToRadians(coordDest.Latitude);
            double lon2 = DegreesToRadians(coordDest.Longitude);

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
