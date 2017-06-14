using Grpc.Core;
using Protoinject;

namespace Protogame.Editor.ExtHost
{
    public interface IGrpcServer
    {
        string GetServerUrl();

        string StartAndGetRuntimeServerUrl(IKernel localKernel);
    }
}
