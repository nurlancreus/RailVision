using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs.Railways;
using System.Threading.Tasks;

namespace RailVision.Application.Abstractions
{
    public interface IRailwayService
    {
        Task<OverpassResponseDTO> GetRailwayDataAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<RailwayLineDTO>> GetRailwayLinesAsync(CancellationToken cancellationToken = default);
    }
}
