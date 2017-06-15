using Grpc.Core;
using Protoinject;
using System.Threading.Tasks;

namespace Protogame.Editor.ExtHost
{
    public interface IGrpcServer
    {
        string GetServerUrl();

        Task<string> StartAndGetRuntimeServerUrlAsync(IKernel localKernel);
    }
}
