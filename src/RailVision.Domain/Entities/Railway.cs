using RailVision.Domain.Entities.Base;
using RailVision.Domain.Entities.Coordinates;

namespace RailVision.Domain.Entities
{
    public class Railway : BaseEntity
    {
        public long ElementId { get; set; }
        public ICollection<RailwayCoordinate> Coordinates { get; set; } = [];
    }
}
