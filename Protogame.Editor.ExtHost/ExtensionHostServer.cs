using Protogame.Editor.Api.Version1;
using Protoinject;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading.Tasks;

namespace Protogame.Editor.ExtHost
{
    public class ExtensionHostServer : MarshalByRefObject, IExtensionHostServer
    {
        private IEditorExtension[] _editorExtensions;

        public bool Running => true;

        public override object InitializeLifetimeService() { return (null); }
        
        public async Task<RegisteredService[]> Start(string assemblyFile)
        {
            Console.WriteLine("Loading assembly from: {0}", assemblyFile);
            var assembly = Assembly.LoadFrom(assemblyFile);

            // Find the attributes that describe the extensions provided by this assembly.
            var editorExtensions = new List<IEditorExtension>();
            foreach (var attr in assembly.GetCustomAttributes<ExtensionAttribute>())
            {
                editorExtensions.Add((IEditorExtension)Activator.CreateInstance(attr.Type));
            }
            _editorExtensions = editorExtensions.ToArray();

            // Perform service registration.
            var localKernel = new StandardKernel();
            var registrations = new List<RegisteredService>();
            var svcRegistration = new LocalServiceRegistration(localKernel, registrations.Add);
            foreach (var ext in _editorExtensions)
            {
                ext.RegisterServices(svcRegistration);
            }
            return registrations.ToArray();
        }

        public void Update()
        {
            // TODO: Update loop for IWantsUpdateSignal
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
                    RemotingConfiguration.RegisterWellKnownServiceType(typeof(TImplementation), "Singleton" + typeof(TImplementation).Name, WellKnownObjectMode.Singleton);
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

            public void BindTransient<TInterface, TImplementation>() where TImplementation : TInterface
            {
                if (typeof(TInterface).Assembly.GetName().Name == "Protogame.Editor.Api")
                {
                    RemotingConfiguration.RegisterWellKnownServiceType(typeof(TImplementation), "SingleCall" + typeof(TImplementation).Name, WellKnownObjectMode.SingleCall);
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
        }
    }
}
