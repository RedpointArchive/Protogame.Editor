using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Protogame.Editor.SharedRendering
{
    public class SharedRendererHost
    {
        private readonly IRenderTargetBackBufferUtilities _renderTargetBackBufferUtilities;

        private readonly MemoryMappedFile _readerWriterMmap;
        private readonly RenderTarget2D[] _renderTargets;
        private readonly IntPtr[] _renderTargetSharedHandles;

        private const int RTBufferSize = 3;
        private readonly MemoryMappedViewAccessor _readerWriterAccessor;
        private bool _mustDestroyRenderTargets;

        public SharedRendererHost(
            IRenderTargetBackBufferUtilities renderTargetBackBufferUtilities)
        {
            _renderTargetBackBufferUtilities = renderTargetBackBufferUtilities;

            SynchronisationMemoryMappedFileName = "ProtogameEditor" + Guid.NewGuid().ToString();
            _readerWriterMmap = MemoryMappedFile.CreateNew(SynchronisationMemoryMappedFileName, 16);
            _readerWriterAccessor = _readerWriterMmap.CreateViewAccessor();

            Size = new Point(640, 480);
            _renderTargets = new RenderTarget2D[RTBufferSize];
            _renderTargetSharedHandles = new IntPtr[RTBufferSize];

            SetReadIndex(0);
            SetWriteIndex(RTBufferSize >= 2 ? 1 : 0);
        }

        public event EventHandler TexturesRecreated;

        public Point Size { get; set; }

        public RenderTarget2D ReadableTexture => _renderTargets[GetReadIndex()];

        public IntPtr[] WritableTextureIntPtrs => _renderTargetSharedHandles;

        public string SynchronisationMemoryMappedFileName { get; }

        public void DestroyTextures()
        {
            _mustDestroyRenderTargets = true;
        }

        public void UpdateTextures(IGameContext gameContext, IRenderContext renderContext)
        {
            if (_mustDestroyRenderTargets)
            {
                for (var i = 0; i < RTBufferSize; i++)
                {
                    if (_renderTargets[i] != null)
                    {
                        _renderTargets[i].Dispose();
                        _renderTargets[i] = null;
                    }
                }

                _mustDestroyRenderTargets = false;
            }

            var didRecreate = false;

            for (var i = 0; i < RTBufferSize; i++)
            {
                var oldRenderTarget = _renderTargets[i];
                _renderTargets[i] = _renderTargetBackBufferUtilities.UpdateCustomSizedRenderTarget(
                    _renderTargets[i],
                    renderContext,
                    Size.ToVector2(),
                    null,
                    null,
                    0, // We must NOT have MSAA on this render target for sharing to work properly!
                    true);
                _renderTargetSharedHandles[i] = _renderTargets[i]?.GetSharedHandle() ?? IntPtr.Zero;
                if (_renderTargets[i] != null && _renderTargets[i] != oldRenderTarget)
                {
                    // Release the lock we will have.
                    _renderTargets[i].ReleaseLock(1234);
                    didRecreate = true;
                }
            }

            if (didRecreate)
            {
                TexturesRecreated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void IncrementReadableTextureIfPossible()
        {
            var nextIndex = GetReadIndex() + 1;
            if (nextIndex == RTBufferSize) { nextIndex = 0; }

            if (nextIndex != GetWriteIndex())
            {
                // Only move the write index if the one we want is not the one
                // we're currently reading from.
                SetReadIndex(nextIndex);
            }
        }

        private int GetReadIndex()
        {
            return _readerWriterAccessor.ReadInt32(0);
        }

        private void SetReadIndex(int index)
        {
            _readerWriterAccessor.Write(0, index);
        }

        private int GetWriteIndex()
        {
            return _readerWriterAccessor.ReadInt32(Marshal.SizeOf(typeof(int)));
        }

        private void SetWriteIndex(int index)
        {
            _readerWriterAccessor.Write(Marshal.SizeOf(typeof(int)), index);
        }
    }
}
