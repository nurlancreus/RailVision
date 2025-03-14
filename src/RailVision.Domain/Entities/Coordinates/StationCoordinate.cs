namespace RailVision.Domain.Entities.Coordinates
{
    public class StationCoordinate : Coordinate
    {
        public Guid StationId { get; set; }
        public Station Station { get; set; } = null!;
    }
}
