using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Protogame.Editor.GameHost
{
    public class HostGameProxy : MarshalByRefObject, IHostGame
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly IGameWindow _protogameWindow;
        private readonly GameWindow _window;
        private readonly GameServiceContainer _services;
        private readonly ContentManager _contentManager;

        public HostGameProxy(
            GraphicsDevice graphicsDevice,
            GraphicsDeviceManager graphicsDeviceManager,
            IGameWindow protogameWindow,
            GameWindow window,
            GameServiceContainer services,
            ContentManager contentManager)
        {
            _graphicsDevice = graphicsDevice;
            _graphicsDeviceManager = graphicsDeviceManager;
            _protogameWindow = protogameWindow;
            _window = window;
            _services = services;
            _contentManager = contentManager;
        }

        public bool IsMouseVisible { get; set; }

        public GraphicsDevice GraphicsDevice => _graphicsDevice;

        public GraphicsDeviceManager GraphicsDeviceManager => _graphicsDeviceManager;

        public IGameWindow ProtogameWindow => _protogameWindow;

        public GameWindow Window => _window;

        public GameServiceContainer Services => _services;

        public ContentManager Content
        {
            get
            {
                return _contentManager;
            }
            set
            {

            }
        }

        public SpriteBatch SplashScreenSpriteBatch { get; set; }

        public Texture2D SplashScreenTexture { get; set; }

        public event EventHandler<EventArgs> Exiting;

        public void Exit()
        {
        }
    }
}
