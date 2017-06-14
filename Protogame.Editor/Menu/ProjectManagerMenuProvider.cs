using Protogame.Editor.ProjectManagement;

namespace Protogame.Editor.Menu
{
    public class ProjectManagerMenuProvider : IMenuProvider
    {
        private readonly IProjectManager _projectManager;
        private readonly IProjectManagerUi _projectManagerUi;
        private readonly ICoroutine _coroutine;

        public ProjectManagerMenuProvider(
            IProjectManager projectManager,
            IProjectManagerUi projectManagerUi,
            ICoroutine coroutine)
        {
            _projectManager = projectManager;
            _projectManagerUi = projectManagerUi;
            _coroutine = coroutine;
        }

        public MenuEntry[] GetMenuItems()
        {
            return new[]
            {
                new MenuEntry("File/New Project...", true, 0, OnNewProject, null),
                new MenuEntry("File/Open Project...", true, 1, OnOpenProject, null),
                new MenuEntry("File/Save Project", true, 2, OnSaveProject, null),
                new MenuEntry("File/Exit", true, 400, OnExitApplication, null),
            };
        }

        private void OnNewProject(MenuEntry obj)
        {
        }

        private void OnSaveProject(MenuEntry obj)
        {
        }

        private void OnOpenProject(MenuEntry obj)
        {
            _coroutine.Run(() => _projectManagerUi.LoadProject());
        }

        private void OnExitApplication(MenuEntry obj)
        {
            //gameContext.Game.Exit();
        }
    }
}