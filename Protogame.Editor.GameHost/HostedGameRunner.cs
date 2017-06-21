using Microsoft.Xna.Framework;
using Protogame.Editor.Api.Version1.Core;
using Protogame.Editor.CommonHost;
using Protogame.Editor.CommonHost.SharedRendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Protogame.Editor.Grpc.Editor.GameHoster;

namespace Protogame.Editor.GameHost
{
    public class HostedGameRunner : IGameRunner
    {
        private readonly ICoreGame _game;
        private readonly ILogShipping _logShipping;
        private readonly Api.Version1.Core.IConsoleHandle _consoleHandle;
        private readonly ISharedRendererClientFactory _sharedRendererClientFactory;
        private readonly IWantsUpdateSignal[] _wantsUpdateSignals;
        private readonly GameHosterClient _gameHosterClient;

        private EditorHostGame _editorHostGame;
        private bool _hasAssignedGame;
        private bool _hasRunGameUpdate;
        private IntPtr[] _sharedResourceHandles;
        private int _currentWriteIndex;
        
        private DateTime? _playingStartTime = null;

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
        private string _sharedMmapName;
        private bool _delayAssignSharedResourceHandles;
        private bool _playing;

        public HostedGameRunner(
            ICoreGame game,
            ILogShipping logShipping,
            Api.Version1.Core.IConsoleHandle consoleHandle,
            ISharedRendererClientFactory sharedRendererClientFactory,
            IWantsUpdateSignal[] wantsUpdateSignals,
            IEditorClientProvider editorClientProvider)
        {
            _game = game;
            _logShipping = logShipping;
            _consoleHandle = consoleHandle;
            _sharedRendererClientFactory = sharedRendererClientFactory;
            _wantsUpdateSignals = wantsUpdateSignals;
            _gameHosterClient = editorClientProvider.GetClient<GameHosterClient>();

            State = LoadedGameState.Loaded;
            _playing = false;
        }

        public LoadedGameState State
        {
            get;
            private set;
        }

        public void SetPlaybackMode(bool playing)
        {
            _playing = playing;
        }

        public void Run()
        {
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
                        Console.Error.WriteLine("Game has been requested to close...");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                        _playing = false;
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                Console.Error.WriteLine("Game has been requested to close...");
            }
            finally
            {

            }
        }

        public void SetMousePositionToGet(int x, int y)
        {
            _editorHostGame?.SetMousePositionToGet(x, y);
        }

        public bool GetMousePositionToSet(ref int x, ref int y)
        {
            if (_editorHostGame == null)
            {
                return false;
            }

            return _editorHostGame.GetMousePositionToSet(ref x, ref y);
        }

        private void Tick()
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
            foreach (var ws in _wantsUpdateSignals)
            {
                ws.Update();
            }

            if (_playing)
            {
                if (State == LoadedGameState.Paused ||
                    State == LoadedGameState.Loaded)
                {
                    State = LoadedGameState.Playing;

                    SyncPlaybackStateToEditor();
                }
            }
            else
            {
                if (State == LoadedGameState.Playing)
                {
                    State = LoadedGameState.Paused;

                    SyncPlaybackStateToEditor();
                }
            }

            /*if (!_isReadyForMainThread || _gameLoader == null)
            {
                return;
            }
            */

            if (!_hasAssignedGame)
            {
                InternalLog("Assigning host instance to game");
                _editorHostGame = new EditorHostGame(_game, _sharedRendererClientFactory);
                InternalLog("Assigned host instance to game");
                _hasAssignedGame = true;

                SyncPlaybackStateToEditor();
            }

            if (State == LoadedGameState.Playing)
            {
                try
                {
                    _editorHostGame?.Update(gameTime.TotalGameTime, gameTime.ElapsedGameTime);
                }
                finally
                {
                    FlushLogs();
                }
            }
            else
            {
                if (!(_game?.HasLoadedContent ?? true))
                {
                    _editorHostGame?.Update(gameTime.TotalGameTime, gameTime.ElapsedGameTime);
                }
                
                Thread.Sleep(0);
            }

