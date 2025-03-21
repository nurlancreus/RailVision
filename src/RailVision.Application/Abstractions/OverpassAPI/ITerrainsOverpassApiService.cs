﻿using RailVision.Application.DTOs.Overpass;

namespace RailVision.Application.Abstractions.OverpassAPI
{
    public interface ITerrainsOverpassApiService
    {
        Task<OverpassResponseDTO> GetTerrainObstaclesDataAsync(CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetNaturalTerrainObstaclesDataAsync(CancellationToken cancellationToken = default);
        Task<OverpassResponseDTO> GetManMadeTerrainObstaclesDataAsync(CancellationToken cancellationToken = default);
    }
}
