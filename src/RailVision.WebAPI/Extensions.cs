namespace RailVision.WebAPI
{
    public static class Extensions
    {
        public static bool IsTesting(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            return hostEnvironment.IsEnvironment("Testing");
        }
    }
}
