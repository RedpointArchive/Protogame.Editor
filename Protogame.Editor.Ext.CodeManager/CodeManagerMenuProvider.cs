using Protogame.Editor.Api.Version1.Menu;
using Protogame.Editor.Api.Version1.ProjectManagement;
using System;
using System.Collections.Generic;

namespace Protogame.Editor.Ext.CodeManager
{
    public class CodeManagerMenuProvider : MarshalByRefObject, IMenuProvider
    {
        private readonly ICodeManagerService _codeManagerService;
        private readonly IProjectManager _projectManager;

        public CodeManagerMenuProvider(
            IProjectManager projectManager,
            ICodeManagerService codeManagerService)
        {
            _projectManager = projectManager;
            _codeManagerService = codeManagerService;
        }

        public MenuEntry[] GetMenuItems()
        {
            var projectExists = _projectManager.Project != null;

            return new[]
            {
                new MenuEntry("Project/Start Visual Studio...", projectExists, 50, MenuOpenCSharpProject, null),
                new MenuEntry("Project/C# Project", projectExists, 110, null, null),
                new MenuEntry("Project/C# Project/Build", projectExists && !_codeManagerService.IsProcessRunning, 50, MenuBuildCSharpProject, null),
                new MenuEntry("Project/C# Project/Resynchronise", projectExists && !_codeManagerService.IsProcessRunning, 100, MenuResyncCSharpProject, null),
                new MenuEntry("Project/C# Project/Synchronise", projectExists && !_codeManagerService.IsProcessRunning, 105, MenuSyncCSharpProject, null),
                new MenuEntry("Project/C# Project/Generate", projectExists && !_codeManagerService.IsProcessRunning, 110, MenuGenerateCSharpProject, null),
                new MenuEntry("Project/C# Project/Upgrade All Packages", projectExists && !_codeManagerService.IsProcessRunning, 200, MenuUpgradeAllPackages, null),
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
