using Microsoft.Xna.Framework.Graphics;
using System;

namespace Protogame.Editor.GameHost
{
    public class EditorGraphicsDeviceService : IGraphicsDeviceService
    {
        private IntPtr _sharedResourceHandle;

        public EditorGraphicsDeviceService()
        {
            _sharedResourceHandle = IntPtr.Zero;
        }

        public void UpdateHandle(IntPtr sharedResourceHandle)
        {
            if (_sharedResourceHandle != sharedResourceHandle)
            {
                if (RenderTarget != null)
                {
                    RenderTarget.Dispose();
                    RenderTarget = null;
                }

                if (sharedResourceHandle == IntPtr.Zero)
                {
                    // We have been requested to dispose our graphics device.
                    if (GraphicsDevice != null)
                    {
                        DeviceDisposing?.Invoke(this, EventArgs.Empty);

                        GraphicsDevice.Dispose();
                        GraphicsDevice = null;
                    }
                }

                if (sharedResourceHandle != IntPtr.Zero && _sharedResourceHandle == IntPtr.Zero)
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

                RenderTarget = RenderTarget2D.FromSharedResourceHandle(
                    GraphicsDevice,
                    sharedResourceHandle);

                _sharedResourceHandle = sharedResourceHandle;
            }
        }

        public RenderTarget2D RenderTarget { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
    }
}
