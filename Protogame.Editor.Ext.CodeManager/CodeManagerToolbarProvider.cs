using Protogame.Editor.Api.Version1.ProjectManagement;
using Protogame.Editor.Api.Version1.Toolbar;

namespace Protogame.Editor.Ext.CodeManager
{
    public class CodeManagerToolbarProvider : IToolbarProvider
    {
        private readonly ICodeManagerService _codeManagerService;
        private readonly IProjectManager _projectManager;

        public CodeManagerToolbarProvider(
            IProjectManager projectManager,
            ICodeManagerService codeManagerService)
        {
            _projectManager = projectManager;
            _codeManagerService = codeManagerService;
        }

        public GenericToolbarEntry[] GetToolbarItems()
        {
            return new GenericToolbarEntry[]
            {
                new GenericToolbarEntry("vs", "texture.IconVisualStudio", false, _projectManager.Project != null, LaunchVisualStudio, null),
            };
        }

        private void LaunchVisualStudio(GenericToolbarEntry toolbarEntry)
        {
            _codeManagerService.OpenCSharpProject();
        }
    }
}
