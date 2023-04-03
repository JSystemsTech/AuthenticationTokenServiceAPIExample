using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Generator;
using System.Web.Services.Description;

namespace AuthNAuthZ.Authentication
{
    public interface IApplicationAuthenticationManager {
        void AddScheme<T>(T authenticatorMethod)
          where T : IAuthenticationProvider;
        void Authenticate();
    }
    internal class ApplicationAuthenticationManager: IApplicationAuthenticationManager
    {
        private readonly List<IAuthenticationProvider> _AuthenticationMethods;

        public ApplicationAuthenticationManager()
        {
            _AuthenticationMethods = new List<IAuthenticationProvider>();
        }

        public void AddScheme<T>(T authenticatorMethod)
          where T : IAuthenticationProvider
        {
            if(!_AuthenticationMethods.Any(m => m.AuthenticationScheme == authenticatorMethod.AuthenticationScheme))
            {
                _AuthenticationMethods.Add(authenticatorMethod);
            }
            else
            {
                throw new ArgumentException($"Authentication Provider {authenticatorMethod.AuthenticationScheme} already exists.");
            }
        }
        public void Authenticate()
        {
            foreach(IAuthenticationProvider authenticationMethod in _AuthenticationMethods)
            {
                if (authenticationMethod.Authenticate(HttpContext.Current))
                {
                    break;
                }
            }
        }
    }

    internal static class ApplicationAuthenticationManagerExtnesions
    {
        public static TRoleProvider GetRoleProvider<TRoleProvider>()
            where TRoleProvider : IRoleProvider
        {
            IRoleProvider roleProvider = RuntimeService.Services.GetRequiredService<IRoleProvider>();
            if (roleProvider is TRoleProvider roleProviderInstance)
            {
                return roleProviderInstance;
            }
            else
            {
                throw new NullReferenceException($"Missing required role provider {typeof(TRoleProvider)}.");
            }
        }
        public static void ConfigureRoleProvider<TRoleProvider>(this IRuntimeServiceBuilder builder)
            where TRoleProvider : class, IRoleProvider
             
        {
            builder.AddSinglton<TRoleProvider>();            
        }
        public static IApplicationAuthenticationManager AddAuthentication(this IRuntimeServiceBuilder builder)

        {
            ApplicationAuthenticationManager instance = new ApplicationAuthenticationManager();
            builder.AddSinglton<IApplicationAuthenticationManager, ApplicationAuthenticationManager>(instance);
            return instance;
        }
    }

}