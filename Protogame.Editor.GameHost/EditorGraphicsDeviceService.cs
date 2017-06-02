using Microsoft.Xna.Framework.Graphics;
using System;

namespace Protogame.Editor.GameHost
{
    public class EditorGraphicsDeviceService : IGraphicsDeviceService
    {
        private IntPtr[] _sharedResourceHandles;
        private RenderTarget2D[] _renderTargets;
        private int _currentWriteIndex;

        public EditorGraphicsDeviceService()
        {
            _sharedResourceHandles = new IntPtr[3];
            _renderTargets = new RenderTarget2D[3];
            _currentWriteIndex = 0;
        }

        public void UpdateHandles(IntPtr[] sharedResourceHandles, int currentWriteIndex)
        {
            _currentWriteIndex = currentWriteIndex;

            if (GraphicsDevice == null && sharedResourceHandles[_currentWriteIndex] != IntPtr.Zero)
            {
                // We have been requested to create a graphics device.
                var parameters = new PresentationParameters();

                parameters.BackBufferWidth = 1;
                parameters.BackBufferHeight = 1;
                parameters.BackBufferFormat = SurfaceFormat.Color;
                parameters.DepthStencilFormat = DepthFormat.Depth24;
                parameters.DeviceWindowHandle = IntPtr.Zero;
                parameters.PresentationInterval = PresentInterval.Immediate;
                parameters.IsFullScreen = false;

                GraphicsDevice = new GraphicsDevice(
                    GraphicsAdapter.DefaultAdapter,
                    GraphicsProfile.HiDef,
                    parameters);

                DeviceCreated?.Invoke(this, EventArgs.Empty);
            }

            if (GraphicsDevice != null && sharedResourceHandles[_currentWriteIndex] == IntPtr.Zero)
            {
                DeviceDisposing?.Invoke(this, EventArgs.Empty);

                GraphicsDevice.Dispose();
                GraphicsDevice = null;
            }

            if (_sharedResourceHandles[_currentWriteIndex] != sharedResourceHandles[_currentWriteIndex])
            {
                for (var i = 0; i < 3; i++)
                {
                    _renderTargets[i]?.Dispose();
                    _renderTargets[i] = null;
                }
                
                for (var i = 0; i < 3; i++)
                {
                    if (GraphicsDevice != null)
                    {
                        _renderTargets[i] = RenderTarget2D.FromSharedResourceHandle(
                            GraphicsDevice,
                            sharedResourceHandles[i]);
                    }
                }

                _sharedResourceHandles = sharedResourceHandles;
            }
        }

        public RenderTarget2D RenderTarget => _renderTargets[_currentWriteIndex];

        public GraphicsDevice GraphicsDevice { get; private set; }

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
    }
}
