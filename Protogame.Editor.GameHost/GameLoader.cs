using Protoinject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Protogame.Editor.GameHost
{
    public class GameLoader
    {
        private ICoreGame _game;
        private IntPtr _sharedResourceHandle;
        private EditorHostGame _editorHostGame;
        private EditorEventEngineHook _editorEventEngineHook;
        private ILogShipping _logShipping;
        private IConsoleHandle _consoleHandle;

        public void LoadFromPath(
            IConsoleHandle consoleHandle,
            IBaseDirectory baseDirectory,
            IBackBufferDimensions backBufferDimensions,
            string gameAssembly)
        {
            // Load the target assembly.
            consoleHandle.LogDebug("Loading game assembly from " + gameAssembly + "...");
            var assembly = Assembly.LoadFrom(gameAssembly);

            consoleHandle.LogDebug("Constructing standard kernel...");
            var kernel = new StandardKernel();
            kernel.Bind<IRawLaunchArguments>()
                .ToMethod(x => new DefaultRawLaunchArguments(new string[0]))
                .InSingletonScope();

            // Bind our extension hook first so that it runs before everything else.
            kernel.Bind<IEngineHook>().To<ExtensionEngineHook>().InSingletonScope();

            Func<System.Reflection.Assembly, Type[]> TryGetTypes = a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return new Type[0];
                }
            };

            consoleHandle.LogDebug("Finding configuration classes in " + gameAssembly + "...");
            var typeSource = new List<Type>();
            foreach (var attribute in assembly.GetCustomAttributes(false))
            {
                if (attribute.GetType().FullName == "Protogame.ConfigurationAttribute")
                {
                    typeSource.Add(((ConfigurationAttribute)attribute).GameConfigurationOrServerClass);
                }
            }

            if (typeSource.Count == 0)
            {
                // Scan all types to find implementors of IGameConfiguration
                typeSource.AddRange(from type in TryGetTypes(assembly)
                                    select type);
            }

            consoleHandle.LogDebug("Found {0} configuration classes in " + gameAssembly, typeSource.Count);

            consoleHandle.LogDebug("Constructing game configurations...");
            var gameConfigurations = new List<IGameConfiguration>();
            foreach (var type in typeSource)
            {
                if (typeof(IGameConfiguration).IsAssignableFrom(type) &&
                    !type.IsInterface && !type.IsAbstract)
                {
                    gameConfigurations.Add(Activator.CreateInstance(type) as IGameConfiguration);
                }
            }

            ICoreGame game = null;
            var hasBoundNewEventEngine = false;

            consoleHandle.LogDebug("Configuring kernel and constructing game instance ({0} configurations)...", gameConfigurations.Count);
            foreach (var configuration in gameConfigurations)
            {
                consoleHandle.LogDebug("Configuring with {0}...", configuration.GetType().FullName);

                configuration.ConfigureKernel(kernel);

                // Rebind services so the game renders correctly inside the editor.
                kernel.Rebind<IBaseDirectory>().ToMethod(x => baseDirectory).InSingletonScope();
                kernel.Rebind<IBackBufferDimensions>().ToMethod(x => backBufferDimensions).InSingletonScope();
                kernel.Rebind<IDebugRenderer>().To<DefaultDebugRenderer>().InSingletonScope();
                var bindings = kernel.GetCopyOfBindings();
                var mustBindNewEventEngine = false;
                if (bindings.ContainsKey(typeof(IEngineHook)))
                {
                    if (bindings[typeof(IEngineHook)].Any(x => x.Target == typeof(EventEngineHook)))
                    {
                        mustBindNewEventEngine = !hasBoundNewEventEngine;
                        kernel.UnbindSpecific<IEngineHook>(x => x.Target == typeof(EventEngineHook));
                    }

                    if (mustBindNewEventEngine)
                    {
                        kernel.Bind<IEngineHook>().ToMethod(ctx =>
                        {
                            _editorEventEngineHook = ctx.Kernel.Get<EditorEventEngineHook>(ctx.Parent);
                            return _editorEventEngineHook;
                        }).InSingletonScope();
                    }
                }

                if (game == null)
                {
                    game = configuration.ConstructGame(kernel);
                }
            }

            if (game != null)
            {
                consoleHandle.LogDebug("Game instance is {0}", game.GetType().FullName);
            }

            _game = game;
            _logShipping = kernel.Get<ILogShipping>();
            _consoleHandle = consoleHandle;

            consoleHandle.LogDebug("LoadFromPath complete");
        }

        public void CreateHost()
        {
            _editorHostGame = new EditorHostGame(_game);
        }
        
        public void SetRenderTargetPointers(IntPtr[] sharedResourceHandles, int currentWriteIndex)
        {
            _editorHostGame?.SetSharedResourceHandles(sharedResourceHandles, currentWriteIndex);
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

        public void Render(TimeSpan elapsedGameTime, TimeSpan totalGameTime)
        {
            try
            {
                _editorHostGame?.Render(totalGameTime, elapsedGameTime);
            }
            finally
            {
                FlushLogs();
            }
        }

        public void Update(TimeSpan elapsedGameTime, TimeSpan totalGameTime)
        {
            try
            {
                _editorHostGame?.Update(totalGameTime, elapsedGameTime);
            }
            finally
            {
                FlushLogs();
            }
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

        public void UpdateForLoadContentOnly(TimeSpan elapsedGameTime, TimeSpan totalGameTime)
        {
            if (!(_game?.HasLoadedContent ?? true))
            {
                _editorHostGame?.Update(totalGameTime, elapsedGameTime);
            }
        }

        public void QueueEvent(Event @event)
        {
            _editorEventEngineHook?.QueueEvent(@event);
        }
    }
}
