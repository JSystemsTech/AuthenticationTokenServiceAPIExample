using System.Net.Http.Headers;

namespace AuthenticationTokenServiceAPI
{
    public partial class AuthenticationTokenServiceAPIClient
    {
        private string AppId { get; set; }
        private Func<string> RequestAuthorizationHeaderResolver { get; set; }
        public AuthenticationTokenServiceAPIClient(Func<string> requestAuthorizationHeaderResolver, string baseUrl, string appId):this(new HttpClient()) {
            RequestAuthorizationHeaderResolver = requestAuthorizationHeaderResolver;
            BaseUrl = baseUrl;
            AppId = appId;
        }
        
        private bool TryGetRequestAuthorizationHeader(out string value)
        {
            if (RequestAuthorizationHeaderResolver() is string val && !string.IsNullOrWhiteSpace(val))
            {
                value = val;
                return true;
            }
            value = null;
            return false;
        }
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            if (TryGetRequestAuthorizationHeader(out string token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public TokenDataAPIResponse SignInWithCacToken(string cacToken)
        => SignIn(new AuthenticationCredendials() { CacToken = cacToken });
        public TokenDataAPIResponse SignInWithEmail(string email)
        => SignIn(new AuthenticationCredendials() { Email = email });
        public TokenDataAPIResponse SignInWithEDIPI(string edipi)
        => SignIn(new AuthenticationCredendials() { Edipi = edipi });
        public TokenDataAPIResponse SignInWithUserIdentifier(Guid userIdentifier)
        => SignIn(new AuthenticationCredendials() { UserIdentifier = userIdentifier });

        public TokenDataAPIResponse SignIn(AuthenticationCredendials credendials)
        {
            credendials.AppId = AppId;
            var result =  AuthenticateAsync(credendials).GetAwaiter().GetResult();
            return result;
        }
        public TokenDataAPIResponse RenewSession()
        => RenewAsync().GetAwaiter().GetResult();
        public UserDataAPIResponse GetUserData()
        {
            var result = UserDataAsync().GetAwaiter().GetResult();
            return result;
        }
    }
}