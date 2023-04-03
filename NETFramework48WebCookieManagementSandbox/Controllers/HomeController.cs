using NETFramework48WebCookieManagementSandbox.AuthNAuthZ;
using NETFramework48WebCookieManagementSandbox.AuthNAuthZ.Authentication;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NETFramework48WebCookieManagementSandbox.Controllers
{
    public static class ControllerExtensions
    {
        
        private sealed class DynamicViewDataDictionary : DynamicObject
        {
            private readonly Func<ViewDataDictionary> _viewDataThunk;

            private ViewDataDictionary ViewData => _viewDataThunk();

            public DynamicViewDataDictionary(Func<ViewDataDictionary> viewDataThunk)
            {
                _viewDataThunk = viewDataThunk;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return ViewData.Keys;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = ViewData[binder.Name];
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                ViewData[binder.Name] = value;
                return true;
            }
        }
        private static ViewResult GetView(string viewName, Action<dynamic> viewBagHandler, object model= null)
        {
            ViewDataDictionary ViewData = new ViewDataDictionary();
            DynamicViewDataDictionary ViewBag = new DynamicViewDataDictionary(() => ViewData);
            if (model != null)
            {
                ViewData.Model = model;
            }
            viewBagHandler(ViewBag);

            return new ViewResult
            {
                ViewName = viewName,
                MasterName = null,
                ViewData = ViewData,
                TempData = new TempDataDictionary(),
                ViewEngineCollection = ViewEngines.Engines
            };
        }
        private static ViewResult GetView(Controller controller, string viewName, object model = null)
        {
            if (model != null)
            {
                controller.ViewData.Model = model;
            }
            return new ViewResult
            {
                ViewName = viewName,
                MasterName = null,
                ViewData = controller.ViewData,
                TempData = controller.TempData,
                ViewEngineCollection = controller.ViewEngineCollection
            };
        }
        private static UrlHelper GetUrlHelper()
        => new UrlHelper(HttpContext.Current.Request.RequestContext);
        public static ActionResult CompleteAndRedirect(string url, string label, string title)
        {            
            return GetView("Redirect", viewBag => {
                viewBag.RedirectUrl = url;
                viewBag.Title = title;
                viewBag.Label = label;
            });
        }
        public static ActionResult CompleteAndRedirectToAction(string action, string label, string title)
        => CompleteAndRedirect(GetUrlHelper().Action(action), label, title);
        public static ActionResult CompleteAndRedirectToAction(string action, string controllerName, string label, string title)
        => CompleteAndRedirect(GetUrlHelper().Action(action, controllerName), label, title);
        public static ActionResult CompleteAndRedirectToAction(string action, string controllerName, object routeValues, string label, string title)
        => CompleteAndRedirect(GetUrlHelper().Action(action, controllerName, routeValues), label, title);

        public static ActionResult CompleteAndRedirect(this Controller controller, string url, string label, string title)
        {
            controller.ViewBag.RedirectUrl = url;
            controller.ViewBag.Title = title;
            controller.ViewBag.Label = label;

            return GetView(controller,"Redirect");
        }
        public static ActionResult CompleteAndRedirectToAction(this Controller controller, string action, string label, string title)
        => controller.CompleteAndRedirect(GetUrlHelper().Action(action), label, title);
        public static ActionResult CompleteAndRedirectToAction(this Controller controller, string action, string controllerName, string label, string title)
        => controller.CompleteAndRedirect(GetUrlHelper().Action(action, controllerName), label, title);
        public static ActionResult CompleteAndRedirectToAction(this Controller controller, string action, string controllerName, object routeValues, string label, string title)
        => controller.CompleteAndRedirect(GetUrlHelper().Action(action, controllerName, routeValues), label, title);
    }

    public class HomeController : Controller
    {
        [AllowPublicUser]
        public ActionResult Index()
        {
            return View();
        }
        
        [AllowPublicUser]
        public ActionResult SignIn()
        {
            TokenServiceAuthentication.SignIn(Guid.NewGuid());
            return this.CompleteAndRedirectToAction("Index", "Dashboard", "Logging In", "Sign In");
        }
        [AllowPublicUser]
        public ActionResult LogOut()
        {
            TokenServiceAuthentication.SignOut();
            Session.Abandon();
            return this.CompleteAndRedirectToAction("Index", "Logging out", "Sign Out");
        }
    }
}