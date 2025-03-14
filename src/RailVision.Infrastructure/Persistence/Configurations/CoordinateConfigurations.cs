using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailVision.Domain.Entities.Coordinates;

namespace RailVision.Infrastructure.Persistence.Configurations
{
    public class CoordinateConfigurations : IEntityTypeConfiguration<Coordinate>
    {
        public void Configure(EntityTypeBuilder<Coordinate> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasDiscriminator<string>("Type")
                   .HasValue<RailwayCoordinate>("Railway")
                   .HasValue<StationCoordinate>("Station")
                   .HasValue<ObstacleCoordinate>("Obstacle");

            builder.ToTable(bi => bi.HasCheckConstraint("CK_Coordinates_Latitude", "[Latitude] >= -90 AND [Latitude] <= 90"));
            
            builder.ToTable(bi => bi.HasCheckConstraint("CK_Coordinates_Longitude", "[Longitude] >= -180 AND [Longitude] <= 180"));

        }
    }

    public class RailwayCoordinateConfigurations : IEntityTypeConfiguration<RailwayCoordinate>
    {
        public void Configure(EntityTypeBuilder<RailwayCoordinate> builder)
        {
            builder.HasOne(c => c.Railway)
                   .WithMany(r => r.Coordinates)
                   .HasForeignKey(c => c.RailwayId);
        }
    }

    public class StationCoordinateConfigurations : IEntityTypeConfiguration<StationCoordinate>
    {
        public void Configure(EntityTypeBuilder<StationCoordinate> builder)
        {
            builder.HasOne(c => c.Station)
                   .WithOne(s => s.Coordinate)
                   .HasForeignKey<StationCoordinate>(c => c.StationId);
        }
    }

    public class ObstacleCoordinateConfigurations : IEntityTypeConfiguration<ObstacleCoordinate>
    {
        public void Configure(EntityTypeBuilder<ObstacleCoordinate> builder)
        {

            builder.HasOne(c => c.Obstacle)
                   .WithMany(o => o.Coordinates)
                   .HasForeignKey(c => c.ObstacleId);
        }
    }
}
