using Protogame.Editor.Api.Version1;
using Protoinject;
using System;
using System.Threading.Tasks;

namespace Protogame.Editor.ExtHost
{
    public class ExtensionHostServer : MarshalByRefObject, IExtensionHostServer
    {
        public bool Running => true;

        public override object InitializeLifetimeService() { return (null); }

        public async Task AcceptMarshalledKernel(IKernel kernel)
        {
        }

        public async Task LoadAssembly(string assembly)
        {
        }

        public void Update()
        {
        }
    }
}
