using Newtonsoft.Json.Linq;

namespace AuthenticationTokenServiceAPI.Models
{
    //Include namespace in Response class name so OpenAPI generated code has no conflictions with other OpenAPI refs
    public class APIResponse<T>
    {
        public T Value { get; private set; }
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
        public string? ErrorMessage { get; private set; }
        private APIResponse(T value)
        {
            Value = value;
        }
        internal static APIResponse<T> Create(T value)
        => new APIResponse<T>(value)
        {
            Value = value
        };
        internal static APIResponse<T> Error(string errorMessage, T value)
        => new APIResponse<T>(value)
        {
            ErrorMessage = errorMessage,
            Value = value
        };
    }
    public class TokenData
    {
        public DateTime ExpiresUtc { get; private set; }
        public string Token { get; private set; }

        public UserData UserData { get; private set; }
        public TokenData(DateTime expiresUtc, string token, UserData userData)
        {
            ExpiresUtc = expiresUtc;
            Token = token;
            UserData = userData;
        }
        public static TokenData Default = new TokenData(DateTime.UtcNow, "", UserData.Default);

    }
    public class UserData
    {
        public string EDIPI { get; private set; }
        public string Email { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string MiddleInitial { get; private set; }
        public Guid  UserIdentifier { get; private set; }
        public DateTime ExpiresUtc { get; private set; }
        public UserData(string edipi, string email, string firstName, string lastName, string middleInitial, Guid userIdentifier, DateTime expiresUtc)
        {
            EDIPI = edipi;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            MiddleInitial = middleInitial;
            UserIdentifier = userIdentifier;
            ExpiresUtc = expiresUtc;
        }
        public static UserData Default = new UserData("", "", "", "", "", default(Guid), DateTime.UtcNow);
    }

    public class AuthenticationCredendials
    {
        public string? AppId { get; set; }
        public string? EDIPI { get; set; }
        public string? Email { get; set; }
        public string? CacToken { get; set; }
        public Guid? UserIdentifier { get; set; }
    }
}
