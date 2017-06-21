using System.Threading.Tasks;
using Grpc.Core;
using Protogame.Editor.Grpc.Editor;
using static Protogame.Editor.Grpc.Editor.GameHoster;
using Protogame.Editor.LoadedGame;
using System;
using Protoinject;

namespace Protogame.Editor.Server
{
    public class GameHosterImpl : GameHosterBase
    {
        private readonly IKernel _kernel;
        private readonly Lazy<ILoadedGame> _loadedGame;

        public GameHosterImpl(IKernel kernel)
        {
            _kernel = kernel;
            _loadedGame = new Lazy<ILoadedGame>(() => _kernel.Get<ILoadedGame>());
        }

        public override Task<GetBaseDirectoryResponse> GetBaseDirectory(GetBaseDirectoryRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetBaseDirectoryResponse { BaseDirectory = _loadedGame.Value.GetBaseDirectory() });
        }

        public override Task<GetBackBufferDimensionsResponse> GetBackBufferDimensions(GetBackBufferDimensionsRequest request, ServerCallContext context)
        {
            var size = _loadedGame.Value.GetRenderTargetSize();
            if (size != null)
            {
                return Task.FromResult(new GetBackBufferDimensionsResponse { Width = size.Value.X, Height = size.Value.Y });
            }

            return Task.FromResult(new GetBackBufferDimensionsResponse { Width = 640, Height = 480 });
        }

        public override Task<PlaybackStateChangedResponse> PlaybackStateChanged(PlaybackStateChangedRequest request, ServerCallContext context)
        {
            _loadedGame.Value.SetPlaybackStateInternal(request);
            return Task.FromResult(new PlaybackStateChangedResponse());
        }
    }
}
