using Protoinject;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Grpc.Core;
using Protogame.Editor.Server;

namespace Protogame.Editor.Extension
{
    public class ExtensionManager : IExtensionManager
    {
        private bool _hasLoadedBundledExtensions;
        private Dictionary<string, ManagedExtension> _extensions;
        private readonly IConsoleHandle _consoleHandle;
        private readonly IKernel _kernel;
        private readonly IGrpcServer _grpcServer;

        public ExtensionManager(
            IKernel kernel,
            IConsoleHandle consoleHandle,
            IGrpcServer grpcServer)
        {
            _kernel = kernel;
            _consoleHandle = consoleHandle;
            _extensions = new Dictionary<string, ManagedExtension>();
            _grpcServer = grpcServer;
        }

        public void Update()
        {
            if (!_hasLoadedBundledExtensions)
            {
                // We need to load extensions bundled with the editor.
                var editorDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
                var extensionFiles = editorDirectory.GetFiles("Protogame.Editor.Ext.*.dll");

                foreach (var file in extensionFiles)
                {
                    _consoleHandle.LogDebug("Added bundled extension {0}", file.FullName);
                    _extensions.Add(file.FullName, new ManagedExtension
                    {
                        File = file,
                        Owner = "bundled",
                        ExtensionProcess = null,
                        ExtensionHostChannel = null,
                        ExtensionHostServerClient = null
                    });
                }

                _hasLoadedBundledExtensions = true;
            }

            foreach (var extension in _extensions)
            {
                var ext = extension;
                if (ext.Value.ExtensionProcess == null ||
                    ext.Value.ExtensionProcess.HasExited)
                    // TODO: Also check if extension file has changed...
                {
                    var extHostPath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "Protogame.Editor.ExtHost.exe");
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = extHostPath,
                        Arguments = "--track " + Process.GetCurrentProcess().Id,
                        WorkingDirectory = ext.Value.File.DirectoryName,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    ext.Value.ExtensionProcess = Process.Start(processStartInfo);
                    ext.Value.ExtensionProcess.Exited += (sender, e) =>
                    {
                        _consoleHandle.LogWarning("Extension host process has unexpectedly quit: {0}", ext.Value.File.FullName);
                        ext.Value.ExtensionProcess = null;
                    };
                    ext.Value.ExtensionProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            return;
                        }
                        if (ext.Value.ExtensionHostServerClient != null)
                        {
                            _consoleHandle.LogDebug(e.Data);
                            return;
                        }

                        var editorGrpcServer = _grpcServer.GetServerUrl();
                        _consoleHandle.LogDebug("Editor gRPC server is {0}", editorGrpcServer);

                        var url = e.Data?.Trim();
                        _consoleHandle.LogDebug("Creating gRPC channel on {0}...", url);
                        ext.Value.ExtensionHostChannel = new Channel(url, ChannelCredentials.Insecure);

                        _consoleHandle.LogDebug("Creating extension host client on gRPC channel...");
                        ext.Value.ExtensionHostServerClient = new Grpc.ExtensionHost.ExtensionHostServer.ExtensionHostServerClient(ext.Value.ExtensionHostChannel);
                        _consoleHandle.LogDebug("Created extension host client on gRPC channel");

                        _consoleHandle.LogDebug("Requesting extension load of {0}...", ext.Value.File.FullName);
                        var registeredServices = ext.Value.ExtensionHostServerClient.Start(new Grpc.ExtensionHost.StartRequest
                        {
                            AssemblyPath = ext.Value.File.FullName,
                            EditorUrl = editorGrpcServer
                        });
                        _consoleHandle.LogDebug("Extension loaded: {0}", ext.Value.File.FullName);
                    };
                    ext.Value.ExtensionProcess.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            _consoleHandle.LogError(e.Data);
                        }
                    };
                    ext.Value.ExtensionProcess.EnableRaisingEvents = true;
                    ext.Value.ExtensionProcess.BeginErrorReadLine();
                    ext.Value.ExtensionProcess.BeginOutputReadLine();
                }
            }
        }

        private class ManagedExtension
        {
            public FileInfo File { get; set; }

            public string Owner { get; set; }

            public Process ExtensionProcess { get; set; }

            public Channel ExtensionHostChannel { get; set; }

            public Protogame.Editor.Grpc.ExtensionHost.ExtensionHostServer.ExtensionHostServerClient ExtensionHostServerClient { get; set; }
        }
    }
}
