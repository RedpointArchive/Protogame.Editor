using Protogame.Editor.ProjectManagement;
using System;

namespace Protogame.Editor.Override
{
    public class GameBaseDirectory : MarshalByRefObject, IBaseDirectory
    {
        private readonly IProjectManager _projectManager;

        public GameBaseDirectory(IProjectManager projectManager)
        {
            _projectManager = projectManager;
        }

        public string FullPath => _projectManager.Project.DefaultGameBinPath.DirectoryName;
    }
}
