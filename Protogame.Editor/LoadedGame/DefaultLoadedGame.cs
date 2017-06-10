using Protogame.Editor.GameHost;
using Protogame.Editor.ProjectManagement;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections;
using Protogame.Editor.Override;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace Protogame.Editor.LoadedGame
{
    public class DefaultLoadedGame : ILoadedGame
    {
        private readonly IProjectManager _projectManager;
        private readonly ICoroutine _coroutine;
        private readonly IConsoleHandle _consoleHandle;
        private readonly IRenderTargetBackBufferUtilities _renderTargetBackBufferUtilities;
        private DateTime? _lastModified;
        private string _lastPath;
        private AppDomain _appDomain;
        private RenderTarget2D[] _renderTargets;
        private IntPtr[] _renderTargetSharedHandles;
        private int _currentWriteTargetIndex;
        private int _currentReadTargetIndex;
        private Point? _renderTargetSize;
        private GameLoader _gameLoader;
        private bool _hasRunGameUpdate;
        private bool _hasAssignedGame;
        private bool _isReadyForMainThread;
        private bool _mustRestart;
        private bool _mustDestroyRenderTargets;
        private Thread _gameThread;
        private List<Action> _mainThreadTasks;
        private Point _offset;
        private bool _didReadStall;
        private bool _didWriteStall;
        private static object _incrementLock = new object();
        private TimeSpan _playingFor = TimeSpan.Zero;
        private DateTime? _playingStartTime = null;

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

            State = LoadedGameState.Loading;
            Playing = false;
        }

        public LoadedGameState State
        {
            get;
            private set;
        }

        public TimeSpan PlayingFor => _playingFor;

        public bool Playing
        {
            get;
            set;
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
            if (Playing)
            {
                if (State == LoadedGameState.Paused ||
                    State == LoadedGameState.Loaded)
                {
                    State = LoadedGameState.Playing;
                }
            }
            else
            {
                if (State == LoadedGameState.Playing)
                {
                    State = LoadedGameState.Paused;
                }
            }
            
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
                if (_appDomain != null && 
                    _lastModified == _projectManager.Project.DefaultGameBinPath.LastWriteTimeUtc && 
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
            try
            {
                _gameTimer = Stopwatch.StartNew();

                while (true)
                {
                    try
                    {
                        Tick();
                    }
                    catch (ThreadAbortException ex)
                    {
                        QueueAction(() => _consoleHandle.LogDebug("Game has been requested to close..."));
                        break;
                    }
                    catch (Exception ex)
                    {
                        _consoleHandle.LogError(ex);
                        Playing = false;
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                QueueAction(() => _consoleHandle.LogDebug("Game has been requested to close..."));
            }
            finally
            {

            }
        }

        private TimeSpan _accumulatedElapsedTime;
        private readonly GameTime _gameTime = new GameTime();
        private Stopwatch _gameTimer;
        private long _previousTicks = 0;
        private int _updateFrameLag;
        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166667);
        private bool IsFixedTimeStep = true;
        private bool _shouldExit = false;
        private bool _suppressDraw = false;

        public void Tick()
        {
            // NOTE: This code is very sensitive and can break very badly
            // with even what looks like a safe change.  Be sure to test 
            // any change fully in both the fixed and variable timestep 
            // modes across multiple devices and platforms.

            RetryTick:

            // Advance the accumulated elapsed time.
            var currentTicks = _gameTimer.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
            _previousTicks = currentTicks;

            // If we're in the fixed timestep mode and not enough time has elapsed
            // to perform an update we sleep off the the remaining time to save battery
            // life and/or release CPU time to other threads and processes.
            if (IsFixedTimeStep && _accumulatedElapsedTime < _targetElapsedTime)
            {
                var sleepTime = (int)(_targetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;

                // NOTE: While sleep can be inaccurate in general it is 
                // accurate enough for frame limiting purposes if some
                // fluctuation is an acceptable result.
                if (false/* && graphicsDeviceManager.SynchronizeWithVerticalRetrace*/)
                {
                    // NOTE: While sleep can be inaccurate in general it is 
                    // accurate enough for frame limiting purposes if some
                    // fluctuation is an acceptable result.
#if WINRT
                    Task.Delay(sleepTime).Wait();
#else
                    System.Threading.Thread.Sleep(sleepTime);
#endif
                    goto RetryTick;
                }
                else
                {
                    // Draw until we have used up our time.
                    DoDraw(_gameTime);

                    goto RetryTick;
                }
            }

            // Do not allow any update to take longer than our maximum.
            if (_accumulatedElapsedTime > _maxElapsedTime)
                _accumulatedElapsedTime = _maxElapsedTime;

            if (IsFixedTimeStep)
            {
                _gameTime.ElapsedGameTime = _targetElapsedTime;
                var stepCount = 0;

                // Perform as many full fixed length time steps as we can.
                while (_accumulatedElapsedTime >= _targetElapsedTime && !_shouldExit)
                {
                    _gameTime.TotalGameTime += _targetElapsedTime;
                    _accumulatedElapsedTime -= _targetElapsedTime;
                    ++stepCount;

                    DoUpdate(_gameTime);
                }

                //Every update after the first accumulates lag
                _updateFrameLag += Math.Max(0, stepCount - 1);

                //If we think we are running slowly, wait until the lag clears before resetting it
                if (_gameTime.IsRunningSlowly)
                {
                    if (_updateFrameLag == 0)
                        _gameTime.IsRunningSlowly = false;
                }
                else if (_updateFrameLag >= 5)
                {
                    //If we lag more than 5 frames, start thinking we are running slowly
                    _gameTime.IsRunningSlowly = true;
                }

                //Every time we just do one update and one draw, then we are not running slowly, so decrease the lag
                if (stepCount == 1 && _updateFrameLag > 0)
                    _updateFrameLag--;

                // Draw needs to know the total elapsed time
                // that occured for the fixed length updates.
                _gameTime.ElapsedGameTime = TimeSpan.FromTicks(_targetElapsedTime.Ticks * stepCount);
            }
            else
            {
                // Perform a single variable length update.
                _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
                _gameTime.TotalGameTime += _accumulatedElapsedTime;
                _accumulatedElapsedTime = TimeSpan.Zero;

                DoUpdate(_gameTime);
            }

            // Draw unless the update suppressed it.
            if (_suppressDraw)
                _suppressDraw = false;
            else
            {
                DoDraw(_gameTime);
            }

            //if (_shouldExit)
            //    Platform.Exit();
        }

        private void DoUpdate(GameTime gameTime)
        {
            if (!_isReadyForMainThread || _gameLoader == null)
            {
                return;
            }
            
            if (!_hasAssignedGame)
            {
                QueueAction(() => _consoleHandle.LogDebug("Assigning host instance to game"));
                _gameLoader.CreateHost();
                QueueAction(() => _consoleHandle.LogDebug("Assigned host instance to game"));
                _hasAssignedGame = true;
            }

            if (State == LoadedGameState.Playing)
            {
                _gameLoader.Update(gameTime.ElapsedGameTime, gameTime.TotalGameTime);
            }
            else
            {
                _gameLoader.UpdateForLoadContentOnly(gameTime.ElapsedGameTime, gameTime.TotalGameTime);
                Thread.Sleep(0);
            }

            _hasRunGameUpdate = true;
        }

        private void DoDraw(GameTime gameTime)
        {
            if (_gameLoader == null || _renderTargetSize == null || !_hasRunGameUpdate)
            {
                return;
            }

            if (State != LoadedGameState.Playing)
            {
                return;
            }
            
            _gameLoader.SetRenderTargetPointers(_renderTargetSharedHandles, _currentWriteTargetIndex);

            _gameLoader.Render(gameTime.ElapsedGameTime, gameTime.TotalGameTime);

            lock (_incrementLock)
            {
                var nextIndex = _currentWriteTargetIndex + 1;
                if (nextIndex == RenderTargetBufferConfiguration.RTBufferSize) { nextIndex = 0; }

                if (nextIndex != _currentReadTargetIndex)
                {
                    // Only move the write index if the one we want is not the one
                    // we're currently reading from.
                    _currentWriteTargetIndex = nextIndex;
                    _didWriteStall = false;
                }
                else
                {
                    _didWriteStall = true;
                }
            }

            if (_playingStartTime == null)
            {
                _playingStartTime = DateTime.Now;
            }

            _playingFor = DateTime.Now - _playingStartTime.Value;
        }
        
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
