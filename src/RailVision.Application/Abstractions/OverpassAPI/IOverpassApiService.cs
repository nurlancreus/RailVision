namespace RailVision.Application.Abstractions.OverpassAPI
{
    public interface IOverpassApiService : IStationsOverpassApiService, IRailwaysOverpassApiService, IPopulationCentersOverpassApiService, ITerrainsOverpassApiService { }
}
