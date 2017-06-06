using Protogame.Editor.Api.Version1;
using System;

namespace Protogame.Editor.Extension
{
    public class ExtensionServiceRegistration : MarshalByRefObject, IServiceRegistration
    {
        private readonly IDynamicServiceProvider _dynamicServiceProvider;

        public ExtensionServiceRegistration(IDynamicServiceProvider dynamicServiceProvider)
        {
            _dynamicServiceProvider = dynamicServiceProvider;
        }

        public void BindSingleton<TInterface, TImplementation>() where TImplementation : TInterface
        {
            _dynamicServiceProvider.BindSingleton<TInterface, TImplementation>();
        }

        public void BindTransient<TInterface, TImplementation>() where TImplementation : TInterface
        {
            _dynamicServiceProvider.BindTransient<TInterface, TImplementation>();
        }
    }
}
