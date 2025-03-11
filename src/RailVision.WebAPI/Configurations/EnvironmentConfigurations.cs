namespace RailVision.WebAPI.Configurations
{
    public static class EnvironmentConfigurations
    {
        public static IConfigurationBuilder ConfigureEnvironments<T>(this IConfigurationBuilder configurationBuilder, IHostEnvironment hostEnvironment) where T : class
        {
            configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<T>(); // Load User Secrets in ALL environments

            return configurationBuilder;
        }
    }
}
