using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;

namespace RailVision.Application.Abstractions
{
    public interface IRailwayService
    {
        Task<OverpassResponseDTO> GetRailwaysDataAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<RailwayLineDTO>> GetRailwayLinesAsync(CancellationToken cancellationToken = default);
    }
}
