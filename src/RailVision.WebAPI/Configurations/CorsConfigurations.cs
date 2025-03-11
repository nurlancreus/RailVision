namespace RailVision.WebAPI.Configurations
{
    public static class CorsConfigurations
    {
        public static WebApplicationBuilder ConfigureCORS(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(Constants.CorsPolicies.AllowAllPolicy, builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            return builder;
        }
    }
}