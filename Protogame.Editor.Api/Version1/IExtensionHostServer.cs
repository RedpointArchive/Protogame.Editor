using Protoinject;
using System.Threading.Tasks;

namespace Protogame.Editor.Api.Version1
{
    public interface IExtensionHostServer
    {
        RegisteredService[] Start(string assembly);

        void RegisterRemoteResolve(IExtensionHostServerRemoteResolve remoteResolve);
    }
}
