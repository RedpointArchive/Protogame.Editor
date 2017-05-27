using Protogame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtogameUIStylingTest
{
    public class ProjectManagerMenuProvider : IMenuProvider
    {
        private readonly IProjectManager _projectManager;

        public ProjectManagerMenuProvider(IProjectManager projectManager)
        {
            _projectManager = projectManager;
        }

        public IEnumerable<MenuEntry> GetMenuItems()
        {
            yield return new MenuEntry("File/New Project...", true, 0, OnNewProject, null);
            yield return new MenuEntry("File/Open Project...", true, 1, OnOpenProject, null);
            yield return new MenuEntry("File/Save Project", true, 2, OnSaveProject, null);
            yield return new MenuEntry("File/Exit", true, 400, OnExitApplication, null);
        }

        private void OnNewProject(IGameContext gameContext, MenuEntry obj)
        {
            
        }

        private void OnSaveProject(IGameContext gameContext, MenuEntry obj)
        {
        }

        private void OnOpenProject(IGameContext gameContext, MenuEntry obj)
        {
        }

        private void OnExitApplication(IGameContext gameContext, MenuEntry obj)
        {
            gameContext.Game.Exit();
        }
    }
}
