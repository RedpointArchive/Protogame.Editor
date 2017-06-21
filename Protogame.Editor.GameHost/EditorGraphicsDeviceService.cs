using Microsoft.Xna.Framework.Graphics;
using Protogame.Editor.CommonHost.SharedRendering;
using System;

namespace Protogame.Editor.GameHost
{
    public class EditorGraphicsDeviceService : IGraphicsDeviceService
    {
        private SharedRendererClient _sharedRendererClient;

        public EditorGraphicsDeviceService(ISharedRendererClientFactory sharedRendererClientFactory)
        {
            _sharedRendererClient = sharedRendererClientFactory.CreateSharedRendererClient();
        }

        public void UpdateHandles(IntPtr[] sharedResourceHandles, string sharedMmapName)
        {
            _sharedRendererClient.SetHandles(sharedResourceHandles, sharedMmapName);

            if (GraphicsDevice == null && _sharedRendererClient.GraphicsDeviceExpected)
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

            if (GraphicsDevice != null && !_sharedRendererClient.GraphicsDeviceExpected)
            {
                DeviceDisposing?.Invoke(this, EventArgs.Empty);

                GraphicsDevice.Dispose();
                GraphicsDevice = null;
            }
            
            _sharedRendererClient.UpdateTextures(GraphicsDevice);
        }

        public void IncrementWritableTextureIfPossible()
        {
            _sharedRendererClient.IncrementWritableTextureIfPossible();
        }

        public RenderTarget2D RenderTarget => _sharedRendererClient.WritableTexture;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
    }
}
