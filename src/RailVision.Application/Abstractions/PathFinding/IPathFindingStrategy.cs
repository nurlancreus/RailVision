namespace RailVision.Application.Abstractions.PathFinding
{
    public interface IPathFindingStrategy
    {
        List<string> FindPath(Graph graph, string startKey, string endKey);
    }
}
