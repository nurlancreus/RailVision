using RailVision.Application.Abstractions.PathFinding;
using RailVision.Application.Helpers;
using RailVision.Application;
using Microsoft.AspNetCore.Hosting;

namespace RailVision.Infrastructure.Services.PathFinding
{
    public class AStarPathFinding : IPathFindingStrategy
    {
        public List<(string node, double distance)> FindPath(Graph graph, string startKey, string endKey, IWebHostEnvironment webHostEnvironment)
        {
            return Algorithms.AStar(graph, startKey, endKey, graph.Nodes);
        }
    }
}
