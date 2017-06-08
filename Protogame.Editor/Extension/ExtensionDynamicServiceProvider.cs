using Protogame.Editor.Api.Version1;
using Protoinject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Protogame.Editor.Extension
{
    public class ExtensionDynamicServiceProvider : IDynamicServiceProvider, IServiceRegistration
    {
        private readonly IKernel _kernel;
        private Dictionary<Type, object> _objectCache;

        public ExtensionDynamicServiceProvider(IKernel kernel)
        {
            _kernel = kernel;
            _objectCache = new Dictionary<Type, object>();
        }

        public void BindSingleton<TInterface, TImplementation>() where TImplementation : TInterface
        {
            _kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope();

            if (_objectCache.ContainsKey(typeof(TInterface)))
            {
                foreach (var o in ((TInterface[])_objectCache[typeof(TInterface)]).OfType<IDisposable>())
                {
                    o.Dispose();
                }

                _objectCache.Remove(typeof(TInterface));
            }
        }

        public void BindSingleton(Type @interface, Func<object> factory)
        {
            _kernel.Bind(@interface).ToMethod(x => factory()).InSingletonScope();

            if (_objectCache.ContainsKey(@interface))
            {
                foreach (var o in ((Array)_objectCache[@interface]).OfType<IDisposable>())
                {
                    o.Dispose();
                }

                _objectCache.Remove(@interface);
            }
        }

        public void BindTransient<TInterface, TImplementation>() where TImplementation : TInterface
        {
            _kernel.Bind<TInterface>().To<TImplementation>();

            if (_objectCache.ContainsKey(typeof(TInterface)))
            {
                foreach (var o in ((TInterface[])_objectCache[typeof(TInterface)]).OfType<IDisposable>())
                {
                    o.Dispose();
                }

                _objectCache.Remove(typeof(TInterface));
            }
        }

        public void BindTransient(Type @interface, Func<object> factory)
        {
            _kernel.Bind(@interface).ToMethod(x => factory());

            if (_objectCache.ContainsKey(@interface))
            {
                foreach (var o in ((Array)_objectCache[@interface]).OfType<IDisposable>())
                {
                    o.Dispose();
                }

                _objectCache.Remove(@interface);
            }
        }

        public T[] GetAll<T>()
        {
            if (_objectCache.ContainsKey(typeof(T)))
            {
                return (T[])_objectCache[typeof(T)];
            }

            _objectCache[typeof(T)] = _kernel.GetAll<T>();
            return (T[])_objectCache[typeof(T)];
        }
    }
}
