using Protogame.Editor.Api.Version1;
using Protoinject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protogame.Editor.Extension
{
    public class ExtensionManagerRemoteResolve : MarshalByRefObject, IExtensionHostServerRemoteResolve
    {
        private readonly IKernel _kernel;

        public ExtensionManagerRemoteResolve(IKernel kernel)
        {
            _kernel = kernel;
        }

        public object GetInstance(Type interfaceType)
        {
            try
            {
                return _kernel.Get(interfaceType);
            }
            catch (Exception ex)
            {
                // Dump the exception somewhere.
                return null;
            }
        }
    }
}
