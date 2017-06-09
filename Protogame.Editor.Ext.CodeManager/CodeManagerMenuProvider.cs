using Protogame.Editor.Api.Version1.Menu;
using System;
using System.Collections.Generic;

namespace Protogame.Editor.Ext.CodeManager
{
    public class CodeManagerMenuProvider : MarshalByRefObject, IMenuProvider
    {
        private readonly ICodeManagerService _codeManagerService;

        public CodeManagerMenuProvider(ICodeManagerService codeManagerService)
        {
            _codeManagerService = codeManagerService;
        }

        public MenuEntry[] GetMenuItems()
        {
            return new[]
            {
                new MenuEntry("Project/Start Visual Studio...", true, 50, MenuOpenCSharpProject, null),
                new MenuEntry("Project/C# Project", true, 110, null, null),
                new MenuEntry("Project/C# Project/Build", !_codeManagerService.IsProcessRunning, 50, MenuBuildCSharpProject, null),
                new MenuEntry("Project/C# Project/Resynchronise", !_codeManagerService.IsProcessRunning, 100, MenuResyncCSharpProject, null),
                new MenuEntry("Project/C# Project/Synchronise", !_codeManagerService.IsProcessRunning, 105, MenuSyncCSharpProject, null),
                new MenuEntry("Project/C# Project/Generate", !_codeManagerService.IsProcessRunning, 110, MenuGenerateCSharpProject, null),
                new MenuEntry("Project/C# Project/Upgrade All Packages", !_codeManagerService.IsProcessRunning, 200, MenuUpgradeAllPackages, null),
            };
        }

        public void MenuUpgradeAllPackages(MenuEntry menuEntry)
        {
            _codeManagerService.UpgradeAllPackages();
        }

        public void MenuBuildCSharpProject(MenuEntry menuEntry)
        {
            _codeManagerService.BuildCSharpProject();
        }

        public void MenuResyncCSharpProject(MenuEntry menuEntry)
        {
            _codeManagerService.ResyncCSharpProject();
        }

        public void MenuSyncCSharpProject(MenuEntry menuEntry)
        {
            _codeManagerService.SyncCSharpProject();
        }

        public void MenuGenerateCSharpProject(MenuEntry menuEntry)
        {
            _codeManagerService.GenerateCSharpProject();
        }

        public void MenuOpenCSharpProject(MenuEntry menuEntry)
        {
            _codeManagerService.OpenCSharpProject();
        }
    }
}
