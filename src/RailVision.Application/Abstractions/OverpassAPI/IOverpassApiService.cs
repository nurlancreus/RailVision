using RailVision.Application.DTOs.Overpass;

namespace RailVision.Application.Abstractions.OverpassAPI
{
    public interface IOverpassApiService : IStationsOverpassApiService, IRailwaysOverpassApiService, ITerrainsOverpassApiService { }
}
