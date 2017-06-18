using System.Threading.Tasks;
using Grpc.Core;
using Protogame.Editor.Grpc.Editor;
using static Protogame.Editor.Grpc.Editor.Presence;

namespace Protogame.Editor.Server
{
    public class PresenceImpl : PresenceBase
    {
        public override Task<CheckPresenceResponse> Check(CheckPresenceRequest request, ServerCallContext context)
        {
            return Task.FromResult(new CheckPresenceResponse());
        }
    }
}
