using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Protogame.Editor.Api.Version1.ProjectManagement;

#if FALSE

namespace Protogame.Editor.GameHost
{
    public class DefaultLoadedGame : ILoadedGame
    {
        private readonly IProjectManager _projectManager;
        private readonly ICoroutine _coroutine;
        private readonly IConsoleHandle _consoleHandle;
        private readonly IRenderTargetBackBufferUtilities _renderTargetBackBufferUtilities;
        private DateTime? _lastModified;
        private string _lastPath;
        private RenderTarget2D[] _renderTargets;
        private IntPtr[] _renderTargetSharedHandles;
        private int _currentWriteTargetIndex;
        private int _currentReadTargetIndex;
        private Point? _renderTargetSize;
        private GameLoader _gameLoader;
        private bool _hasRunGameUpdate;
        private bool _isReadyForMainThread;
        private bool _mustRestart;
        private bool _mustDestroyRenderTargets;
        private Thread _gameThread;
        private List<Action> _mainThreadTasks;
        private Point _offset;
        private bool _didReadStall;
        private bool _didWriteStall;
        private static object _incrementLock = new object();

        public DefaultLoadedGame(
            IProjectManager projectManager,
            ICoroutine coroutine,
            IConsoleHandle consoleHandle,
            IRenderTargetBackBufferUtilities renderTargetBackBufferUtilities)
        {
            _projectManager = projectManager;
            _coroutine = coroutine;
            _consoleHandle = consoleHandle;
            _renderTargetBackBufferUtilities = renderTargetBackBufferUtilities;
            _renderTargetSize = new Point(640, 480);
            _hasRunGameUpdate = false;
            _isReadyForMainThread = false;
            _mainThreadTasks = new List<Action>();
            _renderTargets = new RenderTarget2D[RenderTargetBufferConfiguration.RTBufferSize];
            _renderTargetSharedHandles = new IntPtr[RenderTargetBufferConfiguration.RTBufferSize];
            _currentWriteTargetIndex = RenderTargetBufferConfiguration.RTBufferSize >= 2 ? 1 : 0;
            _currentReadTargetIndex = 0;
        }

        public void Restart()
        {
            _mustRestart = true;
            _mustDestroyRenderTargets = true;
            Playing = false;
            State = LoadedGameState.Loading;
        }

        public Tuple<bool, bool> GetStallState()
        {
            return new Tuple<bool, bool>(_didReadStall, _didWriteStall);
        }

        public RenderTarget2D GetCurrentGameRenderTarget()
        {
            return _renderTargets[_currentReadTargetIndex];
        }

