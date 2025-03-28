using RailVision.Application.Abstractions.PathFinding;
using RailVision.Application.Helpers;
using RailVision.Application;

namespace RailVision.Infrastructure.Services.PathFinding
{
    public class DijkstraPathFinding : IPathFindingStrategy
    {
        public List<string> FindPath(Graph graph, string startKey, string endKey)
        {
            return Algorithms.Dijkstra(graph.Edges, startKey, endKey);
        }
    }
}
