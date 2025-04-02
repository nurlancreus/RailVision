using System.Diagnostics.CodeAnalysis;

namespace RailVision.Application.DTOs.Route
{
    public record RouteRequestDTO 
    {
        public long? FromId { get; set; }
        public long? ToId { get; set; }

        public CoordinateDTO? FromCoordinate { get; set; }
        public CoordinateDTO? ToCoordinate { get; set; }
    }
}
