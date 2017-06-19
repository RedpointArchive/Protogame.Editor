namespace Protogame.Editor.GameHost
{
    public interface IGameRunner
    {
        void Run();

        void SetMousePositionToGet(int x, int y);

        bool GetMousePositionToSet(ref int x, ref int y);
    }
}
