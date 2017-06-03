namespace Protogame.Editor.ProjectManagement
{
    public interface IProjectManager
    {
        void LoadProject(string directoryPath);

        IProject Project { get; }
    }
}
