using RailVision.Application.DTOs;
using RailVision.Application.DTOs.Overpass;

namespace RailVision.Application.Abstractions
{
    public interface ITerrainService
    {
        Task<OverpassResponseDTO> GetTerrainObstaclesDataAsync(string type = "", CancellationToken cancellationToken = default);
        Task<IEnumerable<ObstacleDTO>> GetTerrainObstaclesCoordsAsync(CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetNaturalTerrainObstaclesDataAsync(CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetManMadeTerrainObstaclesDataAsync(CancellationToken cancellationToken = default);
    }
}
