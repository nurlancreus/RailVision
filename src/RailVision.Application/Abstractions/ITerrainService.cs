using RailVision.Application.DTOs;
using RailVision.Application.DTOs.Overpass;

namespace RailVision.Application.Abstractions
{
    public interface ITerrainService
    {
        Task<IEnumerable<ObstacleDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ObstacleDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ObstacleDTO> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetTerrainObstaclesDataAsync(string type = "", CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetNaturalTerrainObstaclesDataAsync(CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetManMadeTerrainObstaclesDataAsync(CancellationToken cancellationToken = default);
    }
}
