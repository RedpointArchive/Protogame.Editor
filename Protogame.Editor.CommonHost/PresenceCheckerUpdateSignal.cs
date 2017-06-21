using Protogame.Editor.Api.Version1.Core;
using System;
using System.Threading.Tasks;
using static Protogame.Editor.Grpc.Editor.Presence;
using Protogame.Editor.Grpc.Editor;
using System.Reflection;

namespace Protogame.Editor.CommonHost
{
    public class PresenceCheckerUpdateSignal : IWantsUpdateSignal
    {
        private readonly IEditorClientProvider _editorClientProvider;
        private readonly Api.Version1.Core.IConsoleHandle _consoleHandle;
        private PresenceClient _client;
        private Task _presenceTask;

        public PresenceCheckerUpdateSignal(
            IEditorClientProvider editorClientProvider,
            Api.Version1.Core.IConsoleHandle consoleHandle)
        {
            _editorClientProvider = editorClientProvider;
            _consoleHandle = consoleHandle;
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

                    _consoleHandle.LogInfo("Checking for editor presence from {0}...", Assembly.GetEntryAssembly().GetName().Name);

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
