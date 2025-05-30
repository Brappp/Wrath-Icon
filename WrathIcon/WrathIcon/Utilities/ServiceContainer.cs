using System;
using System.Collections.Generic;

namespace WrathIcon.Utilities
{
    public class ServiceContainer : IDisposable
    {
        private readonly Dictionary<Type, object> services = new();
        private readonly Dictionary<Type, Func<object>> factories = new();

        public void Register<TInterface, TImplementation>(TImplementation instance)
            where TImplementation : class, TInterface
        {
            services[typeof(TInterface)] = instance;
        }

        public void Register<T>(T instance) where T : class
        {
            services[typeof(T)] = instance;
        }

        public void Register<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            factories[typeof(TInterface)] = () => new TImplementation();
        }

        public void Register<TInterface>(Func<TInterface> factory)
        {
            factories[typeof(TInterface)] = () => factory()!;
        }

        public T Get<T>()
        {
            var type = typeof(T);
            
            if (services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            
            if (factories.TryGetValue(type, out var factory))
            {
                var instance = factory();
                services[type] = instance;
                return (T)instance;
            }
            
            throw new InvalidOperationException($"Service of type {type.Name} is not registered.");
        }

        public bool TryGet<T>(out T? service)
        {
            try
            {
                service = Get<T>();
                return true;
            }
            catch
            {
                service = default;
                return false;
            }
        }

        public void Dispose()
        {
            foreach (var service in services.Values)
            {
                if (service is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            services.Clear();
            factories.Clear();
        }
    }
} 