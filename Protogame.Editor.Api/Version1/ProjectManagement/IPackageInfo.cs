namespace Protogame.Editor.Api.Version1.ProjectManagement
{
    public interface IPackageInfo
    {
        string Repository { get; }

        string Package { get; }

        string Version { get; }
    }
}
