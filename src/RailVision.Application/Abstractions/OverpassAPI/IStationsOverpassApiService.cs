using RailVision.Application.DTOs.Overpass;

namespace RailVision.Application.Abstractions.OverpassAPI
{
    public interface IStationsOverpassApiService
    {
        Task<OverpassResponseDTO> GetStationsDataAsync(CancellationToken cancellationToken = default);
    }
}
