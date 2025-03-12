using RailVision.Application.DTOs.Overpass;

namespace RailVision.Application.Abstractions
{
    public interface IOverpassApiService
    {
        Task<OverpassResponseDTO> GetRailwaysDataAsync(CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetStationsDataAsync(CancellationToken cancellationToken = default);
    }
}
