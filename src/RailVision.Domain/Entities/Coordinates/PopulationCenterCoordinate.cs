namespace RailVision.Domain.Entities.Coordinates
{
    public class PopulationCenterCoordinate : Coordinate
    {
        public Guid PopulationCenterId { get; set; }
        public PopulationCenter PopulationCenter { get; set; } = null!;
    }
}
