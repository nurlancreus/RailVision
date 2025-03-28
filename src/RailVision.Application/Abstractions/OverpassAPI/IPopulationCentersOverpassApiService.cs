using RailVision.Application.DTOs.Overpass;

namespace RailVision.Application.Abstractions.OverpassAPI
{
    public interface IPopulationCentersOverpassApiService
    {
        Task<OverpassResponseDTO> GetPopulationCentersAsync(CancellationToken cancellationToken = default);
    }
}
