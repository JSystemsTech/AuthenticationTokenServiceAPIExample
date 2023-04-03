using NETFramework48WebCookieManagementSandbox.App_Start;
using NETFramework48WebCookieManagementSandbox.AuthNAuthZ;
using NETFramework48WebCookieManagementSandbox.AuthNAuthZ.Authentication;
using NETFramework48WebCookieManagementSandbox.Extensions;
using System.Collections.Specialized;
using System.Configuration;
using System.Web.Mvc;

namespace NETFramework48WebCookieManagementSandbox
{
    public interface IConfigurationSection
    {
        string GetSetting(string key);
    }
    
    public interface IConfiguration
    {
        NameValueCollection GetSection(string name);
        TConfigSection GetSection<TConfigSection>(string name);
        string GetSetting(string name);
    }
    public enum EnvironmentType
    {
        Development,
        Test,
        Release
    }
    public sealed class AppConfiguration: IConfiguration
    {
        public NameValueCollection GetSection(string name) => GetSection<NameValueCollection>(name);
        public TConfigSection GetSection<TConfigSection>(string name) => ConfigurationManager.GetSection(name) is TConfigSection section ? section : default;

        public string GetSetting(string name) => ConfigurationManager.AppSettings.Get(name);

        public EnvironmentType Environment => GetSetting("Environment").ToEnum<EnvironmentType>();
        public bool IsDevelopmentEnvironment() => Environment == EnvironmentType.Development;
    }

    public sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IRuntimeServiceBuilder builder)
        {
            builder.ConfigureRoleProvider<WebAppRoleProvider>();            
            builder.AddAuthentication()
                .UseTokenServiceAuthentication(TokenServiceAuthentication.DefaultAuthenticationScheme, config =>
            {
                config.SessionMode = TokenServiceSessionMode.Static;
                config.SessionExpireWarningThreshold = 2;
                config.LoginRedirectUrl = Configuration.GetSetting("LoginRedirectUrl");
                config.LogoutRedirectUrl = Configuration.GetSetting("LogoutRedirectUrl");
                config.CookieName = Configuration.GetSetting("AuthCookieName");
                config.AppId = Configuration.GetSetting("SiteId");
                config.ApiUrlBase = Configuration.GetSetting("AuthenticationTokenServiceAPIBaseUrl");
            });
        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseBundles();
            app.UseAuthentication();
            app.UseFilters(filters => {
                filters.Add(new HandleErrorAttribute());
            });
            app.UseAreas();
            app.UseEndpoints(routes => {
                routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

                routes.MapRoute(
                    name: "SignIn",
                    url: "SignIn",
                    defaults: new { controller = "Home", action = "SignIn"}
                );
                routes.MapRoute(
                    name: "Logout",
                    url: "Logout",
                    defaults: new { controller = "Home", action = "Logout" }
                );
                routes.MapRoute(
                    name: "Default",
                    url: "{controller}/{action}/{id}",
                    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                );
            });
        }

       
    }
}