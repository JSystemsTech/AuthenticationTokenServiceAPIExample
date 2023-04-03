using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationTokenServiceAPI.Services
{
    internal static class JwtSecretManager
    {
        private static HMACSHA256 _Hmac = new HMACSHA256();
        public static SymmetricSecurityKey CreateSymmetricSecurityKey() => new SymmetricSecurityKey(_Hmac.Key);
        public static SigningCredentials CreateSigningCredentials() => new SigningCredentials(CreateSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256Signature);

    }
    public class JwtClaimsBuilder
    {
        private IDictionary<string, object> _Claims { get; set; }

        public JwtClaimsBuilder()
        {
            _Claims = new Dictionary<string, object>();
        }
        public void Add(string key, object value)
        {
            if (!_Claims.ContainsKey(key))
            {
                _Claims[key] = value;
            }
        }
        internal Claim[] GetClaims() {

            List<Claim> claims = new List<Claim>();
            foreach(var claim in _Claims)
            {
                if (claim.Value.ToString() is string val)
                {
                    claims.Add(new Claim(claim.Key, val));
                }
            }
            return claims.ToArray();
        }
    }
    internal class JwtTokenConfig
    {
        public string? Issuer { get; set; }
        public string? Audience { get; set; }

        public int ValidFor { get; set; } = 20;
        public bool ValidateIssuer => !string.IsNullOrWhiteSpace(Issuer);
        public bool ValidAudience => !string.IsNullOrWhiteSpace(Audience);
        public static JwtTokenConfig Default => new JwtTokenConfig() {
            Issuer = ApplicationConfiguration.JwtSettings.Issuer,
            Audience = ApplicationConfiguration.JwtSettings.Audience,
            ValidFor = ApplicationConfiguration.JwtSettings.ValidForCalculated
        };
    }
internal static class JwtManager
    {
        public static string GenerateToken(JwtClaimsBuilder jwtClaimsBuilder, out DateTime expires)
            => GenerateToken(jwtClaimsBuilder, JwtTokenConfig.Default, out expires);
        public static string GenerateToken(JwtClaimsBuilder jwtClaimsBuilder, JwtTokenConfig config, out DateTime expires)
            => jwtClaimsBuilder.GetClaims().ToTokenString(config, out expires);

        public static string RenewToken(ClaimsPrincipal principal, out DateTime expires)
            => RenewToken(principal, JwtTokenConfig.Default, out expires);
        public static string RenewToken(ClaimsPrincipal principal, JwtTokenConfig config, out DateTime expires)
        => principal.Claims.ToTokenString(config, out expires);
        private static string ToTokenString(this IEnumerable<Claim> claims, JwtTokenConfig config, out DateTime expires)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            expires = DateTime.UtcNow.AddMinutes(config.ValidFor);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = config.Issuer,
                Audience = config.Audience,
                SigningCredentials = JwtSecretManager.CreateSigningCredentials()
            };

            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }
        public static TokenValidationParameters ToTokenValidationParameters(this JwtTokenConfig config)
        => new TokenValidationParameters()
        {
            RequireExpirationTime = true,
            ValidateIssuer = config.ValidateIssuer,
            ValidIssuer = config.Issuer,
            ValidateAudience = config.ValidAudience,
            ValidAudience = config.Audience,
            IssuerSigningKey = JwtSecretManager.CreateSymmetricSecurityKey()
        };
        public static bool TryGetTokenData(string token, out (ClaimsPrincipal? principal, SecurityToken? securityToken) tokenData)
            => TryGetTokenData(token, JwtTokenConfig.Default, out tokenData);
        public static bool TryGetTokenData(string token, JwtTokenConfig config, out (ClaimsPrincipal? principal, SecurityToken? securityToken) tokenData)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                {
                    tokenData = (null, null) ;
                    return false;
                }

                var principal = tokenHandler.ValidateToken(token, config.ToTokenValidationParameters(), out SecurityToken validatedToken);
                tokenData = (principal, validatedToken);
                return true;
            }

            catch (Exception ex)
            {
                tokenData = (null, null);
                return false;
            }
        }
    }
}
