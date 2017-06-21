using System.Threading.Tasks;
using Grpc.Core;
using Protogame.Editor.Grpc.GameHost;
using static Protogame.Editor.Grpc.GameHost.GameHostServer;
using System;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Protogame.Editor.GameHost
{
    public class GameHostServerImpl : GameHostServerBase
    {
        private readonly IGameRunner _gameRunner;
        private readonly HostedEventEngineHook _hostedEventEngineHook;
        private readonly BinaryFormatter _formatter;

        public GameHostServerImpl(
            IGameRunner gameRunner,
            HostedEventEngineHook hostedEventEngineHook)
        {
            _gameRunner = gameRunner;
            _hostedEventEngineHook = hostedEventEngineHook;
            _formatter = new BinaryFormatter();
        }

        public override Task<SetRenderTargetsResponse> SetRenderTargets(SetRenderTargetsRequest request, ServerCallContext context)
        {
            _gameRunner.SetHandles(request.SharedPointer.Select(x => new IntPtr(x)).ToArray(), request.SyncMmappedFileName);
            return Task.FromResult(new SetRenderTargetsResponse());
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

        public override Task<SetPlaybackModeResponse> SetPlaybackMode(SetPlaybackModeRequest request, ServerCallContext context)
        {
            _gameRunner.SetPlaybackMode(request.Playing);
            return Task.FromResult(new SetPlaybackModeResponse());
        }

        public override Task<QueueSerializedEventResponse> QueueSerializedEvent(QueueSerializedEventRequest request, ServerCallContext context)
        {
            using (var memory = new MemoryStream(request.SerializedEvent.ToByteArray()))
            {
                var @event = _formatter.Deserialize(memory) as Event;
                _hostedEventEngineHook.QueueEvent(@event);
            }   
            return Task.FromResult(new QueueSerializedEventResponse());
        }
    }
}
