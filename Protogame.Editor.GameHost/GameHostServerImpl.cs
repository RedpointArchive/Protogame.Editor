using System.Threading.Tasks;
using Grpc.Core;
using Protogame.Editor.Grpc.GameHost;
using static Protogame.Editor.Grpc.GameHost.GameHostServer;

namespace Protogame.Editor.GameHost
{
    public class GameHostServerImpl : GameHostServerBase
    {
        private readonly IGameRunner _gameRunner;
        private readonly HostedEventEngineHook _hostedEventEngineHook;

        public GameHostServerImpl(
            IGameRunner gameRunner,
            HostedEventEngineHook hostedEventEngineHook)
        {
            _gameRunner = gameRunner;
            _hostedEventEngineHook = hostedEventEngineHook;
        }

        public override Task<SetMousePositionResponse> SetMousePosition(SetMousePositionRequest request, ServerCallContext context)
        {
            _gameRunner.SetMousePositionToGet(request.X, request.Y);
            return Task.FromResult(new SetMousePositionResponse());
        }

        public override Task<GetMousePositionResponse> GetMousePosition(GetMousePositionRequest request, ServerCallContext context)
        {
            int x = 0, y = 0;
            _gameRunner.GetMousePositionToSet(ref x, ref y);
            return Task.FromResult(new GetMousePositionResponse
            {
                X = x,
                Y = y
            });
        }
    }
}
