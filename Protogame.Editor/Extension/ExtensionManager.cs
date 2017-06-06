using System;
using Protogame.Editor.Api.Version1;
using Protoinject;
using Protogame.Editor.Api.Version1.Core;

namespace Protogame.Editor.Extension
{
    public class ExtensionManager : IExtensionManager
    {
        private readonly IServiceRegistration _serviceRegistration;
        private readonly IEditorExtension[] _editorExtensions;
        private readonly IKernel _kernel;
        private readonly IDynamicServiceProvider _dynamicServiceProvider;

        public ExtensionManager(
            IServiceRegistration serviceRegistration,
            IEditorExtension[] editorExtensions,
            IDynamicServiceProvider dynamicServiceProvider,
            IKernel kernel)
        {
            _serviceRegistration = serviceRegistration;
            _editorExtensions = editorExtensions;
            _dynamicServiceProvider = dynamicServiceProvider;
            _kernel = kernel;

            foreach (var ext in _editorExtensions)
            {
                ext.RegisterServices(_serviceRegistration);
            }
        }

        public void Update()
        {
            foreach (var us in _dynamicServiceProvider.GetAll<IWantsUpdateSignal>())
            {
                us.Update();
            }
        }
    }
}
