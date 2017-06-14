using System.Threading.Tasks;
using Grpc.Core;
using Protogame.Editor.Grpc.ExtensionHost;
using Protoinject;
using System;

namespace Protogame.Editor.ExtHost
{
    public class ExtensionHostServerImpl : Protogame.Editor.Grpc.ExtensionHost.ExtensionHostServer.ExtensionHostServerBase
    {
        private readonly ExtensionHost _extensionHost;
        private readonly IKernel _kernel;
        private readonly IEditorClientProvider _editorClientProvider;
        private IGrpcServer _grpcServer;

        public ExtensionHostServerImpl(
            IKernel kernel,
            ExtensionHost extensionHost,
            IEditorClientProvider editorClientProvider)
        {
            _kernel = kernel;
            _extensionHost = extensionHost;
            _editorClientProvider = editorClientProvider;
        }

        public override Task<StartResponse> Start(StartRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                System.Console.Error.WriteLine("Handling Start request...");

                if (_grpcServer == null)
                {
                    System.Console.Error.WriteLine("Getting gRPC server instance for extension host...");
                    _grpcServer = _kernel.Get<IGrpcServer>();
                }

                System.Console.Error.WriteLine("Creating channel to editor gRPC server...");
                _editorClientProvider.CreateChannel(request.EditorUrl);

                System.Console.Error.WriteLine("Requesting start from assembly path...");
                string extensionUrl = null;
                try
                {
                    extensionUrl = _extensionHost.Start(_grpcServer, _editorClientProvider, request.AssemblyPath);
                }
                catch (Exception e)
                {
                    System.Console.Error.WriteLine(e);
                }

                System.Console.Error.WriteLine("Returning start response with gRPC extension server URL...");

                return new StartResponse
                {
                    ExtensionUrl = extensionUrl
                };
            });
        }
    }
}
