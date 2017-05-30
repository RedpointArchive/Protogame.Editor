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

namespace Protogame.Editor.LoadedGame
{
    public class DefaultLoadedGame : ILoadedGame
    {
        private readonly IProjectManager _projectManager;
        private readonly ICoroutine _coroutine;
        private readonly IConsoleHandle _consoleHandle;
        private readonly IRenderTargetBackBufferUtilities _renderTargetBackBufferUtilities;
        private DateTime? _lastModified;
        private AppDomain _appDomain;
        private Task _appDomainLoadingCoroutine;
        private RenderTarget2D _renderTarget2D;
        private Point? _renderTargetSize;
        private GameLoader _gameLoader;
        private bool _hasRunGameUpdate;
        private Dictionary<string, object> _crossDomainData;
        private bool _hasAssignedGame;
        private bool _isReadyForMainThread;
        private bool _mustRestart;

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

            State = LoadedGameState.Loading;
            Playing = false;
        }

        public LoadedGameState State
        {
            get;
            private set;
        }

        public bool Playing
        {
            get;
            set;
        }

        public void Restart()
        {
            _mustRestart = true;
            Playing = false;
            State = LoadedGameState.Loading;
        }

        public Texture2D GetGameRenderTarget()
        {
            return _renderTarget2D;
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
                if (_appDomain != null && _lastModified == _projectManager.Project.DefaultGameBinPath.LastWriteTimeUtc && !_mustRestart)
                {
                    return;
                }

                // If the app domain loading coroutine is running, skip.
                if (_appDomainLoadingCoroutine != null && !_appDomainLoadingCoroutine.IsCompleted)
                {
                    return;
                }
                if (_appDomainLoadingCoroutine != null && _appDomainLoadingCoroutine.IsFaulted)
                {
                    throw new AggregateException(_appDomainLoadingCoroutine.Exception);
                }

                // We need to reload the app domain.  Kick off a coroutine to handle it.
                _isReadyForMainThread = false;
                _mustRestart = false;
                _appDomainLoadingCoroutine = Task.Run(LoadAppDomain);
            }
        }

        public void UpdateGame(IGameContext gameContext, IUpdateContext updateContext)
        {
            if (!_isReadyForMainThread || _gameLoader == null)
            {
                return;
            }

            try
            {
                if (!_hasAssignedGame)
                {
                    _crossDomainData.Add("GraphicsDevice", gameContext.Game.GraphicsDevice);
                    _crossDomainData.Add("GraphicsDeviceManager", gameContext.Game.GraphicsDeviceManager);
                    _crossDomainData.Add("ProtogameWindow", new EditorGameWindow(this, gameContext.Game.HostGame.Window));
                    _crossDomainData.Add("Window", gameContext.Game.HostGame.Window);
                    _crossDomainData.Add("Services", gameContext.Game.HostGame.Services);
                    _crossDomainData.Add("ContentManager", gameContext.Game.HostGame.Content);
                    _gameLoader.CreateHostProxyFromCrossDomainDictionary();
                    _hasAssignedGame = true;
                }

                if (State == LoadedGameState.Playing)
                {
                    _gameLoader.Update(gameContext.GameTime.ElapsedGameTime, gameContext.GameTime.TotalGameTime);
                }
                else
                {
                    _gameLoader.UpdateForLoadContentOnly(gameContext.GameTime.ElapsedGameTime, gameContext.GameTime.TotalGameTime);
                }

                _hasRunGameUpdate = true;
            }
            catch (Exception ex)
            {
                _consoleHandle.LogError(ex.Message + ex.StackTrace);
            }
        }
        
        public void RenderGame(IGameContext gameContext, IRenderContext renderContext)
        {
            if (_gameLoader == null || _renderTargetSize == null || !_hasRunGameUpdate)
            {
                return;
            }

            if (State != LoadedGameState.Playing)
            {
                return;
            }

            try
            {
                _renderTarget2D = _renderTargetBackBufferUtilities.UpdateCustomSizedRenderTarget(
                    _renderTarget2D,
                    gameContext,
                    _renderTargetSize.Value.ToVector2(),
                    null,
                    null,
                    null);

                var oldRenderTargets = renderContext.GraphicsDevice.GetRenderTargets();
                renderContext.GraphicsDevice.SetRenderTarget(_renderTarget2D);

                _gameLoader.Render(gameContext.GameTime.ElapsedGameTime, gameContext.GameTime.TotalGameTime);
            
                renderContext.GraphicsDevice.SetRenderTargets(oldRenderTargets);
            }
            catch (Exception ex)
            {
                _consoleHandle.LogError(ex.Message + ex.StackTrace);
            }
        }

        private async Task LoadAppDomain()
        {
            _consoleHandle.LogDebug("Waiting for things to settle before loading game appdomain");

            await Task.Delay(1000);

            _consoleHandle.LogDebug("Starting load of game appdomain");

            if (_appDomain != null)
            {
                _consoleHandle.LogDebug("Unloading existing appdomain first");
                _gameLoader = null;
                // TODO: Restore this when we're using marshalled objects for MonoGame instead of cross-domain hacks.
                //AppDomain.Unload(_appDomain);
                _appDomain = null;
            }

            var domaininfo = new AppDomainSetup();
            domaininfo.ApplicationBase = _projectManager.Project.DefaultGameBinPath.DirectoryName;
            domaininfo.DisallowApplicationBaseProbing = false;
            _consoleHandle.LogDebug("Game appdomain will have base of {0}", domaininfo.ApplicationBase);
            _appDomain = AppDomain.CreateDomain("LoadedGame", null, new AppDomainSetup
            {
                LoaderOptimization = LoaderOptimization.MultiDomain
            });
            var monogameType = typeof(Microsoft.Xna.Framework.Curve);
            var protogameType = typeof(Protogame.ProtogameBaseModule);
            _appDomain.AssemblyResolve += new GameLoaderContext(
                new FileInfo(monogameType.Assembly.Location).DirectoryName,
                domaininfo.ApplicationBase).ResolveAssembly;

            // Create the game loader.
            _gameLoader = (GameLoader)_appDomain.CreateInstanceFromAndUnwrap(typeof(GameLoader).Assembly.Location, typeof(GameLoader).FullName);

            // Setup our cross-domain data store.
            var domainGate = _gameLoader.CreateDomainGate();
            _crossDomainData = new Dictionary<string, object>();
            DomainGate.Send(domainGate, _crossDomainData);

            // Load the game assemblies.
            _lastModified = _projectManager.Project.DefaultGameBinPath.LastWriteTimeUtc;
            _crossDomainData["BaseDirectory"] = new GameBaseDirectory(_projectManager);
            _crossDomainData["BackBufferDimensions"] = new GameBackBufferDimensions(this);
            _gameLoader.LoadFromPath(
                new MarshallableConsoleHandle(_consoleHandle),
                _projectManager.Project.DefaultGameBinPath.FullName);

            _hasRunGameUpdate = false;
            _hasAssignedGame = false;
            _isReadyForMainThread = true;

            _consoleHandle.LogDebug("GameLoader LoadFromPath has completed (now outside appdomain)");

            State = LoadedGameState.Loaded;
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
