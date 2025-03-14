using RailVision.Domain.Entities.Base;
using RailVision.Domain.Entities.Coordinates;

namespace RailVision.Domain.Entities
{
    public class Station : BaseEntity
    {
        public long ElementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public StationCoordinate Coordinate { get; set; } = null!;
    }
}
