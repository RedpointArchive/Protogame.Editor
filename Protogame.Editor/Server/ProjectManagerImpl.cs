using System.Threading.Tasks;
using Grpc.Core;
using Protogame.Editor.Grpc.Editor;
using Protogame.Editor.ProjectManagement;

namespace Protogame.Editor.Server
{
    public class ProjectManagerImpl : Grpc.Editor.ProjectManager.ProjectManagerBase
    {
        private readonly IProjectManager _projectManager;

        public ProjectManagerImpl(IProjectManager projectManager)
        {
            _projectManager = projectManager;
        }

        public override async Task<GetProjectResponse> GetProject(GetProjectRequest request, ServerCallContext context)
        {
            var resp = new GetProjectResponse();

            if (_projectManager.Project == null)
            {
                resp.HasProject = false;
                resp.Project = null;
                return resp;
            }

            resp.HasProject = true;
            resp.Project = new Grpc.Editor.Project();

            resp.Project.ProjectPath = _projectManager.Project.ProjectPath.FullName;
            resp.Project.LoadingStatus = _projectManager.Project.LoadingStatus;
            resp.Project.Name = _projectManager.Project.Name;
            resp.Project.DefaultGameDefinitionName = _projectManager.Project.DefaultGame.Name;
            resp.Project.SolutionFilePath = _projectManager.Project.SolutionFile.FullName;
            resp.Project.DefaultGameBinPath = _projectManager.Project.DefaultGameBinPath.FullName;
            foreach (var package in _projectManager.Project.Packages)
            {
                resp.Project.Packages.Add(new Package
                {
                    Package_ = package.Package,
                    Repository = package.Repository,
                    Version = package.Version
                });
            }
            foreach (var definition in _projectManager.Project.Definitions)
            {
                resp.Project.Definitions.Add(new Definition
                {
                    Name = definition.Name,
                    Type = definition.Type,
                    Role = definition.Role,
                    XmlDocumentPath = definition.LoadedDocumentPath
                });
            }

            return resp;
        }

        public override async Task<GetScannedContentPathsResponse> GetScannedContentPaths(GetScannedContentPathsRequest request, ServerCallContext context)
        {
            var resp = new GetScannedContentPathsResponse();

            return resp;
        }
    }
}
