using NETFramework48WebCookieManagementSandbox.App_Start;
using System;

namespace NETFramework48WebCookieManagementSandbox
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static ApplicationBuilder ApplicationBuilder { get; set; }
        private static Startup Startup { get; set; }
        
        protected void Application_Start()
        {
            Startup = new Startup(new AppConfiguration());
            Startup.ConfigureServices(RuntimeService.Builder);
            ApplicationBuilder = new ApplicationBuilder(RuntimeService.Services);
            Startup.Configure(ApplicationBuilder);
            ApplicationBuilder.OnApplicationStart();
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            RuntimeService.UpdateTransientServices();
        }
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            ApplicationBuilder.OnAuthentication();
        }
    }
}
