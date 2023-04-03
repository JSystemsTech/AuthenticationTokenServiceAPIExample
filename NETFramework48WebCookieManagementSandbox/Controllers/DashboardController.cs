using NETFramework48WebCookieManagementSandbox.AuthNAuthZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NETFramework48WebCookieManagementSandbox.Controllers
{
    [AuthorizeUser]
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}