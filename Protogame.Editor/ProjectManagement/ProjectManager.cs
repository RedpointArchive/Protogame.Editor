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
        private readonly IRecentProjects _recentProjects;
        private FileSystemWatcher _fileSystemWatcher;

        public ProjectManager(
            IRawLaunchArguments launchArguments,
            IConsoleHandle consoleHandle,
            ICoroutine coroutine,
            IRecentProjects recentProjects)
        {
            _coroutine = coroutine;
            _consoleHandle = consoleHandle;
            _recentProjects = recentProjects;

            var arguments = launchArguments.Arguments;
            var directoryIndex = Array.IndexOf(arguments, "--project");
            if (!(directoryIndex == -1 || directoryIndex == arguments.Length - 1))
            {
                LoadProject(arguments[directoryIndex + 1]);
            }
        }

        public IProject Project => _project;

        public void LoadProject(string directoryPath)
        {
            _project = new Project
            {
                ProjectPath = new DirectoryInfo(directoryPath)
            };

            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.Dispose();
            }

            _fileSystemWatcher = new FileSystemWatcher(directoryPath);
            _fileSystemWatcher.NotifyFilter = 
                NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.Security;
            _fileSystemWatcher.Filter = "*";
            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Created += _fileSystemWatcher_Created;
            _fileSystemWatcher.Deleted += _fileSystemWatcher_Deleted;
            _fileSystemWatcher.Renamed += _fileSystemWatcher_Renamed;
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.EnableRaisingEvents = true;

            _loadingTask = _coroutine.Run(async () => await LoadProjectDataAsync(_project));
        }

        private void _fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.OldFullPath != null)
            {
                _consoleHandle.LogDebug("Renamed from: " + e.OldFullPath);
                FileChanged(e.OldFullPath);
            }
            if (e.FullPath != null)
            {
                _consoleHandle.LogDebug("Renamed to: " + e.FullPath);
                FileChanged(e.FullPath);
            }
        }

        private void _fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath != null)
            {
                _consoleHandle.LogDebug("Deleted: " + e.FullPath);
                FileChanged(e.FullPath);
            }
        }

        private void _fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath != null)
            {
                _consoleHandle.LogDebug("Created: " + e.FullPath);
                FileChanged(e.FullPath);
            }
        }

        private void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath != null)
            {
                _consoleHandle.LogDebug("Changed: " + e.FullPath);
                FileChanged(e.FullPath);
            }
        }

        private void FileChanged(string path)
        {
            if (string.Equals(_project.DefaultGameBinPath.FullName, path, StringComparison.InvariantCultureIgnoreCase))
            {
                _project.DefaultGameBinPath = new FileInfo(_project.DefaultGameBinPath.FullName);
            }
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

            project.SolutionFile = new FileInfo(Path.Combine(project.ProjectPath.FullName, project.Name + ".Windows.sln"));

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
                    LoadedDocumentPath = definitionFile.FullName,
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
                        var debugFile = new FileInfo(Path.Combine(project.ProjectPath.FullName, definitionInfo.Path, "bin", "Windows", "AnyCPU", "Debug", definitionInfo.Name + ".exe"));
                        var releaseFile = new FileInfo(Path.Combine(project.ProjectPath.FullName, definitionInfo.Path, "bin", "Windows", "AnyCPU", "Release", definitionInfo.Name + ".exe"));

                        project.DefaultGame = definitionInfo;
                        if (debugFile.Exists)
                        {
                            if (releaseFile.Exists)
                            {
                                if (debugFile.LastWriteTimeUtc > releaseFile.LastWriteTimeUtc)
                                {
                                    project.DefaultGameBinPath = debugFile;
                                }
                                else
                                {
                                    project.DefaultGameBinPath = releaseFile;
                                }
                            }
                            else
                            {
                                project.DefaultGameBinPath = debugFile;
                            }
                        }
                        else if (releaseFile.Exists)
                        {
                            project.DefaultGameBinPath = releaseFile;
                        }
                        else
                        {
                            project.DefaultGameBinPath = debugFile;
                        }
                    }
                }
            }

            project.Definitions = definitionsList;

            project.LoadingStatus = null;

            await _recentProjects.AddProjectToRecentProjects(project.ProjectPath.FullName);

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
