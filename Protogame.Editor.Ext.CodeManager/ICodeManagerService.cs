namespace Protogame.Editor.Ext.CodeManager
{
    public interface ICodeManagerService
    {
        bool IsProcessRunning { get; }

        void Update();

        void BuildCSharpProject();
        void AutoCSharpProject();
        void GenerateCSharpProject();
        void ResyncCSharpProject();
        void SyncCSharpProject();
        void UpgradeAllPackages();
        void OpenCSharpProject();
    }
}