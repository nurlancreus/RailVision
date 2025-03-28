using RailVision.Application.DTOs;
using RailVision.Domain.Entities;

namespace RailVision.Application.Helpers
{
    public static class Mappings
    {
        public static PopulationCenterDTO ToPopulationCenterDTO(this PopulationCenter populationCenter)
        {
            return new PopulationCenterDTO
            {
                Id = populationCenter.Id,
                ElementId = populationCenter.ElementId,
                Name = populationCenter.Name,
                Population = populationCenter.Population,
                Coordinate = new CoordinateDTO
                {
                    Latitude = populationCenter.Coordinate.Latitude,
                    Longitude = populationCenter.Coordinate.Longitude
                }
            };
        }
        public static RailwayLineDTO ToRailwayDTO(this Railway railway)
        {
            return new RailwayLineDTO
            {
                Id = railway.Id,
                ElementId = railway.ElementId,
                Coordinates = railway.Coordinates.Select(coord => new CoordinateDTO
                {
                    Latitude = coord.Latitude,
                    Longitude = coord.Longitude
                }).ToList()
            };
        }

        public static StationDTO ToStationDTO(this Station station)
        {
            return new StationDTO
            {
                Id = station.Id,
                ElementId = station.ElementId,
                Name = station.Name,
                Coordinate = new CoordinateDTO
                {
                    Latitude = station.Coordinate.Latitude,
                    Longitude = station.Coordinate.Longitude
                }
            };
        }

        public static ObstacleDTO ToObstacleDTO(this Obstacle obstacle)
        {
            var obstacleDto = new ObstacleDTO
            {
                Id = obstacle.Id,
                ElementId = obstacle.ElementId,
                Name = obstacle.Name,
                Type = obstacle.Type,
            };
            if (obstacle.Coordinates.Count == 1)
            {
                obstacleDto.Coordinate = new CoordinateDTO
                {
                    Latitude = obstacle.Coordinates.First().Latitude,
                    Longitude = obstacle.Coordinates.First().Longitude
                };
            }
            else obstacleDto.Coordinates = obstacle.Coordinates.Select(c => new CoordinateDTO
            {
                Latitude = c.Latitude,
                Longitude = c.Longitude
            });
            return obstacleDto;
        }
    }
}
