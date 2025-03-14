namespace RailVision.Domain.Entities.Coordinates
{
    public class RailwayCoordinate : Coordinate
    {
        public Guid RailwayId { get; set; }
        public Railway Railway { get; set; } = null!;
    }
}
