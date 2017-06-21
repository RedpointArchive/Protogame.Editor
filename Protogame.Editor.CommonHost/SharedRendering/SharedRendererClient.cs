using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Protogame.Editor.CommonHost.SharedRendering
{
    public class SharedRendererClient
    {
        private string _sharedMmapName;
        private IntPtr[] _sharedTexturePointers;
        private RenderTarget2D[] _sharedTextures;
        private MemoryMappedFile _readerWriterMmap;
        private MemoryMappedViewAccessor _readerWriterAccessor;
        private bool _mustRecreateTextures;

        public SharedRendererClient()
        {
        }

        public bool GraphicsDeviceExpected => _sharedTexturePointers != null && _sharedTexturePointers.Length > 0;

        public RenderTarget2D WritableTexture => _sharedTextures?[GetWriteIndex()];

        public void SetHandles(IntPtr[] sharedTextures, string sharedMmapName)
        {
            if (_sharedMmapName != sharedMmapName)
            {
                if (_readerWriterMmap != null)
                {
                    _readerWriterAccessor.Dispose();
                    _readerWriterMmap.Dispose();
                }

                _sharedMmapName = sharedMmapName;

                if (_sharedMmapName != null)
                {
                    _readerWriterMmap = MemoryMappedFile.OpenExisting(sharedMmapName);
                    _readerWriterAccessor = _readerWriterMmap.CreateViewAccessor();
                }
            }

            _sharedTexturePointers = sharedTextures;
            _mustRecreateTextures = true;
        }

        public void UpdateTextures(GraphicsDevice graphicsDevice)
        {
            if (_mustRecreateTextures)
            {
                if (_sharedTextures != null)
                {
                    for (var i = 0; i < _sharedTextures.Length; i++)
                    {
                        _sharedTextures[i].Dispose();
                    }

                    if (_sharedTexturePointers != null)
                    {
                        _sharedTextures = new RenderTarget2D[_sharedTexturePointers.Length];
                    }
                    else
                    {
                        _sharedTextures = null;
                    }

                    if (_sharedTextures != null)
                    {
                        for (var i = 0; i < _sharedTexturePointers.Length; i++)
                        {
                            _sharedTextures[i] = RenderTarget2D.FromSharedResourceHandle(
                                graphicsDevice,
                                _sharedTexturePointers[i]);
                        }
                    }
                }
            }
        }

        public void IncrementWritableTextureIfPossible()
        {
            var nextIndex = GetWriteIndex() + 1;
            if (nextIndex == _sharedTextures.Length) { nextIndex = 0; }

            if (nextIndex != GetReadIndex())
            {
                // Only move the write index if the one we want is not the one
                // we're currently reading from.
                SetWriteIndex(nextIndex);
            }
        }

        private int GetReadIndex()
        {
            return _readerWriterAccessor?.ReadInt32(0) ?? 0;
        }

        private void SetReadIndex(int index)
        {
            _readerWriterAccessor?.Write(0, index);
        }

        private int GetWriteIndex()
        {
            return _readerWriterAccessor?.ReadInt32(Marshal.SizeOf(typeof(int))) ?? 0;
        }

        private void SetWriteIndex(int index)
        {
            _readerWriterAccessor?.Write(Marshal.SizeOf(typeof(int)), index);
        }
    }
}
