using AuthenticationTokenServiceAPI.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;

namespace AuthNAuthZ.Authentication
{
    public static class TokenServiceAuthentication
    {
        public const string DefaultAuthenticationScheme = "__TokenServiceAuthentication";
        private static TokenServiceAuthenticationProvider _Provider;

        public static void SignIn(Guid userGuid)
        {
            if(_Provider != null)
            {
                _Provider.SignIn(userGuid);
            }
        }
        public static string SignOut()
        => _Provider != null ? _Provider.SignOut(): null;
        public static IApplicationAuthenticationManager UseTokenServiceAuthentication(this IApplicationAuthenticationManager manager , string authenticationScheme, Action<TokenServiceAuthenticationProviderConfig> handler)
        {
            _Provider = new TokenServiceAuthenticationProvider(TokenServiceAuthenticationProviderConfig.Create(handler), authenticationScheme);
            manager.AddScheme(_Provider);
            return manager;
        }
    }
    public enum TokenServiceSessionMode
    {
        Static,
        Mixed,
        Sliding
    }


    public class TokenServiceAuthenticationProviderConfig
    {
        public string CookieName { get; set; }
        public string LoginRedirectUrl { get; set; }
        public string LogoutRedirectUrl { get; set; }


        public string ApiUrlBase { get; set; }
        public string AppId { get; set; }

        public string SSOTokenQueryParameterName { get; set; }
        public string CacTokenParameterName { get; set; }

        public TokenServiceSessionMode SessionMode { get; set; }
        public int SessionExpireWarningThreshold { get; set; }
        public int MixedModeThreshold { get; set; }


        private static string DefaultCookieName = "__TokenServiceAuthenticationToken";
        private static string DefaultSSOTokenQueryParameterName = "__SSOToken";
        private static string DefaultCacTokenParameterName = "__CacToken";
        internal static TokenServiceAuthenticationProviderConfig Create(Action<TokenServiceAuthenticationProviderConfig> handler) { 
            var config = new TokenServiceAuthenticationProviderConfig();
            handler(config);
            config.Init();
            return config;
        }
        private void Init()
        {
            SSOTokenQueryParameterName = string.IsNullOrWhiteSpace(SSOTokenQueryParameterName) ? DefaultSSOTokenQueryParameterName : SSOTokenQueryParameterName;
            CacTokenParameterName = string.IsNullOrWhiteSpace(SSOTokenQueryParameterName) ? DefaultCacTokenParameterName : CacTokenParameterName;
#if DEBUG
            CookieName = string.IsNullOrWhiteSpace(CookieName) ? DefaultCookieName : CookieName;
#else
            CookieName = $"{(string.IsNullOrWhiteSpace(CookieName) ? DefaultCookieName : CookieName)}_{Guid.NewGuid().ToString().Replace("-", "")}";
#endif
        }
    }
    public interface ITokenServiceRoleProvider : IRoleProvider
    {
        string[] GetRolesFromUserData(UserData data);
    }
    public interface ITokenServicePrincipal
    {
        DateTime SessionExpirationDate { get; }
        bool IsPublicUser { get; }
        UserData UserData { get; }
    }
    internal class TokenServicePrincipal : IPrincipal, ITokenServicePrincipal
    {

        public static TokenServicePrincipal PublicUser = new TokenServicePrincipal() { 
            _Roles = new string[0],
            IsPublicUser = true, 
            Identity = new TokenServiceIdentity { Name = "Application Public User" } 
        };
        private static ITokenServiceRoleProvider RoleProvider => ApplicationAuthenticationManagerExtnesions.GetRoleProvider<ITokenServiceRoleProvider>();
        private TokenServiceAuthenticationProviderConfig _Config { get; set; }
        public DateTime SessionExpirationDate { get; set; }
        public bool IsPublicUser { get; private set; }
        public bool ShowSessionWarning => !IsPublicUser && _Config != null ? (SessionExpirationDate - DateTime.UtcNow).TotalMinutes < _Config.SessionExpireWarningThreshold : false;

        public IIdentity Identity { get; private set; }
        public UserData UserData { get; private set; }

        private IEnumerable<string> _Roles { get; set; }

        private TokenServicePrincipal() { }
        public TokenServicePrincipal(UserData data, TokenServiceAuthenticationProviderConfig config)
        {
            UserData = data;
            Identity = new TokenServiceIdentity { Name = $"{data.FirstName} {data.MiddleInitial} {data.LastName}" };
            _Roles = RoleProvider.GetRolesFromUserData(data);
            _Config = config;
        }
        public bool IsInRole(string role)
        => _Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase);


