using Microsoft.Xna.Framework.Graphics;
using Protogame.Editor.CommonHost;
using Protogame.Editor.Grpc.Editor;
using System.Threading.Tasks;
using static Protogame.Editor.Grpc.Editor.GameHoster;

namespace Protogame.Editor.GameHost
{
    public class HostedBackBufferDimensions : IBackBufferDimensions
    {
        private readonly GameHosterClient _gameHosterClient;
        private BackBufferSize? _backBufferSize;
        private Task _backBufferSizeUpdater;

        public HostedBackBufferDimensions(IEditorClientProvider editorClientProvider)
        {
            _gameHosterClient = editorClientProvider.GetClient<GameHosterClient>();
        }

        public BackBufferSize GetSize(GraphicsDevice graphicsDevice)
        {
            if (_backBufferSize == null)
            {
                // Make synchronous request to get information.
                var response = _gameHosterClient.GetBackBufferDimensions(new GetBackBufferDimensionsRequest());
                _backBufferSize = new BackBufferSize(response.Width, response.Height);
            }

            if (_backBufferSizeUpdater == null || _backBufferSizeUpdater.IsCompleted)
            {
                _backBufferSizeUpdater = Task.Run(async () =>
                {
                    var response = await _gameHosterClient.GetBackBufferDimensionsAsync(new GetBackBufferDimensionsRequest());
                    _backBufferSize = new BackBufferSize(response.Width, response.Height);

                    await Task.Delay(1000);
                });
            }

            return _backBufferSize.Value;
        }
    }
}