            _hasRunGameUpdate = true;
        }

        private void DoDraw(GameTime gameTime)
        {
            /*if (_gameLoader == null || _renderTargetSize == null || !_hasRunGameUpdate)
            {
                return;
            }*/

            if (_delayAssignSharedResourceHandles)
            {
                _consoleHandle.LogInfo("Delay assigning shared textures from editor for game hosting...");
                _consoleHandle.LogInfo("Shared texture count: " + (_sharedResourceHandles == null ? "<null>" : _sharedResourceHandles.Length.ToString()));
                _consoleHandle.LogInfo("Shared memory mapped filename: " + _sharedMmapName);

                _editorHostGame.SetSharedResourceHandles(_sharedResourceHandles, _sharedMmapName);
                _delayAssignSharedResourceHandles = false;
            }

            if (State != LoadedGameState.Playing)
            {
                return;
            }

            try
            {
                _editorHostGame?.Render(gameTime.TotalGameTime, gameTime.ElapsedGameTime);
            }
            finally
            {
                FlushLogs();
            }

            _editorHostGame?.IncrementWritableTextureIfPossible();

            if (_playingStartTime == null)
            {
                _playingStartTime = DateTime.Now;

                SyncPlaybackStateToEditor();
            }
        }

        private void InternalLog(string message)
        {
            _consoleHandle.LogDebug(message);
        }
        
        private void FlushLogs()
        {
            var logs = _logShipping.GetAndFlushLogs();
            foreach (var l in logs)
            {
                switch (l.LogLevel)
                {
                    case ConsoleLogLevel.Debug:
                        _consoleHandle.LogDebug(l.Message);
                        break;
                    case ConsoleLogLevel.Info:
                        _consoleHandle.LogInfo(l.Message);
                        break;
                    case ConsoleLogLevel.Warning:
                        _consoleHandle.LogWarning(l.Message);
                        break;
                    case ConsoleLogLevel.Error:
                        _consoleHandle.LogError(l.Message);
                        break;
                }
            }
        }

        private void SyncPlaybackStateToEditor()
        {
            Grpc.Editor.PlaybackState state = Grpc.Editor.PlaybackState.Loading;
            switch (State)
            {
                case LoadedGameState.Loading:
                    state = Grpc.Editor.PlaybackState.Loading;
                    break;
                case LoadedGameState.Loaded:
                    state = Grpc.Editor.PlaybackState.Loaded;
                    break;
                case LoadedGameState.Playing:
                    state = Grpc.Editor.PlaybackState.Playing;
                    break;
                case LoadedGameState.Paused:
                    state = Grpc.Editor.PlaybackState.Paused;
                    break;
            }

            Grpc.Editor.Timestamp timestamp = null;
            if (_playingStartTime != null)
            {
                timestamp = new Grpc.Editor.Timestamp
                {
                    UnixTimestamp = (UInt64)(_playingStartTime.Value.Subtract(new DateTime(1970, 1, 1))).TotalSeconds
                };
            }

            _gameHosterClient.PlaybackStateChanged(new Grpc.Editor.PlaybackStateChangedRequest
            {
                State = state,
                StartTime = timestamp
            });
        }

        public void SetHandles(IntPtr[] sharedTextures, string sharedMmapName)
        {
            _consoleHandle.LogInfo("Received shared textures from editor for game hosting...");
            _consoleHandle.LogInfo("Shared texture count: " + (sharedTextures == null ? "<null>" : sharedTextures.Length.ToString()));
            _consoleHandle.LogInfo("Shared memory mapped filename: " + sharedMmapName);

            _sharedResourceHandles = sharedTextures;
            _sharedMmapName = sharedMmapName;

            if (_editorHostGame == null)
            {
                _consoleHandle.LogWarning("Needs delay-assignment of shared resource handles");
                _delayAssignSharedResourceHandles = true;
            }
            else
            {
                _editorHostGame.SetSharedResourceHandles(_sharedResourceHandles, _sharedMmapName);
            }
        }
    }
}
