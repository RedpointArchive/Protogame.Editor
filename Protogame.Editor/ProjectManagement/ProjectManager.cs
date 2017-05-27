using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Protogame.Editor.ProjectManagement
{
    public class ProjectManager : IProjectManager
    {
        private readonly ICoroutine _coroutine;
        private Task _loadingTask;
        private Project _project;

        public ProjectManager(
            IRawLaunchArguments launchArguments,
            ICoroutine coroutine)
        {
            _coroutine = coroutine;

            var arguments = launchArguments.Arguments;
            var directoryIndex = Array.IndexOf(arguments, "--project");
            if (directoryIndex == -1 || directoryIndex == arguments.Length - 1)
            {
                throw new InvalidOperationException("No project defined for startup!  You must launch the Protogame Editor from the workbench, or pass --project <dir> on the command-line.");
            }

            LoadProject(arguments[directoryIndex + 1]);
        }

        public IProject Project => _project;

        private void LoadProject(string directoryPath)
        {
            _project = new Project
            {
                ProjectPath = new DirectoryInfo(directoryPath)
            };

            _loadingTask = _coroutine.Run(async () => await LoadProjectDataAsync(_project));
        }

        private async Task LoadProjectDataAsync(Project project)
        {
            var protobuildFile = new FileInfo(Path.Combine(project.ProjectPath.FullName, "Protobuild.exe"));
            var moduleInfoFile = new FileInfo(Path.Combine(project.ProjectPath.FullName, "Build", "Module.xml"));
            var projectsDefinitionsDirectory = new DirectoryInfo(Path.Combine(project.ProjectPath.FullName, "Build", "Projects"));

            project.LoadingStatus = "Loading project...";

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(moduleInfoFile.FullName);

            project.Name = xmlDocument.SelectSingleNode("/Module/Name").InnerText;
            var packages = xmlDocument.SelectNodes("/Module/Packages/Package");
            var packagesList = new List<PackageInfo>();

            await Task.Yield();

            project.LoadingStatus = "Loading package list...";

            foreach (var package in packages.OfType<XmlElement>())
            {
                packagesList.Add(new PackageInfo
                {
                    Repository = package.GetAttribute("Repository"),
                    Package = package.GetAttribute("Package"),
                    Version = package.GetAttribute("Version"),
                });
            }

            project.Packages = packagesList;

            await Task.Yield();

            project.LoadingStatus = "Loading definitions...";

            var definitionsList = new List<DefinitionInfo>();
            foreach (var definitionFile in projectsDefinitionsDirectory.GetFiles("*.definition"))
            {
                var definitionXmlDocument = new XmlDocument();
                definitionXmlDocument.Load(definitionFile.FullName);

                var definitionInfo = new DefinitionInfo
                {
                    Name = definitionXmlDocument.DocumentElement.GetAttribute("Name"),
                    Path = definitionXmlDocument.DocumentElement.GetAttribute("Path"),
                    Type = definitionXmlDocument.DocumentElement.GetAttribute("Type"),
                    LoadedDocument = definitionXmlDocument
                };
                definitionsList.Add(definitionInfo);
            }

            project.Definitions = definitionsList;

            project.LoadingStatus = null;
        }
    }
}
