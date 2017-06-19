using Protogame.Editor.Api.Version1.ProjectManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using static Protogame.Editor.Grpc.Editor.ProjectManager;
using Protogame.Editor.Grpc.Editor;
using System.Linq;

namespace Protogame.Editor.CommonHost
{
    public class ProjectManager : IProjectManager
    {
        private readonly IEditorClientProvider _editorClientProvider;
        private IProject _cachedProject;
        private int _cachedHashCode;
        private Task _projectUpdateTask;

        public ProjectManager(IEditorClientProvider editorClientProvider)
        {
            _editorClientProvider = editorClientProvider;
        }

        public IProject Project => _cachedProject;

        public void Update()
        {
            if (_projectUpdateTask == null || _projectUpdateTask.IsCompleted)
            {
                _projectUpdateTask = Task.Run(async () =>
                {
                    var client = _editorClientProvider.GetClient<ProjectManagerClient>();
                    var project = await client.GetProjectAsync(new Grpc.Editor.GetProjectRequest(), deadline: DateTime.UtcNow.AddSeconds(1));
                    if (project.HasProject)
                    {
                        ProjectImpl projectInstance;
                        if (_cachedProject == null || project.Project.HashCode != _cachedHashCode)
                        {
                            projectInstance = new ProjectImpl();
                        }
                        else
                        {
                            projectInstance = (ProjectImpl)_cachedProject;
                        }
                        projectInstance.ProjectPath = new DirectoryInfo(project.Project.ProjectPath);
                        projectInstance.Name = project.Project.Name;
                        projectInstance.LoadingStatus = project.Project.LoadingStatus;
                        var packages = new List<IPackageInfo>();
                        foreach (var pkg in project.Project.Packages)
                        {
                            packages.Add(new PackageInfo
                            {
                                Repository = pkg.Repository,
                                Package = pkg.Package_,
                                Version = pkg.Version
                            });
                        }
                        projectInstance.Packages = packages.AsReadOnly();
                        var definitions = new List<IDefinitionInfo>();
                        foreach (var def in project.Project.Definitions)
                        {
                            definitions.Add(new DefinitionInfo(def.XmlDocumentPath, client)
                            {
                                Name = def.Name,
                                Type = def.Type,
                                Role = def.Role,
                            });
                        }
                        projectInstance.Definitions = definitions.AsReadOnly();
                        projectInstance.DefaultGame = projectInstance.Definitions.FirstOrDefault(x => x.Name == project.Project.DefaultGameDefinitionName);
                        projectInstance.SolutionFile = new FileInfo(project.Project.SolutionFilePath);
                        projectInstance.DefaultGameBinPath = new FileInfo(project.Project.DefaultGameBinPath);
                        _cachedProject = projectInstance;
                        _cachedHashCode = project.Project.HashCode;
                    }
                    else
                    {
                        _cachedProject = null;
                    }

                    await Task.Delay(3000);
                });
            }
        }

        private class ProjectImpl : IProject
        {
            public DirectoryInfo ProjectPath { get; set; }

            public string LoadingStatus { get; set; }

            public string Name { get; set; }

            public ReadOnlyCollection<IPackageInfo> Packages { get; set; }

            public ReadOnlyCollection<IDefinitionInfo> Definitions { get; set; }

            public IDefinitionInfo DefaultGame { get; set; }

            public FileInfo SolutionFile { get; set; }

            public FileInfo DefaultGameBinPath { get; set; }
        }

        private class PackageInfo : IPackageInfo
        {
            public string Repository { get; set; }

            public string Package { get; set; }

            public string Version { get; set; }
        }

        private class DefinitionInfo : IDefinitionInfo
        {
            private XmlDocument _loadedDocument;
            private string _xmlDocumentPath;
            private ProjectManagerClient _projectManagerClient;
            private List<FileInfo> _scannedContent;

            public DefinitionInfo(string xmlDocumentPath, ProjectManagerClient projectManagerClient)
            {
                _xmlDocumentPath = xmlDocumentPath;
                _projectManagerClient = projectManagerClient;
            }

            public string Name { get; set; }

            public string Type { get; set; }

            public string Role { get; set; }

            public XmlDocument LoadedDocument
            {
                get
                {
                    if (_loadedDocument != null)
                    {
                        return _loadedDocument;
                    }

                    _loadedDocument = new XmlDocument();
                    _loadedDocument.Load(_xmlDocumentPath);
                    return _loadedDocument;
                }
            }

            public List<FileInfo> ScannedContent
            {
                get
                {
                    if (_scannedContent != null)
                    {
                        return _scannedContent;
                    }

                    _scannedContent = _projectManagerClient.GetScannedContentPaths(new GetScannedContentPathsRequest()).FilePath.Select(x => new FileInfo(x)).ToList();
                    return _scannedContent;
                }
            }
        }
    }
}
