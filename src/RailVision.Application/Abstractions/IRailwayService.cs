using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;

namespace RailVision.Application.Abstractions
{
    public interface IRailwayService
    {
        Task<IEnumerable<RailwayLineDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<RailwayLineDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<RailwayLineDTO> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetRailwaysDataAsync(CancellationToken cancellationToken = default);
    }
}
