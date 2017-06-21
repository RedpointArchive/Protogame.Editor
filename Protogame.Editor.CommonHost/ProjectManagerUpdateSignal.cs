using System;
using Protogame.Editor.Api.Version1.Core;
using Protogame.Editor.Api.Version1.ProjectManagement;

namespace Protogame.Editor.CommonHost
{
    public class ProjectManagerUpdateSignal : IWantsUpdateSignal
    {
        private readonly ProjectManager _projectManager;

        public ProjectManagerUpdateSignal(IProjectManager projectManager)
        {
            _projectManager = (ProjectManager)projectManager;
        }

        public void Update()
        {
            _projectManager.Update();
        }
    }
}