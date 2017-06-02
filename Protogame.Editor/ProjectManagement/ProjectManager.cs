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
        private readonly IConsoleHandle _consoleHandle;
        private Task _loadingTask;
        private Project _project;

        public ProjectManager(
            IRawLaunchArguments launchArguments,
            IConsoleHandle consoleHandle,
            ICoroutine coroutine)
        {
            _coroutine = coroutine;
            _consoleHandle = consoleHandle;

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
            _consoleHandle.LogDebug("Loading project from {0}...", project.ProjectPath.FullName);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(moduleInfoFile.FullName);

            project.Name = xmlDocument.SelectSingleNode("/Module/Name").InnerText;
            var packages = xmlDocument.SelectNodes("/Module/Packages/Package");
            var packagesList = new List<PackageInfo>();

            await Task.Yield();

            project.LoadingStatus = "Loading package list...";
            _consoleHandle.LogDebug("Loading package list...");

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
            _consoleHandle.LogDebug("Loading definitions...");

            var definitionsList = new List<DefinitionInfo>();
            foreach (var definitionFile in projectsDefinitionsDirectory.GetFiles("*.definition"))
            {
                var definitionXmlDocument = new XmlDocument();
                definitionXmlDocument.Load(definitionFile.FullName);
                _consoleHandle.LogDebug("Loading definition file {0}...", definitionFile.Name);

                var type = definitionXmlDocument.DocumentElement.GetAttribute("Type");
                switch (definitionXmlDocument.DocumentElement.Name)
                {
                    case "ContentProject":
                        type = "Content";
                        break;
                    case "IncludeProject":
                        type = "Include";
                        break;
                    case "ExternalProject":
                        type = "External";
                        break;
                }

                var role = "Default";
                if (type != "Content" && type != "Include" && type != "External")
                {
                    var referenceNodes = definitionXmlDocument.SelectNodes("//Reference").OfType<XmlElement>().ToList();

                    if (referenceNodes.Any(x => x.GetAttribute("Include") == "Protogame.EntryPoint"))
                    {
                        if (type == "Console")
                        {
                            role = "Server";
                        }
                        else
                        {
                            role = "Game";
                        }
                    }
                }

                var definitionInfo = new DefinitionInfo
                {
                    Name = definitionXmlDocument.DocumentElement.GetAttribute("Name"),
                    Path = definitionXmlDocument.DocumentElement.GetAttribute("Path"),
                    Type = type,
                    Role = role,
                    LoadedDocument = definitionXmlDocument,
                };
                definitionsList.Add(definitionInfo);

                if (definitionInfo.Type == "Content")
                {
                    project.LoadingStatus = "Scanning for content within " + definitionInfo.Name + "...";
                    _consoleHandle.LogDebug("Scanning for content within {0}...", definitionInfo.Name);

                    // TODO: Use a file watcher.
                    definitionInfo.ScannedContent = await ScanContentProject(project, definitionInfo);

                    project.LoadingStatus = "Loading definitions...";
                }
                else if (definitionInfo.Role == "Game")
                {
                    if (project.DefaultGame == null)
                    {
                        project.DefaultGame = definitionInfo;
                        project.DefaultGameBinPath = new FileInfo(Path.Combine(project.ProjectPath.FullName, definitionInfo.Path, "bin", "Windows", "AnyCPU", "Release", definitionInfo.Name + ".exe"));
                    }
                }
            }

            project.Definitions = definitionsList;

            project.LoadingStatus = null;

            _consoleHandle.LogDebug("Project loading has completed.");
        }

        private async Task<List<FileInfo>> ScanContentProject(Project project, DefinitionInfo definitionInfo)
        {
            var file = new List<FileInfo>();
            
            foreach (var source in definitionInfo.LoadedDocument.SelectNodes("//Source").OfType<XmlElement>())
            {
                file.AddRange(GetListOfFilesInDirectory(Path.Combine(project.ProjectPath.FullName, definitionInfo.Path, source.GetAttribute("Include")), source.GetAttribute("Match")));
            }

            return file;
        }

        private List<FileInfo> GetListOfFilesInDirectory(string folder, string match)
        {
            var result = new List<FileInfo>();
            if (!Directory.Exists(folder))
            {
                return result;
            }
            var directoryInfo = new DirectoryInfo(folder);
            foreach (var directory in directoryInfo.GetDirectories())
            {
                result.AddRange(
                    this.GetListOfFilesInDirectory(directory.FullName, match));
            }
            foreach (var file in directoryInfo.GetFiles(match))
            {
                result.Add(file);
            }
            return result;
        }
    }
}
