using Protoinject;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Grpc.Core;
using Protogame.Editor.Server;
using System.Linq;
using System;

namespace Protogame.Editor.Extension
{
    public class ExtensionManager : IExtensionManager
    {
        private bool _hasLoadedBundledExtensions;
        private Dictionary<string, ManagedExtension> _extensions;
        private readonly IConsoleHandle _consoleHandle;
        private readonly IKernel _kernel;
        private readonly IGrpcServer _grpcServer;
        private Extension[] _publicExtensions;
        private bool _recomputeExtensions;

        public ExtensionManager(
            IKernel kernel,
            IConsoleHandle consoleHandle,
            IGrpcServer grpcServer)
        {
            _kernel = kernel;
            _consoleHandle = consoleHandle;
            _extensions = new Dictionary<string, ManagedExtension>();
            _grpcServer = grpcServer;
            _publicExtensions = new Extension[0];
            _recomputeExtensions = false;
        }

        public Extension[] Extensions => _publicExtensions;

        public void DebugExtension(Extension extension)
        {
            if (!_extensions.ContainsKey(extension.Path))
            {
                return;
            }

            var managedExtension = _extensions[extension.Path];
            managedExtension.ShouldDebug = true;
            managedExtension.ShouldRestart = true;
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
                        ExtensionChannel = null,
                    });
                    _recomputeExtensions = true;
                }

                _hasLoadedBundledExtensions = true;
            }

            foreach (var extension in _extensions)
            {
                var ext = extension;
                if (ext.Value.ExtensionProcess == null ||
                    ext.Value.ExtensionProcess.HasExited ||
                    // TODO: Use file watcher...
                    ext.Value.File.LastWriteTimeUtc != new FileInfo(ext.Value.File.FullName).LastWriteTimeUtc ||
                    ext.Value.ShouldDebug != ext.Value.IsDebugging ||
                    ext.Value.ShouldRestart)
                {
                    var extHostPath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "Protogame.Editor.ExtHost.exe");
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = extHostPath,
                        Arguments = 
                            (ext.Value.ShouldDebug ? "--debug " : "") + 
                            "--track " + Process.GetCurrentProcess().Id + 
                            " --editor-url " + _grpcServer.GetServerUrl() + 
                            " --assembly-path \"" + ext.Value.File.FullName + "\"",
                        WorkingDirectory = ext.Value.File.DirectoryName,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    // Update last write time.
                    ext.Value.File = new FileInfo(ext.Value.File.FullName);
                    ext.Value.IsDebugging = ext.Value.ShouldDebug;
                    ext.Value.ShouldRestart = false;
                    if (ext.Value.ExtensionProcess != null)
                    {
                        try
                        {
                            ext.Value.ExtensionProcess.EnableRaisingEvents = false;
                            ext.Value.ExtensionProcess.Kill();
                        }
                        catch { }
                        _consoleHandle.LogDebug("Extension host process was killed for reload: {0}", ext.Value.File.FullName);
                        ext.Value.ExtensionProcess = null;
                        ext.Value.ExtensionChannel = null;
                    }
                    ext.Value.ExtensionProcess = Process.Start(processStartInfo);
                    ext.Value.ExtensionProcess.Exited += (sender, e) =>
                    {
                        _consoleHandle.LogWarning("Extension host process has unexpectedly quit: {0}", ext.Value.File.FullName);
                        _recomputeExtensions = true;
                        ext.Value.ExtensionProcess = null;
                        ext.Value.ExtensionChannel = null;
                        ext.Value.ShouldDebug = false;
                    };
                    ext.Value.ExtensionProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            return;
                        }
                        if (ext.Value.ExtensionChannel != null)
                        {
                            _consoleHandle.LogDebug(e.Data);
                            return;
                        }

                        var editorGrpcServer = _grpcServer.GetServerUrl();
                        _consoleHandle.LogDebug("Editor gRPC server is {0}", editorGrpcServer);

                        var url = e.Data?.Trim();
                        _consoleHandle.LogDebug("Creating gRPC channel on {0}...", url);
                        ext.Value.ExtensionChannel = new Channel(url, ChannelCredentials.Insecure);

                        _recomputeExtensions = true;
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

            if (_recomputeExtensions)
            {
                _publicExtensions = _extensions.Values
                    .Where(x => x.ExtensionChannel != null)
                    .Select(x => new Extension(x.File.Name, x.File.FullName, x.ExtensionChannel))
                    .ToArray();
                _consoleHandle.LogInfo("Recomputed loaded extension list");
                _recomputeExtensions = false;
            }
        }

        private class ManagedExtension
        {
            public FileInfo File { get; set; }

            public string Owner { get; set; }

            public Process ExtensionProcess { get; set; }

            public Channel ExtensionChannel { get; set; }

            public bool ShouldDebug { get; set; }

            public bool IsDebugging { get; set; }

            public bool ShouldRestart { get; set; }
        }
    }
}
