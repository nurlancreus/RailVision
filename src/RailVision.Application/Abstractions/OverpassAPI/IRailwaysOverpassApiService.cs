using RailVision.Application.DTOs.Overpass;

namespace RailVision.Application.Abstractions.OverpassAPI
{
    public interface IRailwaysOverpassApiService
    {
        Task<OverpassResponseDTO> GetRailwaysDataAsync(CancellationToken cancellationToken = default);
    }
}
