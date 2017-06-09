using Protogame.Editor.Api.Version1;
using Protogame.Editor.Api.Version1.Core;
using Protoinject;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading.Tasks;

namespace Protogame.Editor.ExtHost
{
    public class ExtensionHostServer : MarshalByRefObject, IExtensionHostServer, IDynamicResolutionFallback
    {
        private IEditorExtension[] _editorExtensions;
        private IExtensionHostServerRemoteResolve _remoteResolve;
        private IWantsUpdateSignal[] _wantsUpdateSignal;

        public bool Running => true;

        public object GetInstance(Type interfaceType)
        {
            return _remoteResolve.GetInstance(interfaceType);
        }

        public override object InitializeLifetimeService() { return (null); }

        public void RegisterRemoteResolve(IExtensionHostServerRemoteResolve remoteResolve)
        {
            _remoteResolve = remoteResolve;
        }

        public RegisteredService[] Start(string assemblyFile)
        {
            try
            {
                Console.WriteLine("Loading assembly from: {0}", assemblyFile);
                var assembly = Assembly.LoadFrom(assemblyFile);

                // Find the attributes that describe the extensions provided by this assembly.
                var editorExtensions = new List<IEditorExtension>();
                foreach (var attr in assembly.GetCustomAttributes<ExtensionAttribute>())
                {
                    try
                    {
                        editorExtensions.Add((IEditorExtension)Activator.CreateInstance(attr.Type));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                _editorExtensions = editorExtensions.ToArray();

                // Perform service registration.
                var localKernel = new StandardKernel();
                localKernel.DynamicResolutionFallback = this;
                var registrations = new List<RegisteredService>();
                var svcRegistration = new LocalServiceRegistration(localKernel, registrations.Add);
                foreach (var ext in _editorExtensions)
                {
                    ext.RegisterServices(svcRegistration);
                }

                try
                {
                    _wantsUpdateSignal = localKernel.GetAll<IWantsUpdateSignal>();
                }
                catch (Exception ex)
                {
                    // Maybe no update signals are registered - so ignore.
                }

                return registrations.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new RegisteredService[0];
            }
        }

        public void Update()
        {
            if (_wantsUpdateSignal != null)
            {
                foreach (var u in _wantsUpdateSignal)
                {
                    u.Update();
                }
            }
        }

        private class LocalServiceRegistration : IServiceRegistration
        {
            private readonly IKernel _kernel;
            private readonly Action<RegisteredService> _appendRegisteredType;

            public LocalServiceRegistration(IKernel kernel, Action<RegisteredService> appendRegisteredType)
            {
                _kernel = kernel;
                _appendRegisteredType = appendRegisteredType;
            }

            public void BindSingleton<TInterface, TImplementation>() where TImplementation : TInterface
            {
                if (typeof(TInterface).Assembly.GetName().Name == "Protogame.Editor.Api")
                {
                    var instance = new ExtensionRemoteFactory<TInterface>(_kernel);
                    _kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope();
                    RemotingServices.Marshal(instance, "Singleton" + typeof(TImplementation).Name, typeof(IRemoteFactory));
                    _appendRegisteredType(new RegisteredService
                    {
                        Interface = typeof(TInterface),
                        ImplementationUri = "Singleton" + typeof(TImplementation).Name,
                        IsSingleton = true
                    });
                }
                else
                {
                    _kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope();
                }
            }

            public void BindSingleton(Type @interface, Func<object> factory)
            {
                throw new NotSupportedException();
            }

            public void BindTransient<TInterface, TImplementation>() where TImplementation : TInterface
            {
                if (typeof(TInterface).Assembly.GetName().Name == "Protogame.Editor.Api")
                {
                    var instance = new ExtensionRemoteFactory<TInterface>(_kernel);
                    _kernel.Bind<TInterface>().To<TImplementation>();
                    RemotingServices.Marshal(instance, "SingleCall" + typeof(TImplementation).Name, typeof(IRemoteFactory));
                    _appendRegisteredType(new RegisteredService
                    {
                        Interface = typeof(TInterface),
                        ImplementationUri = "SingleCall" + typeof(TImplementation).Name,
                        IsSingleton = false
                    });
                }
                else
                {
                    _kernel.Bind<TInterface>().To<TImplementation>();
                }
            }

            public void BindTransient(Type @interface, Func<object> factory)
            {
                throw new NotSupportedException();
            }
        }
    }
}
