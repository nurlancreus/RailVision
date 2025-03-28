using RailVision.Domain.Entities.Base;
using RailVision.Domain.Entities.Coordinates;

namespace RailVision.Domain.Entities
{
    public class PopulationCenter : BaseEntity
    {
        public long ElementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Population { get; set; }
        public PopulationCenterCoordinate Coordinate { get; set; } = null!;
    }
}
