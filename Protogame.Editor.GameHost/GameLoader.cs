using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Protoinject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Protogame.Editor.GameHost
{
    public class GameLoader : MarshalByRefObject, IDisposable
    {
        private ICoreGame _game;
        private DomainGate _domainGate;
        private Dictionary<string, object> _crossDomainData;
        private GraphicsDevice _graphicsDevice;

        internal void AssignCrossDomainDataStorage(Dictionary<string, object> crossDomainDictionary)
        {
            _crossDomainData = crossDomainDictionary;
        }

        public void LoadFromPath(
            IConsoleHandle consoleHandle,
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
                kernel.Rebind<IBaseDirectory>().ToMethod(x => (IBaseDirectory)_crossDomainData["BaseDirectory"]).InSingletonScope();
                kernel.Rebind<IBackBufferDimensions>().ToMethod(x => (IBackBufferDimensions)_crossDomainData["BackBufferDimensions"]).InSingletonScope();

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

        public DomainGate CreateDomainGate()
        {
            return _domainGate = new DomainGate(this);
        }

        public void CreateHostProxyFromCrossDomainDictionary()
        {
            _graphicsDevice = (GraphicsDevice)_crossDomainData["GraphicsDevice"];

            _game?.AssignHost(new HostGameProxy(
                _graphicsDevice,
                (GraphicsDeviceManager)_crossDomainData["GraphicsDeviceManager"],
                (IGameWindow)_crossDomainData["ProtogameWindow"],
                (GameWindow)_crossDomainData["Window"],
                (GameServiceContainer)_crossDomainData["Services"],
                (ContentManager)_crossDomainData["ContentManager"]));

            _game?.LoadContent();
            _game?.EnableImmediateStartFromHost();
        }

        public void Dispose()
        {
            _crossDomainData = null;
        }

        public void Render(TimeSpan elapsedGameTime, TimeSpan totalGameTime)
        {
            if (_game == null || _game.RenderContext == null || _graphicsDevice == null)
            {
                return;
            }

            // We have to tell the game's RenderContext about the render targets
            // that it will be rendering to.  That way, the stack won't get uncorrectly
            // set to the backbuffer when all targets are popped.
            var didPush = false;
            if (_game.RenderContext.GraphicsDevice != null)
            {
                _game.RenderContext.PushRenderTarget(_graphicsDevice.GetRenderTargets());
                didPush = true;
            }

            _game.Draw(new GameTime(totalGameTime, elapsedGameTime));

            if (didPush)
            {
                _game.RenderContext.PopRenderTarget();
            }
        }

        public void Update(TimeSpan elapsedGameTime, TimeSpan totalGameTime)
        {
            _game?.Update(new GameTime(totalGameTime, elapsedGameTime));
        }

        public void UpdateForLoadContentOnly(TimeSpan elapsedGameTime, TimeSpan totalGameTime)
        {
            if (!(_game?.HasLoadedContent ?? true))
            {
                _game?.Update(new GameTime(totalGameTime, elapsedGameTime));
            }
        }
    }
}
