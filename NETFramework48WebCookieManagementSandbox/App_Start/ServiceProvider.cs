using NETFramework48WebCookieManagementSandbox.AuthNAuthZ.Authentication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NETFramework48WebCookieManagementSandbox.App_Start
{
    public interface IRuntimeService
    {
        void init(IRuntimeServiceProvider services);
    }
    public interface IRuntimeServiceBuilder
    {
        void AddSinglton<TService>()
                where TService : class;
        void AddSinglton<TService>(TService instance)
            where TService : class;
        void AddSinglton<TService, TInstance>(TInstance instance)
            where TService : class
            where TInstance : TService;
        void AddTransient<TService>()
            where TService : class;
        void AddTransient<TService>(Func<TService> instanceResolver)
            where TService : class;
        void AddTransient<TService, TInstance>(Func<TInstance> instanceResolver)
            where TService : class
            where TInstance : TService;
    }
    public interface IRuntimeServiceProvider
    {
        TService GetService<TService>();
        TService GetRequiredService<TService>();
    }
    
    public class RuntimeService
    {
        private static RuntimeServiceContainer Container = new RuntimeServiceContainer();
        public static IRuntimeServiceBuilder Builder => Container;
        public static IRuntimeServiceProvider Services => Container;
        public static void UpdateTransientServices() => Container.UpdateTransientServices();
        private class RuntimeServiceContainer : IRuntimeServiceProvider, IRuntimeServiceBuilder
        {
            private ConcurrentDictionary<Type, object> _services { get; set; }
            private ConcurrentDictionary<Type, Func<object>> _transientServiceResolvers { get; set; }
            private List<Type> _serviceTypes { get; set; }
            private List<Type> _transientServiceTypes { get; set; }
            public RuntimeServiceContainer()
            {
                _services = new ConcurrentDictionary<Type, object>();
                _serviceTypes = new List<Type>();
                _transientServiceTypes = new List<Type>();
                _transientServiceResolvers = new ConcurrentDictionary<Type, Func<object>>();
            }

            private void CheckInitService(Type serviceType)
            {
                if (!_services.ContainsKey(serviceType))
                {
                    object service = Activator.CreateInstance(serviceType);
                    if (service is IRuntimeService runtimeService)
                    {
                        runtimeService.init(this);
                    }
                    _services.TryAdd(serviceType, service);
                }
            }
            private void ConfigureServiceCore<TService>()
                where TService : class
            {
                if (!_serviceTypes.Contains(typeof(TService)))
                {
                    _serviceTypes.Add(typeof(TService));
                }

            }
            private void ConfigureTransientServiceCore<TService>()
                where TService : class
            {
                ConfigureServiceCore<TService>();
                if (!_transientServiceTypes.Contains(typeof(TService)))
                {
                    _transientServiceTypes.Add(typeof(TService));
                }
            }
            private void ConfigureServiceInstanceCore<TService, TInstance>(TInstance instance)
                where TService : class
                where TInstance : TService
            {
                if (!_services.ContainsKey(typeof(TService)))
                {
                    if (instance is IRuntimeService runtimeService)
                    {
                        runtimeService.init(this);
                    }
                    _services.TryAdd(typeof(TService), instance);
                }
            }
            private void UpdateTransientService(Type serviceType)
            {
                object instance =
                        _transientServiceResolvers.TryGetValue(serviceType, out Func<object> instanceResolver) ? instanceResolver() :
                        Activator.CreateInstance(serviceType);

                if (instance is IRuntimeService runtimeService)
                {
                    runtimeService.init(this);
                }
                _services.AddOrUpdate(serviceType, instance, (key, oldValue) => instance);
            }
            public void UpdateTransientServices()
            {
                foreach (Type serviceType in _transientServiceTypes)
                {
                    UpdateTransientService(serviceType);
                }
            }

            public void AddSinglton<TService>()
                where TService : class
            => ConfigureServiceCore<TService>();
            public void AddSinglton<TService>(TService instance)
                where TService : class
            => AddSinglton<TService, TService>(instance);
            
            public void AddSinglton<TService, TInstance>(TInstance instance)
                where TService : class
                where TInstance : TService
            {
                ConfigureServiceCore<TService>();
                ConfigureServiceInstanceCore<TService, TInstance>(instance);
            }
            public void AddTransient<TService>()
                where TService : class
            => ConfigureTransientServiceCore<TService>();
            public void AddTransient<TService>(Func<TService> instanceResolver)
                where TService : class
                => AddTransient<TService, TService>(instanceResolver);
            public void AddTransient<TService, TInstance>(Func<TInstance> instanceResolver)
                where TService : class
                where TInstance : TService
            {
                ConfigureTransientServiceCore<TService>();
                if (!_transientServiceResolvers.ContainsKey(typeof(TService)))
                {
                    _transientServiceResolvers.TryAdd(typeof(TService), () => {
                        TInstance instance = instanceResolver();
                        if (instance is IRuntimeService runtimeService)
                        {
                            runtimeService.init(this);
                        }
                        return instance;
                    });
                }
            }

            public TService GetService<TService>()
            {
                Type requestedType = typeof(TService);
                Type serviceType = requestedType.IsInterface ? _serviceTypes.FirstOrDefault(t => t == requestedType || requestedType.IsAssignableFrom(t)) : requestedType;
                
                if (_transientServiceTypes.Contains(serviceType) && !_services.ContainsKey(serviceType))
                {
                    UpdateTransientService(serviceType);
                }
                else
                {
                    CheckInitService(serviceType);
                }
                return _services.TryGetValue(serviceType, out object val) && val is TService service ? service: default;
            }
            public TService GetRequiredService<TService>()
            {
                TService service = GetService<TService>();
                if (service == null)
                {
                    throw new NullReferenceException($"Missing required service {typeof(TService)}.");
                }
                return service;
            }

        }
    }
}