namespace RailVision.Application.DTOs
{
    public class CoordinateDTO
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Override Equals and GetHashCode to compare by latitude and longitude
        public override bool Equals(object? obj)
        {
            if (obj is CoordinateDTO other)
            {
                return Latitude == other.Latitude && Longitude == other.Longitude;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude);
        }

        public override string ToString()
        {
            return $"{Latitude:F6}_{Longitude:F6}";
        }
    }
}