        private class TokenServiceIdentity : IIdentity
        {
            public string Name { get; internal set;}

            public string AuthenticationType => "TokenService";

            public bool IsAuthenticated => true;
        }

    }
    internal class TokenServiceAuthenticationProvider : IAuthenticationProvider
    {
        private readonly TokenServiceAuthenticationProviderConfig _Config;
        private readonly AuthenticationTokenServiceAPI.AuthenticationTokenServiceAPIClient _APIclient;


        public string AuthenticationScheme { get; private set; }

        public TokenServiceAuthenticationProvider(TokenServiceAuthenticationProviderConfig config, string authenticationScheme)
        {
            _Config = config;
            AuthenticationScheme = authenticationScheme;
            _APIclient = new AuthenticationTokenServiceAPI.AuthenticationTokenServiceAPIClient(() => {
                //check Auth cookie for value
                if (HttpContext.Current.Request.Cookies[_Config.CookieName] is HttpCookie httpCookie)
                {
                    return httpCookie.Value;
                }
                //check configured Request SSO token query parameter for value
                else if (HttpContext.Current.Request.QueryString[config.SSOTokenQueryParameterName] is string ssoTokenParam && !string.IsNullOrWhiteSpace(ssoTokenParam))
                {
                    return ssoTokenParam;
                }
                
                return null;
            }, config.ApiUrlBase, config.AppId);
        }

        public void SignIn(Guid userGuid)
        {
            try
            {
                var response = _APIclient.SignInWithUserIdentifier(userGuid);
                if (!response.HasError)
                {
                    AddHttpCookie(response.Value.Token, response.Value.ExpiresUtc.UtcDateTime);
                }  
            }
            catch (Exception ex)
            {
               // handle error

            }
            
        }
        public string SignOut()
        {
            RemoveHttpCookie();
            return _Config.LoginRedirectUrl;
        }

        public bool Authenticate(HttpContext httpContext)
        {
            httpContext.User = TokenServicePrincipal.PublicUser;
            try
            {
                //Check for presense of CAC token
                if (HttpContext.Current.Request.QueryString[_Config.CacTokenParameterName] is string cacToken && !string.IsNullOrWhiteSpace(cacToken))
                {
                    var cacResolveResponse = _APIclient.SignInWithCacToken(cacToken);
                    if(cacResolveResponse.HasError )
                    {
                        return false;
                    }
                    httpContext.User = new TokenServicePrincipal(cacResolveResponse.Value.UserData, _Config);
                    AddHttpCookie(cacResolveResponse.Value.Token, cacResolveResponse.Value.ExpiresUtc.UtcDateTime);
                    return true;
                }

                var response = _APIclient.GetUserData();
                if(response.HasError)
                {
                    return false;
                }
                httpContext.User = new TokenServicePrincipal(response.Value, _Config);
                var currentCookie = HttpContext.Current.Request.Cookies[_Config.CookieName];
                if(_Config.SessionMode == TokenServiceSessionMode.Sliding)
                {
                    var renewSlidingResponse = _APIclient.RenewSession();
                    AddHttpCookie(renewSlidingResponse.Value.Token, renewSlidingResponse.Value.ExpiresUtc.UtcDateTime);
                    return true;
                }
                else if (_Config.SessionMode == TokenServiceSessionMode.Mixed)
                {
                    TimeSpan ts = ((ITokenServicePrincipal)httpContext.User).SessionExpirationDate - DateTime.UtcNow;
                    if (ts.TotalMinutes < _Config.MixedModeThreshold)
                    {
                        var renewSlidingResponse = _APIclient.RenewSession();
                        AddHttpCookie(renewSlidingResponse.Value.Token, renewSlidingResponse.Value.ExpiresUtc.UtcDateTime);
                        return true;
                    }                    
                }
                AddHttpCookie(currentCookie.Value, ((ITokenServicePrincipal)httpContext.User).SessionExpirationDate);
                return true;
            }
            catch(Exception ex)
            {
                return false;

            }
        }

        private void AddHttpCookie(string value, DateTime expires)
        {
            HttpCookie HttpCookie = new HttpCookie(_Config.CookieName, value)
            {
                Expires = expires,
                HttpOnly = true
            };
            HttpContext.Current.Response.Cookies.Add(HttpCookie);
        }
        private void RemoveHttpCookie()
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(_Config.CookieName, string.Empty) { Expires = DateTime.Now.AddDays(-1) });
        }
    }
}