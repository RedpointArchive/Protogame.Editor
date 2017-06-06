using Protogame.Editor.Api.Version1.Menu;
using System.Collections.Generic;

namespace Protogame.Editor.Ext.CodeManager
{
    public class CodeManagerMenuProvider : IMenuProvider
    {
        private readonly ICodeManagerService _codeManagerService;

        public CodeManagerMenuProvider(ICodeManagerService codeManagerService)
        {
            _codeManagerService = codeManagerService;
        }

        public IEnumerable<MenuEntry> GetMenuItems()
        {
            yield return new MenuEntry("Project/Start Visual Studio...", true, 50, MenuOpenCSharpProject, null);
            yield return new MenuEntry("Project/C# Project", true, 110, null, null);
            yield return new MenuEntry("Project/C# Project/Build", !_codeManagerService.IsProcessRunning, 50, MenuBuildCSharpProject, null);
            yield return new MenuEntry("Project/C# Project/Resynchronise", !_codeManagerService.IsProcessRunning, 100, MenuResyncCSharpProject, null);
            yield return new MenuEntry("Project/C# Project/Synchronise", !_codeManagerService.IsProcessRunning, 105, MenuSyncCSharpProject, null);
            yield return new MenuEntry("Project/C# Project/Generate", !_codeManagerService.IsProcessRunning, 110, MenuGenerateCSharpProject, null);
            yield return new MenuEntry("Project/C# Project/Upgrade All Packages", !_codeManagerService.IsProcessRunning, 200, MenuUpgradeAllPackages, null);
        }

        private void MenuUpgradeAllPackages(MenuEntry menuEntry)
        {
            _codeManagerService.UpgradeAllPackages();
        }

        private void MenuBuildCSharpProject(MenuEntry menuEntry)
        {
            _codeManagerService.BuildCSharpProject();
        }

        private void MenuResyncCSharpProject(MenuEntry menuEntry)
        {
            _codeManagerService.ResyncCSharpProject();
        }

        private void MenuSyncCSharpProject(MenuEntry menuEntry)
        {
            _codeManagerService.SyncCSharpProject();
        }

        private void MenuGenerateCSharpProject(MenuEntry menuEntry)
        {
            _codeManagerService.GenerateCSharpProject();
        }

        private void MenuOpenCSharpProject(MenuEntry menuEntry)
        {
            _codeManagerService.OpenCSharpProject();
        }
    }
}
