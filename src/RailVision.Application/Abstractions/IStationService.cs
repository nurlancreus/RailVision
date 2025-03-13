using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;

namespace RailVision.Application.Abstractions
{
    public interface IStationService
    {
        Task<OverpassResponseDTO> GetStationsDataAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<StationDTO>> GetStationCoordsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<StationDTO>> GetStationNamesAsync(CancellationToken cancellationToken = default);
    }
}
