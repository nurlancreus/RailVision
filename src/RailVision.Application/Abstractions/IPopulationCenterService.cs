using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;

namespace RailVision.Application.Abstractions
{
    public interface IPopulationCenterService
    {
        Task<IEnumerable<PopulationCenterDTO>> GetAllAsync(string? searchQuery, int? minPopulation, int? maxPopulation, CancellationToken cancellationToken = default);
        Task<PopulationCenterDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PopulationCenterDTO> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetPopulationCentersDataAsync(CancellationToken cancellationToken = default);
    }
}
