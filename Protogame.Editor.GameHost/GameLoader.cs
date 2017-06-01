using Protoinject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Protogame.Editor.GameHost
{
    public class GameLoader : MarshalByRefObject//, IDisposable
    {
        private ICoreGame _game;
        private IntPtr _sharedResourceHandle;
        private EditorHostGame _editorHostGame;

        public void LoadFromPath(
            IConsoleHandle consoleHandle,
            IBaseDirectory baseDirectory,
            IBackBufferDimensions backBufferDimensions,
            string gameAssembly)
        {
            // Wrap the backbuffer dimensions service in a proxy, since GraphicsDevice can not
            // cross the AppDomain boundary.
            backBufferDimensions = new BackBufferDimensionsProxy(backBufferDimensions);

            // Load the target assembly.
            consoleHandle.LogDebug("Loading game assembly from " + gameAssembly + "...");
            var assembly = Assembly.LoadFrom(gameAssembly);

            consoleHandle.LogDebug("Constructing standard kernel...");
            var kernel = new StandardKernel();
            kernel.Bind<IRawLaunchArguments>()
                .ToMethod(x => new DefaultRawLaunchArguments(new string[0]))
                .InSingletonScope();

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

            consoleHandle.LogDebug("Configuring kernel and constructing game instance ({0} configurations)...", gameConfigurations.Count);
            foreach (var configuration in gameConfigurations)
            {
                consoleHandle.LogDebug("Configuring with {0}...", configuration.GetType().FullName);

                configuration.ConfigureKernel(kernel);

                // Rebind services so the game renders correctly inside the editor.
                kernel.Rebind<IBaseDirectory>().ToMethod(x => baseDirectory).InSingletonScope();
                kernel.Rebind<IBackBufferDimensions>().ToMethod(x => backBufferDimensions).InSingletonScope();

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

            consoleHandle.LogDebug("LoadFromPath complete");
        }

        public void CreateHost()
        {
            _editorHostGame = new EditorHostGame(_game);
        }
        
        public void SetRenderTargetPointer(IntPtr sharedResourceHandle)
        {
            _editorHostGame?.SetSharedResourceHandle(sharedResourceHandle);
        }

        public void Render(TimeSpan elapsedGameTime, TimeSpan totalGameTime)
        {
            _editorHostGame?.Render(totalGameTime, elapsedGameTime);
        }

        public void Update(TimeSpan elapsedGameTime, TimeSpan totalGameTime)
        {
            _editorHostGame?.Update(totalGameTime, elapsedGameTime);
        }

        public void UpdateForLoadContentOnly(TimeSpan elapsedGameTime, TimeSpan totalGameTime)
        {
            if (!(_game?.HasLoadedContent ?? true))
            {
                _editorHostGame?.Update(totalGameTime, elapsedGameTime);
            }
        }
    }
}
