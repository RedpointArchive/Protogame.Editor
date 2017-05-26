using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtogameUIStylingTest
{
    public class ProjectManager : IMenuProvider
    {
        public IEnumerable<MenuEntry> GetMenuItems()
        {
            yield return new MenuEntry("File/New Project...", true, 0, OnNewProject, null);
            yield return new MenuEntry("File/Open Project...", true, 1, OnOpenProject, null);
            yield return new MenuEntry("File/Save Project", true, 2, OnSaveProject, null);
            yield return new MenuEntry("File/Exit", true, 400, OnExitApplication, null);
        }

        private void OnNewProject(MenuEntry obj)
        {
        }

        private void OnSaveProject(MenuEntry obj)
        {
        }

        private void OnOpenProject(MenuEntry obj)
        {
        }

        private void OnExitApplication(MenuEntry obj)
        {
        }
    }
}
