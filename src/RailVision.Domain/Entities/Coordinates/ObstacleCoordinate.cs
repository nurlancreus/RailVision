namespace RailVision.Domain.Entities.Coordinates
{
    public class ObstacleCoordinate : Coordinate
    {
        public Guid ObstacleId { get; set; }
        public Obstacle Obstacle { get; set; } = null!;
    }
}
