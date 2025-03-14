using RailVision.Domain.Entities.Base;
using RailVision.Domain.Entities.Coordinates;

namespace RailVision.Domain.Entities
{
    public class Obstacle : BaseEntity
    {
        public long ElementId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ICollection<ObstacleCoordinate> Coordinates { get; set; } = [];
    }
}
