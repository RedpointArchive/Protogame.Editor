namespace Protogame.Editor.Api.Version1.ProjectManagement
{
    public interface IProjectManager
    {
        void LoadProject(string directoryPath);

        IProject Project { get; }
    }
}
