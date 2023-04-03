using AuthenticationTokenServiceAPI;
using NETFramework48WebCookieManagementSandbox.AuthNAuthZ.Authentication;
using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace NETFramework48WebCookieManagementSandbox.AuthNAuthZ
{
    public class WebAppRoleProvider : ITokenServiceRoleProvider
    {
        public string[] GetRolesFromUserData(UserData data)
        {
            return new string[] { "Test" };
        }
    }
    internal static class PrincipalExtensions
    {
        public static bool IsAllowedPrincipal(this IPrincipal user)
        => user is ITokenServicePrincipal tsUser && !tsUser.IsPublicUser;
        public static bool IsPublicUser (this IPrincipal user)
        => user is ITokenServicePrincipal tsUser && tsUser.IsPublicUser;
        public static bool IsAllowedPrincipal(this HtmlHelper html)
        => html.ViewContext.HttpContext.User.IsAllowedPrincipal();
        public static bool IsPublicUser(this HtmlHelper html)
        => html.ViewContext.HttpContext.User.IsPublicUser();
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext.User.IsAllowedPrincipal();
        }
        
    }
    public sealed class AllowPublicUserAttribute : AuthorizeUserAttribute
    {
        protected sealed override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext.User.IsPublicUser() || base.AuthorizeCore(httpContext);
        }
    }
}