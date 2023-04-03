using NETFramework48WebCookieManagementSandbox.AuthNAuthZ;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace NETFramework48WebCookieManagementSandbox.Extensions.HtmlHelpers
{
    public static class  HtmlHelpersBase
    {
        public static string UniqueId(this HtmlHelper html)
        => $"uid{Guid.NewGuid().ToString().Replace("-", "")}";

        public static IHtmlString AllowedUserContent(this HtmlHelper html, Func<IHtmlString> resolver)
        => html.IsPublicUser() ? new HtmlString("") : resolver();

        public static IHtmlString LogoutActionLink(this HtmlHelper html)
        => html.AllowedUserContent(() => html.RouteLink("Logout", "Logout", new { }, new { @class = "btn btn-link" }));
        

        private static IHtmlString ToHtml(this object content, bool isHtmlSafe = false)
        => content is IHtmlString html ? html : 
            new HtmlString(isHtmlSafe ? content.ToString() : HttpUtility.HtmlEncode(content));

        public static IHtmlString NavItem(this HtmlHelper html, IHtmlString content)
        => new HtmlString($"<li class=\"nav-item\">{content}</li>");
        public static IHtmlString NavItem(this HtmlHelper html, string content)
        => html.NavItem(content.ToHtml());
        public static IHtmlString ActionLinkNavItem(this HtmlHelper html, string label, string action)
        => html.NavItem(html.ActionLink(label, action, new { }, new { @class = "nav-link" }));
        public static IHtmlString ActionLinkNavItem(this HtmlHelper html, string label, string action, string controller)
        => html.NavItem(html.ActionLink(label, action, controller, new { }, new { @class = "nav-link" }));
        public static IHtmlString RouteLinkNavItem(this HtmlHelper html, string label, string routeName)
        => html.NavItem(html.RouteLink(label, routeName, new { }, new { @class = "nav-link" }));
        public static IHtmlString LogoutNavItem(this HtmlHelper html)
        => html.AllowedUserContent(() => html.RouteLinkNavItem("Logout", "Logout"));

        public static IHtmlString AllowedUserActionLinkNavItem(this HtmlHelper html, string label, string action)
        => html.AllowedUserContent(() => html.ActionLinkNavItem(label, action));
        public static IHtmlString AllowedUserActionLinkNavItem(this HtmlHelper html, string label, string action, string controller)
        => html.AllowedUserContent(() => html.ActionLinkNavItem(label, action, controller));

        public static IHtmlString ExternalLink(this HtmlHelper html, object content, string url)
        => new HtmlString($"<a href=\"{url.ToHtml()}\" class=\"nav-link\" rel = \"noopener noreferrer\">{content.ToHtml()}</a>");
        private static string SSOToken(this HtmlHelper html)
        => html.ViewContext.HttpContext.User is AuthNAuthZ.Authentication.ITokenServicePrincipal user ? user.SSOToken : "";
        public static IHtmlString LinkToSSOApp(this HtmlHelper html, object content, string url, string ssoTokenName, string btnClass= "btn btn-link")
        => html.AllowedUserContent(() => new HtmlString($"<a href=\"{url.AppendQueryParameter(ssoTokenName, html.SSOToken()).ToHtml()}\" class=\"{btnClass}\" rel=\"noopener noreferrer\">{content.ToHtml()}</a>"));

        public static IHtmlString LinkToSSOAppNavItem(this HtmlHelper html, object content, string url, string ssoTokenName)
        => html.AllowedUserContent(() => html.LinkToSSOApp(content, url, ssoTokenName, "nav-link"));
    }
}