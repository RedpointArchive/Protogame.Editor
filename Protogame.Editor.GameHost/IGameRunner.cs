using System;

namespace Protogame.Editor.GameHost
{
    public interface IGameRunner
    {
        void Run();

        void SetMousePositionToGet(int x, int y);

        bool GetMousePositionToSet(ref int x, ref int y);

        void SetHandles(IntPtr[] sharedTextures, string sharedMmapName);

        void SetPlaybackMode(bool playing);
    }
}
