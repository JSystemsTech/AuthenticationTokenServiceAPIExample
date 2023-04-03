using Microsoft.Ajax.Utilities;
using Microsoft.SqlServer.Server;
using NETFramework48WebCookieManagementSandbox.AuthNAuthZ.Authentication;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace NETFramework48WebCookieManagementSandbox.App_Start
{

    public interface IApplicationBuilder
    {
        void UseAreas();
        void UseEndpoints(Action<RouteCollection> routeBuilder);
        void UseFilters(Action<GlobalFilterCollection> filterBuilder);
        void UseAuthentication();
        void UseBundles(string filePath = ApplicationBuilder.DefaultConfigfilePath);
    }
    internal class ApplicationBuilder: IApplicationBuilder
    {
        private readonly IRuntimeServiceProvider _services;
        private Action<GlobalFilterCollection> _filterBuilder { get; set; }
        private Action<RouteCollection> _routeBuilder { get; set; }
        private bool _useAreas { get; set; }
        private bool _useAuthentication { get; set; }
        public ApplicationBuilder(IRuntimeServiceProvider services ) {
            _services = services;
        }
        public void OnApplicationStart()
        {
            if (_useAreas)
            {
                AreaRegistration.RegisterAllAreas();
            }
            if (_filterBuilder != null)
            {
                _filterBuilder(GlobalFilters.Filters);
            }
            if (_routeBuilder != null)
            {
                _routeBuilder(RouteTable.Routes);
            }
        }
        public void OnAuthentication()
        {
            if (_useAuthentication)
            {
                _services.GetRequiredService<IApplicationAuthenticationManager>().Authenticate();
            }
        }
        public void UseAreas()
        {
            _useAreas = true;
        }
        public void UseEndpoints(Action<RouteCollection> routeBuilder)
        {
            _routeBuilder = routeBuilder;
        }
        public void UseFilters(Action<GlobalFilterCollection> filterBuilder)
        {
            _filterBuilder = filterBuilder;
        }
        public void UseAuthentication()
        {
            _useAuthentication = true;
        }
        internal const string DefaultConfigfilePath = "~/bundle.config.json";
        public void UseBundles(string filePath = DefaultConfigfilePath)
        {
            var config = BundleConfiguration.Load(filePath);
            if (config.Scripts != null && config.Scripts.Count() > 0)
            {
                config.Scripts.ConfigureScriptBundle();
            }
            if (config.Styles != null && config.Styles.Count() > 0)
            {
                config.Styles.ConfigureStylesBundle();
            }
            if (config.RouteScripts != null &&  config.RouteScripts.Count() > 0)
            {
                foreach (var routeScript in config.RouteScripts)
                {
                    string[] combined = config.Scripts.Concat(routeScript.Scripts).ToArray();
                    combined.ConfigureRouteScriptBundle(routeScript.Path);
                }

            }
        }
    }

    //BundleTable.Bundles.Any(y => y.Path == bundleName)
    internal class RouteScriptsConfiguration
    {
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("scripts")]
        public string[] Scripts { get; set; }
    }
    internal class BundleConfiguration
    {
        [JsonProperty("scripts")]
        public string[] Scripts { get; set; }
        [JsonProperty("styles")]
        public string[] Styles { get; set; }

        [JsonProperty("routeScripts")]
        public RouteScriptsConfiguration[] RouteScripts { get; set; }
        public static BundleConfiguration Load(string filePath)
        {
            try
            {
                string json = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath(filePath));
                var config = JsonConvert.DeserializeObject<BundleConfiguration>(json);
                return config;
            }
            catch
            {
                return new BundleConfiguration() { Scripts = new string[0], Styles = new string[0] };
            }
        }

    }
    public static class BundleConfigurationExtensions
    {
        internal static void ConfigureScriptBundle(this string[] scripts)
            => BundleTable.Bundles.Add(new Bundle("~/bundles/scripts").Include(scripts));
        internal static void ConfigureStylesBundle(this string[] styles)
            => BundleTable.Bundles.Add(new Bundle("~/bundles/styles").Include(styles));
        internal static void ConfigureRouteScriptBundle(this string[] scripts, string path)
            => BundleTable.Bundles.Add(new Bundle($"~/bundles/routeScripts/{path}").Include(scripts));

        public static IHtmlString LoadScripts(this HtmlHelper helper)
        {            
            try
            {
                string path = helper.ViewContext.HttpContext.Request.Url.LocalPath;
                if (BundleTable.Bundles.Any(y => y.Path == $"~/bundles/routeScripts/{path}"))
                {
                    return Scripts.Render($"~/bundles/routeScripts/{path}");
                }
                return Scripts.Render("~/bundles/scripts");
            }
            catch
            {
                return new HtmlString("");
            }
        }
        public static IHtmlString LoadStyles(this HtmlHelper helper) => Styles.Render("~/bundles/styles");
    }
}