using Grpc.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Protogame.Editor.ProjectManagement;
using Protogame.Editor.Server;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Protogame.Editor.LoadedGame
{
    public class DefaultLoadedGame : ILoadedGame
    {
        private readonly IProjectManager _projectManager;
        private readonly IGrpcServer _grpcServer;
        private readonly IConsoleHandle _consoleHandle;
        private readonly IRenderTargetBackBufferUtilities _renderTargetBackBufferUtilities;

        private FileInfo _executingFile;
        private bool _isDebugging;
        private bool _shouldDebug;
        private bool _shouldRestart;
        private Process _process;
        private Channel _channel;

        private bool _mustDestroyRenderTargets;
        private int _currentWriteTargetIndex;
        private int _currentReadTargetIndex;
        private RenderTarget2D[] _renderTargets;
        private IntPtr[] _renderTargetSharedHandles;
        private Point? _renderTargetSize;
        private int RTBufferSize = 3;

        private Point _offset;

        private bool _didReadStall;
        private bool _didWriteStall;

        private static object _incrementLock = new object();

        public DefaultLoadedGame(
            IConsoleHandle consoleHandle,
            IProjectManager projectManager,
            IGrpcServer grpcServer,
            IRenderTargetBackBufferUtilities renderTargetBackBufferUtilities)
        {
            _consoleHandle = consoleHandle;
            _projectManager = projectManager;
            _grpcServer = grpcServer;
            _renderTargetBackBufferUtilities = renderTargetBackBufferUtilities;
        }

        public void SetPositionOffset(Point offset)
        {
            _offset = offset;
        }

        public void SetRenderTargetSize(Point size)
        {
            _renderTargetSize = size;
        }

        public Point? GetRenderTargetSize()
        {
            return _renderTargetSize;
        }

        public RenderTarget2D GetCurrentGameRenderTarget()
        {
            return _renderTargets[_currentReadTargetIndex];
        }

        public void QueueEvent(Event @event)
        {
            // TODO FIX
        }

        public void IncrementReadRenderTargetIfPossible()
        {
            lock (_incrementLock)
            {
                var nextIndex = _currentReadTargetIndex + 1;
                if (nextIndex == RTBufferSize) { nextIndex = 0; }

                if (nextIndex != _currentWriteTargetIndex)
                {
                    // Only move the write index if the one we want is not the one
                    // we're currently reading from.
                    _currentReadTargetIndex = nextIndex;
                    _didReadStall = false;
                }
                else
                {
                    _didReadStall = true;
                }
            }
        }

        public void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            if (_mustDestroyRenderTargets)
            {
                for (var i = 0; i < RTBufferSize; i++)
                {
                    if (_renderTargets[i] != null)
                    {
                        _renderTargets[i].Dispose();
                        _renderTargets[i] = null;
                    }
                }

                _mustDestroyRenderTargets = false;
            }

            for (var i = 0; i < RTBufferSize; i++)
            {
                var oldRenderTarget = _renderTargets[i];
                _renderTargets[i] = _renderTargetBackBufferUtilities.UpdateCustomSizedRenderTarget(
                    _renderTargets[i],
                    renderContext,
                    _renderTargetSize.Value.ToVector2(),
                    null,
                    null,
                    0, // We must NOT have MSAA on this render target for sharing to work properly!
                    true);
                _renderTargetSharedHandles[i] = _renderTargets[i]?.GetSharedHandle() ?? IntPtr.Zero;
                if (_renderTargets[i] != null && _renderTargets[i] != oldRenderTarget)
                {
                    // Release the lock we will have.
                    _renderTargets[i].ReleaseLock(1234);
                }
            }
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            if (!_projectManager.Project.DefaultGameBinPath.Exists)
            {
                return;
            }

            if (true)
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
                }
                _process = Process.Start(processStartInfo);
                _process.Exited += (sender, e) =>
                {
                    _consoleHandle.LogWarning("Game host process has unexpectedly quit: {0}", _projectManager.Project.DefaultGameBinPath.FullName);
                    _process = null;
                    _channel = null;
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
    }
}
