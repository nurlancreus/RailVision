using RailVision.Domain.Entities.Base;

namespace RailVision.Domain.Entities.Coordinates
{
    public class Coordinate : BaseEntity
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
