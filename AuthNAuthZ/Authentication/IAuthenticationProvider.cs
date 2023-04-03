using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AuthNAuthZ.Authentication
{
    public interface IAuthenticationProvider
    {
        string AuthenticationScheme { get; }
        bool Authenticate(HttpContext httpContext); 
    }
    public interface IRoleProvider { }
}
