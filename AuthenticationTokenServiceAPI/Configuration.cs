using DbFacade.DataLayer.ConnectionService;
using DbFacade.Services;

namespace AuthenticationTokenServiceAPI
{
    public static class ApplicationConfiguration
    {
        public static ApplicationSettings ApplicationSettings = new ApplicationSettings();
        public static JwtSettings JwtSettings = new JwtSettings();
        public static SwaggerSettings SwaggerSettings = new SwaggerSettings();

        internal static ConfigurationManager UseStaticApplicationConfiguration(this ConfigurationManager configuration)
        {
            configuration.GetSection(ApplicationSettings.ConfigurationSection).Bind(ApplicationSettings);
            configuration.GetSection(JwtSettings.ConfigurationSection).Bind(JwtSettings);
            configuration.GetSection(SwaggerSettings.ConfigurationSection).Bind(SwaggerSettings);
            return configuration;
        }
        internal static IServiceCollection AddDbConnectionConfig<TDbConnectionConfig>(this IServiceCollection services)
            where TDbConnectionConfig : DbConnectionConfigBase
        => services.AddScoped<TDbConnectionConfig>();
        internal static IServiceProvider UseDbConnectionConfig<TDbConnectionConfig>(this IServiceProvider services)
            where TDbConnectionConfig : DbConnectionConfigBase
        {
            using (var scope = services.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<TDbConnectionConfig>().Register();
            }
            return services;
        }

    }
    public enum ApplicationEnvironment
    {
        Localhost,
        Development,
        Evaluation,
        Production
    }
    public class ApplicationSettings
    {
        public static string ConfigurationSection = "ApplicationSettings";
        public ApplicationEnvironment Environment { get; set; }
        public string EnvironmentName =>
            Environment == ApplicationEnvironment.Localhost ? "Localhost" :
            Environment == ApplicationEnvironment.Development ? "Development" :
            Environment == ApplicationEnvironment.Evaluation ? "Evaluation" :
            Environment == ApplicationEnvironment.Production ? "Production" :
            "Unknown";
        public string BasePath { get; set; } = string.Empty;

    }
    public class JwtSettings
    {
        public static string ConfigurationSection = "JwtSettings";

        public string EncryptionKey { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public int ValidFor { get; set; }
        public int RefreshAfter { get; set; }
        public int ValidForCalculated => ValidFor <= 0 ? 20 : ValidFor;
        public int RefreshAfterCalculated => RefreshAfter <= 0 || RefreshAfter >= ValidForCalculated ? ValidForCalculated : RefreshAfter;

    }
    public class SwaggerSettings
    {
        public static string ConfigurationSection = "SwaggerSettings";

        public bool UseSwaggerUI { get; set; }
        public bool UseDeveloperExceptionPage { get; set; }
        public string BasePath { get; set; } = "api/docs";
        public string APIName { get; set; } = "AuthTokenService";
        public string Version { get; set; } = "v1";


    }
}
