using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Threading;

namespace Protogame.Editor.GameHost
{
    public class EditorHostGame : IHostGame
    {
        private readonly ICoreGame _coreGame;
        private readonly GameServiceContainer _serviceContainer;
        private readonly EditorGraphicsDeviceService _graphicsDeviceService;
        private readonly EditorGameWindow _editorGameWindow;
        private ContentManager _contentManager;
        private bool _hasLoadedContent = false;
        private int _mouseX;
        private int _mouseY;
        private bool _wantsMouseSet;

        public EditorHostGame(ICoreGame coreGame)
        {
            _coreGame = coreGame;
            _serviceContainer = new GameServiceContainer();

            _graphicsDeviceService = new EditorGraphicsDeviceService();
            _serviceContainer.AddService<IGraphicsDeviceService>(_graphicsDeviceService);
            _editorGameWindow = new EditorGameWindow(this);
            _contentManager = new ContentManager(_serviceContainer, "Content");
        }

        internal void SetMousePositionToSet(int x, int y)
        {
            _mouseX = x;
            _mouseY = y;
            _wantsMouseSet = true;
        }

        public bool GetMousePositionToSet(ref int x, ref int y)
        {
            if (_wantsMouseSet)
            {
                x = _mouseX;
                y = _mouseY;
                _wantsMouseSet = false;
                return true;
            }

            return false;
        }

        public void SetSharedResourceHandles(IntPtr[] sharedResourceHandles, int currentWriteIndex)
        {
            _graphicsDeviceService.UpdateHandles(sharedResourceHandles, currentWriteIndex);

            if (_graphicsDeviceService.RenderTarget != null)
            {
                _editorGameWindow.ClientBounds = _graphicsDeviceService.RenderTarget.Bounds;
            }
        }

        public void Update(TimeSpan totalTimeSpan, TimeSpan elapsedTimeSpan)
        {
            if (!_hasLoadedContent)
            {
                _coreGame.AssignHost(this);
                _coreGame.LoadContent();
                _hasLoadedContent = true;
            }

            _coreGame.Update(new GameTime(totalTimeSpan, elapsedTimeSpan));
        }

        public void Render(TimeSpan totalTimeSpan, TimeSpan elapsedTimeSpan)
        {
            var didPush = false;
            if (_coreGame.RenderContext != null && _coreGame.RenderContext.GraphicsDevice != null)
            {
                _graphicsDeviceService.RenderTarget.AcquireLock(1234, 1000000);

                _coreGame.RenderContext.PushRenderTarget(_graphicsDeviceService.RenderTarget);
                didPush = true;
            }
            
            _coreGame.GraphicsDevice.Clear(Color.Red);

            _coreGame.Draw(new GameTime(totalTimeSpan, elapsedTimeSpan));

            if (didPush)
            {
                _coreGame.RenderContext.PopRenderTarget();
                _coreGame.GraphicsDevice.Flush();
                _coreGame.GraphicsDevice.Metrics = new GraphicsMetrics();

                _graphicsDeviceService.RenderTarget.ReleaseLock(1234);
            }
        }

        #region IHostGame implementation

        public event EventHandler<EventArgs> Exiting;

        public bool IsMouseVisible { get; set; }

        public bool IsActive => true;

        public GraphicsDevice GraphicsDevice => _graphicsDeviceService.GraphicsDevice;

        public GraphicsDeviceManager GraphicsDeviceManager => null;

        public IGameWindow ProtogameWindow => _editorGameWindow;

        public GameWindow Window => null;

        public GameServiceContainer Services => _serviceContainer;

        public ContentManager Content
        {
            get
            {
                return _contentManager;
            }
            set
            {
                _contentManager = value;
            }
        }

        public SpriteBatch SplashScreenSpriteBatch => null;

        public Texture2D SplashScreenTexture => null;

        public void Exit()
        {
        }

        #endregion

    }
}
