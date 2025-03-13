using RailVision.Application.DTOs.Overpass;

namespace RailVision.Application.Abstractions
{
    public interface ITerrainService
    {
        Task<OverpassResponseDTO> GetTerrainObstaclesDataAsync(string type = "natural", CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetTerrainObstaclesCoordsAsync(CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetNaturalTerrainObstaclesDataAsync(CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetManMadeTerrainObstaclesDataAsync(CancellationToken cancellationToken = default);
    }
}
