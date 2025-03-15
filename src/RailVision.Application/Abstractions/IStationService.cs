using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;

namespace RailVision.Application.Abstractions
{
    public interface IStationService
    {
        Task<IEnumerable<StationDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<StationDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<StationDTO> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetStationsDataAsync(CancellationToken cancellationToken = default);
    }
}
