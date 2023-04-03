using AuthenticationTokenServiceAPI.Models;
using AuthenticationTokenServiceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AuthenticationTokenServiceAPI.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]

    public class AuthenticationController : ApiControllerBase
    {
        public AuthenticationController(IServiceProvider services) : base(services) { }
        private UserManager _UserManager => Services.GetRequiredService<UserManager>();

#if OperationId
        [HttpPost("Authenticate", Name = nameof(AuthenticateAsync))]
#else
        [HttpPost("Authenticate")]
#endif
        [SwaggerResponse((int)HttpStatusCode.OK, "Creates API Authentication JWT Token", typeof(APIResponse<TokenData>))]
        [AllowAnonymous]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> AuthenticateAsync(AuthenticationCredendials authenticationCredendials)
        {
            try
            {
                if (await _UserManager.ValidateAuthenticationCredendialsAsync(authenticationCredendials) is Guid userId)
                {
                    var jwtClaimsBuilder = new JwtClaimsBuilder();
                    jwtClaimsBuilder.Add(ClaimTypes.Name, "John E Doe");
                    jwtClaimsBuilder.Add(ClaimTypes.NameIdentifier, userId);
                    var token = JwtManager.GenerateToken(jwtClaimsBuilder, out DateTime expires);
                    return SuccessResponse(new TokenData(expires, token, new UserData("1234567890", "some.user@emaildomain.com", "John", "Doe", "E", userId, expires)));
                }
                return ErrorResponse($"Unauthorized", TokenData.Default);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex, TokenData.Default);
            }
        }
#if OperationId
        [HttpGet("Renew", Name = nameof(RenewAsync))]
#else
        [HttpGet("Renew")]
#endif
        [SwaggerResponse((int)HttpStatusCode.OK, "Renews API Authentication JWT Token", typeof(APIResponse<TokenData>))]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> RenewAsync()
        {
            try
            {
                await Task.CompletedTask;
                var token = JwtManager.RenewToken(HttpContext.User, out DateTime expires);
                if (HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier) is Claim claim && Guid.TryParse(claim.Value, out Guid userId))
                {
                    //Get real user data from DB
                    return SuccessResponse(new TokenData(expires, token, new UserData("1234567890", "some.user@emaildomain.com", "John", "Doe", "E", userId, expires)));
                }
                return ErrorResponse("No user data found", UserData.Default);
                
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex, TokenData.Default);
            }
        }
#if OperationId
        [HttpGet("UserData", Name = nameof(UserDataAsync))]
#else
        [HttpGet("UserData")]
#endif
        [SwaggerResponse((int)HttpStatusCode.OK, "Renews API Authentication JWT Token", typeof(APIResponse<UserData>))]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UserDataAsync()
        {
            try
            {
                await Task.CompletedTask;
                if(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier) is Claim claim && Guid.TryParse(claim.Value, out Guid userId))
                {
                    if(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "exp") is Claim expiresClaim && long.TryParse(expiresClaim.Value, out long seconds))
                    {
                        DateTime expires = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
                        var data = new UserData("1234567890", "some.user@emaildomain.com", "John", "Doe", "E", userId, expires);
                        //Get real user data from DB
                        return SuccessResponse(data);
                    }
                    return ErrorResponse("Missing Expiration Date", UserData.Default);
                }
                return ErrorResponse("No user data found", UserData.Default);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex, UserData.Default);
            }
        }
    }
}
