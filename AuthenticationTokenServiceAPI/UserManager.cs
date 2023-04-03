using AuthenticationTokenServiceAPI.Models;
using AuthenticationTokenServiceAPI.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationTokenServiceAPI
{
    //manages connected Users
    public class UserManager
    {
        private async Task<bool> ValidateAppIdAsync(AuthenticationCredendials authenticationCredendials){
            bool isValid = !string.IsNullOrWhiteSpace(authenticationCredendials.AppId);
            await Task.CompletedTask;
            return isValid;
        }
        private async Task<Guid?> TryGetUserIdentifierAsync(AuthenticationCredendials authenticationCredendials)
        {
            if(authenticationCredendials.UserIdentifier is Guid userId)
            {
                //check database to validate user
                await Task.CompletedTask;
                return userId;
            }
            await Task.CompletedTask;
            return null;
        }
        private async Task<Guid?> TryGetUserIdentifierFromCacTokenAsync(AuthenticationCredendials authenticationCredendials)
        {
            if (authenticationCredendials.CacToken is string token && !string.IsNullOrWhiteSpace(token))
            {
                //var userData = CacService.SomeCallToGetUserData  
                //get user identifier from db bsed on userData

                //if(SomeCallToDb(userData).UserIdentifier is Guid userId)
                //{
                //    await Task.CompletedTask;
                //    return userId;
                //}
                await Task.CompletedTask;
                return null;
            }
            await Task.CompletedTask;
            return null;
        }
        private async Task<Guid?> TryGetUserIdentifierFromEDIPIAsync(AuthenticationCredendials authenticationCredendials)
        {
            if (authenticationCredendials.CacToken is string token && !string.IsNullOrWhiteSpace(token))
            {
                //if(SomeCallToDb(AuthenticationCredendials.EDIPI).UserIdentifier is Guid userId)
                //{
                //    await Task.CompletedTask;
                //    return userId;
                //}
                await Task.CompletedTask;
                return null;
            }
            await Task.CompletedTask;
            return null;
        }
        private async Task<Guid?> TryGetUserIdentifierFromEmailAsync(AuthenticationCredendials authenticationCredendials)
        {
            if (authenticationCredendials.CacToken is string token && !string.IsNullOrWhiteSpace(token))
            {
                //if(SomeCallToDb(AuthenticationCredendials.Email).UserIdentifier is Guid userId)
                //{
                //    await Task.CompletedTask;
                //    return userId;
                //}
                await Task.CompletedTask;
                return null;
            }
            await Task.CompletedTask;
            return null;
        }
        public async Task<Guid?> ValidateAuthenticationCredendialsAsync(AuthenticationCredendials authenticationCredendials)
        {
            if(await ValidateAppIdAsync(authenticationCredendials))
            {
                return  await TryGetUserIdentifierAsync(authenticationCredendials) is Guid userIdVal ? userIdVal :
                    await TryGetUserIdentifierFromCacTokenAsync(authenticationCredendials) is Guid userIdValFromCac ? userIdValFromCac :
                    await TryGetUserIdentifierFromEDIPIAsync(authenticationCredendials) is Guid userIdValFromEDIPI ? userIdValFromEDIPI :
                    await TryGetUserIdentifierFromEDIPIAsync(authenticationCredendials) is Guid userIdValFromEmail ? userIdValFromEmail :
                    null;
            }
            await Task.CompletedTask;
            return null;
        }
    }
}
