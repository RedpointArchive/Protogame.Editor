using Protogame.Editor.Api.Version1;
using Protoinject;
using System;

namespace Protogame.Editor.ExtHost
{
    public class ExtensionRemoteFactory<T> : MarshalByRefObject, IRemoteFactory
    {
        private readonly IKernel _kernel;

        public ExtensionRemoteFactory(IKernel kernel)
        {
            _kernel = kernel;
        }

        public object GetInstance()
        {
            return _kernel.Get<T>();
        }
    }
}
