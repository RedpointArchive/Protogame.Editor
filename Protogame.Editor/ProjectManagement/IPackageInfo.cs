namespace Protogame.Editor.ProjectManagement
{
    public interface IPackageInfo
    {
        string Repository { get; }

        string Package { get; }

        string Version { get; }
    }
}
