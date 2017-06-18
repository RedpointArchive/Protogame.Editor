using Protogame.Editor.Api.Version1.Core;
using System;
using System.Threading.Tasks;
using static Protogame.Editor.Grpc.Editor.Presence;
using Protogame.Editor.Grpc.Editor;

namespace Protogame.Editor.ExtHost
{
    public class PresenceCheckerUpdateSignal : IWantsUpdateSignal
    {
        private readonly IEditorClientProvider _editorClientProvider;
        private PresenceClient _client;
        private Task _presenceTask;

        public PresenceCheckerUpdateSignal(IEditorClientProvider editorClientProvider)
        {
            _editorClientProvider = editorClientProvider;
        }

        public void Update()
        {
            if (_presenceTask == null || _presenceTask.IsCompleted)
            {
                _presenceTask = Task.Run(async () =>
                {
                    if (_client == null)
                    {
                        _client = _editorClientProvider.GetClient<PresenceClient>();
                    }

                    try
                    {
                        await _client.CheckAsync(new CheckPresenceRequest(), deadline: DateTime.UtcNow.AddSeconds(3));
                    }
                    catch
                    {
                        System.Console.Error.WriteLine("Unable to contact editor; automatically exiting!");
                        Environment.Exit(1);
                    }

                    await Task.Delay(10000);
                });
            }
        }
    }
}