        public void IncrementReadRenderTargetIfPossible()
        {
            lock (_incrementLock)
            {
                var nextIndex = _currentReadTargetIndex + 1;
                if (nextIndex == RenderTargetBufferConfiguration.RTBufferSize) { nextIndex = 0; }

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

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            
            foreach (var act in _mainThreadTasks)
            {
                act();
            }

            _mainThreadTasks.Clear();

            if (_gameLoader != null)
            {
                int x = 0;
                int y = 0;
                if (_gameLoader.GetMousePositionToSet(ref x, ref y))
                {
                    Mouse.SetPosition(x + _offset.X, y + _offset.Y);
                    _gameLoader.SetMousePositionToGet(x, y);
                }
                else
                {
                    var state = Mouse.GetState();
                    _gameLoader.SetMousePositionToGet(state.X - _offset.X, state.Y - _offset.Y);
                }
            }

            if (_projectManager.Project != null && _projectManager.Project.DefaultGameBinPath != null)
            {
                // We have a binary for the game that we can load.

                // Check the binary exists.
                if (!_projectManager.Project.DefaultGameBinPath.Exists)
                {
                    return;
                }

                // If we have already loaded the app domain, and the last modified date of
                // the DLL is the same, then no need to reload.
                if (_lastModified == _projectManager.Project.DefaultGameBinPath.LastWriteTimeUtc && 
                    _lastPath == _projectManager.Project.DefaultGameBinPath.FullName &&
                    _gameThread != null &&
                     _gameThread.ThreadState != System.Threading.ThreadState.Stopped &&
                    !_mustRestart)
                {
                    return;
                }
                
                // Abort the game thread if we plan on reloading.
                if (_gameThread != null)
                {
                    _gameThread.Abort();
                    _gameThread = null;

                    if (_mustRestart)
                    {
                        _playingFor = TimeSpan.Zero;
                        _playingStartTime = null;
                    }
                }

                // We need to reload the app domain.  Kick off a coroutine to handle it.
                _lastModified = _projectManager.Project.DefaultGameBinPath.LastWriteTimeUtc;
                _lastPath = _projectManager.Project.DefaultGameBinPath.FullName;
                _isReadyForMainThread = false;
                _mustRestart = false;
                _gameThread = new Thread(GameThreadRun);
                _gameThread.Name = "Game Hosting Thread";
                _gameThread.IsBackground = true;
                _gameThread.Start();
            }
        }

        public void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            if (_mustDestroyRenderTargets)
            {
                for (var i = 0; i < RenderTargetBufferConfiguration.RTBufferSize; i++)
                {
                    if (_renderTargets[i] != null)
                    {
                        _renderTargets[i].Dispose();
                        _renderTargets[i] = null;
                    }
                }

                _mustDestroyRenderTargets = false;
            }

            for (var i = 0; i < RenderTargetBufferConfiguration.RTBufferSize; i++)
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

        private void QueueAction(Action act)
        {
            _mainThreadTasks.Add(act);
        }

        /*
        private void GameThreadRun()
        {
            QueueAction(() => _consoleHandle.LogDebug("Starting load of game appdomain"));

            if (_appDomain != null)
            {
                QueueAction(() => _consoleHandle.LogDebug("Unloading existing appdomain first"));
                _gameLoader = null;
                AppDomain.Unload(_appDomain);
                _appDomain = null;
            }

            var domaininfo = new AppDomainSetup();
            domaininfo.ApplicationBase = _projectManager.Project.DefaultGameBinPath.DirectoryName;
            domaininfo.DisallowApplicationBaseProbing = false;
            QueueAction(() => _consoleHandle.LogDebug("Game appdomain will have base of {0}", domaininfo.ApplicationBase));
            _appDomain = AppDomain.CreateDomain("LoadedGame", null, new AppDomainSetup
            {
                LoaderOptimization = LoaderOptimization.MultiDomain,
                ShadowCopyFiles = "true",
            });
            var monogameType = typeof(Microsoft.Xna.Framework.Curve);
            var protogameType = typeof(Protogame.ProtogameBaseModule);
            _appDomain.AssemblyResolve += new GameLoaderContext(
                new FileInfo(monogameType.Assembly.Location).DirectoryName,
                domaininfo.ApplicationBase).ResolveAssembly;

            // Create the game loader.
            _gameLoader = (GameLoader)_appDomain.CreateInstanceFromAndUnwrap(typeof(GameLoader).Assembly.Location, typeof(GameLoader).FullName);

            // Load the game assemblies.
            _lastModified = _projectManager.Project.DefaultGameBinPath.LastWriteTimeUtc;
            _lastPath = _projectManager.Project.DefaultGameBinPath.FullName;
            _gameLoader.LoadFromPath(
                new MarshallableConsoleHandle(_consoleHandle),
                new GameBaseDirectory(_projectManager),
                new GameBackBufferDimensions(this),
                _projectManager.Project.DefaultGameBinPath.FullName);

            _hasRunGameUpdate = false;
            _hasAssignedGame = false;
            _isReadyForMainThread = true;

            QueueAction(() => _consoleHandle.LogDebug("GameLoader LoadFromPath has completed (now outside appdomain)"));

            State = LoadedGameState.Loaded;

            // Now run the main game loop.
        }
        */
        
        public void QueueEvent(Event @event)
        {
            if (State == LoadedGameState.Playing)
            {
                _gameLoader.QueueEvent(@event);
            }
        }

        private class MarshallableConsoleHandle : MarshalByRefObject, IConsoleHandle
        {
            private readonly IConsoleHandle _realImpl;

            public MarshallableConsoleHandle(IConsoleHandle realImpl)
            {
                _realImpl = realImpl;
            }

            public void Log(string messageFormat)
            {
                _realImpl.Log(messageFormat);
            }

            public void Log(string messageFormat, params object[] objects)
            {
                _realImpl.Log(messageFormat, objects);
            }

            public void LogDebug(string messageFormat)
            {
                _realImpl.LogDebug(messageFormat);
            }

            public void LogDebug(string messageFormat, params object[] objects)
            {
                _realImpl.LogDebug(messageFormat, objects);
            }

            public void LogError(string messageFormat)
            {
                _realImpl.LogError(messageFormat);
            }

            public void LogError(string messageFormat, params object[] objects)
            {
                _realImpl.LogError(messageFormat, objects);
            }

            public void LogError(Exception exception)
            {
                _realImpl.LogError(exception);
            }

            public void LogInfo(string messageFormat)
            {
                _realImpl.LogInfo(messageFormat);
            }

            public void LogInfo(string messageFormat, params object[] objects)
            {
                _realImpl.LogInfo(messageFormat, objects);
            }

            public void LogWarning(string messageFormat)
            {
                _realImpl.LogWarning(messageFormat);
            }

            public void LogWarning(string messageFormat, params object[] objects)
            {
                _realImpl.LogWarning(messageFormat, objects);
            }
        }
    }
}

#endif