using Grpc.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Protogame.Editor.ProjectManagement;
using Protogame.Editor.Server;
using Protogame.Editor.SharedRendering;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using static Protogame.Editor.Grpc.Editor.GameHoster;
using Protogame.Editor.Grpc.Editor;
using static Protogame.Editor.Grpc.GameHost.GameHostServer;
using Protogame.Editor.Grpc.GameHost;
using System.Linq;

namespace Protogame.Editor.LoadedGame
{
    public class DefaultLoadedGame : ILoadedGame
    {
        private readonly IProjectManager _projectManager;
        private readonly IGrpcServer _grpcServer;
        private readonly IConsoleHandle _consoleHandle;
        private readonly IRenderTargetBackBufferUtilities _renderTargetBackBufferUtilities;
        private readonly SharedRendererHost _sharedRendererHost;
        
        private FileInfo _executingFile;
        private bool _isDebugging;
        private bool _shouldDebug;
        private bool _shouldRestart;
        private Process _process;
        private Channel _channel;
        private GameHostServerClient _gameHostClient;
        private string _baseDirectory;

        private Point _offset;
        private bool _requiresDelaySync;

        public DefaultLoadedGame(
            IConsoleHandle consoleHandle,
            IProjectManager projectManager,
            IGrpcServer grpcServer,
            IRenderTargetBackBufferUtilities renderTargetBackBufferUtilities,
            ISharedRendererHostFactory sharedRendererHostFactory)
        {
            _consoleHandle = consoleHandle;
            _projectManager = projectManager;
            _grpcServer = grpcServer;
            _sharedRendererHost = sharedRendererHostFactory.CreateSharedRendererHost();
            _sharedRendererHost.TexturesRecreated += OnTexturesRecreated;
        }

        private void OnTexturesRecreated(object sender, EventArgs e)
        {
            if (_gameHostClient != null)
            {
                SendTexturesToGameHost();
            }
            else
            {
                _requiresDelaySync = true;
            }
        }

        private void SendTexturesToGameHost()
        {
            var req = new SetRenderTargetsRequest();
            req.SharedPointer.AddRange(_sharedRendererHost.WritableTextureIntPtrs.Select(x => x.ToInt64()));
            req.SyncMmappedFileName = _sharedRendererHost.SynchronisationMemoryMappedFileName;
            try
            {
                _gameHostClient.SetRenderTargets(req);
            }
            catch
            {
                _requiresDelaySync = true;
            }
        }

        public void SetPositionOffset(Point offset)
        {
            _offset = offset;
        }

        public void QueueEvent(Event @event)
        {
            // TODO FIX
        }

        public void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            _sharedRendererHost.UpdateTextures(gameContext, renderContext);
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            if (_requiresDelaySync && _gameHostClient != null)
            {
                SendTexturesToGameHost();
                _requiresDelaySync = false;
            }

            if (_projectManager.Project == null ||
                _projectManager.Project.DefaultGameBinPath == null)
            {
                return;
            }

            if (!_projectManager.Project.DefaultGameBinPath.Exists)
            {
                return;
            }

            if (_process == null ||
                _process.HasExited ||
                // TODO: Use file watcher...
                (_executingFile != null && _executingFile.LastWriteTimeUtc != new FileInfo(_executingFile.FullName).LastWriteTimeUtc) ||
                _shouldDebug != _isDebugging ||
                _shouldRestart)
            {
                var extHostPath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "Protogame.Editor.GameHost.exe");
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = extHostPath,
                    Arguments =
                        (_shouldDebug ? "--debug " : "") +
                        "--track " + Process.GetCurrentProcess().Id +
                        " --editor-url " + _grpcServer.GetServerUrl() +
                        " --assembly-path \"" + _projectManager.Project.DefaultGameBinPath.FullName + "\"",
                    WorkingDirectory = _projectManager.Project.DefaultGameBinPath.DirectoryName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                // Update last write time.
                _baseDirectory = _projectManager.Project.DefaultGameBinPath.DirectoryName;
                _executingFile = new FileInfo(_projectManager.Project.DefaultGameBinPath.FullName);
                _isDebugging = _shouldDebug;
                _shouldRestart = false;
                if (_process != null)
                {
                    try
                    {
                        _process.EnableRaisingEvents = false;
                        _process.Kill();
                    }
                    catch { }
                    _consoleHandle.LogDebug("Game host process was killed for reload: {0}", _projectManager.Project.DefaultGameBinPath.FullName);
                    _process = null;
                    _channel = null;
                    _gameHostClient = null;
                }
                _process = Process.Start(processStartInfo);
                _process.Exited += (sender, e) =>
                {
                    _consoleHandle.LogWarning("Game host process has unexpectedly quit: {0}", _projectManager.Project.DefaultGameBinPath.FullName);
                    _process = null;
                    _channel = null;
                    _gameHostClient = null;
                    _shouldDebug = false;
                };
                _process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        return;
                    }
                    if (_channel != null)
                    {
                        _consoleHandle.LogDebug(e.Data);
                        return;
                    }

                    var editorGrpcServer = _grpcServer.GetServerUrl();
                    _consoleHandle.LogDebug("Editor gRPC server is {0}", editorGrpcServer);

                    var url = e.Data?.Trim();
                    _consoleHandle.LogDebug("Creating gRPC channel on {0}...", url);
                    _channel = new Channel(url, ChannelCredentials.Insecure);
                    _gameHostClient = new GameHostServerClient(_channel);
                };
                _process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        _consoleHandle.LogError(e.Data);
                    }
                };
                _process.EnableRaisingEvents = true;
                _process.BeginErrorReadLine();
                _process.BeginOutputReadLine();
            }
        }

        public string GetBaseDirectory()
        {
            return _baseDirectory;
        }

        public void IncrementReadRenderTargetIfPossible()
        {
            _sharedRendererHost.IncrementReadableTextureIfPossible();
        }

        public RenderTarget2D GetCurrentGameRenderTarget()
        {
            return _sharedRendererHost.ReadableTexture;
        }

        public void SetRenderTargetSize(Point size)
        {
            _sharedRendererHost.Size = size;
        }

        public Point? GetRenderTargetSize()
        {
            return _sharedRendererHost.Size;
        }
    }
}
