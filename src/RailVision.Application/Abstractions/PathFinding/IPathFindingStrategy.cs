using Microsoft.AspNetCore.Hosting;

namespace RailVision.Application.Abstractions.PathFinding
{
    public interface IPathFindingStrategy
    {
        List<(string node, double distance)> FindPath(Graph graph, string startKey, string endKey, IWebHostEnvironment webHostEnvironment);
    }
}
