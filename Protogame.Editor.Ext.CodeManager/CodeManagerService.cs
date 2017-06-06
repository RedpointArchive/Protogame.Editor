using Protogame.Editor.Api.Version1.Core;
using Protogame.Editor.Api.Version1.ProjectManagement;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Protogame.Editor.Ext.CodeManager
{
    public class CodeManagerService : ICodeManagerService
    {
        private readonly IProjectManager _projectManager;
        private readonly IConsoleHandle _consoleHandle;
        private IProject _project;
        private Process _process;

        public CodeManagerService(
            IProjectManager projectManager,
            IConsoleHandle consoleHandle)
        {
            _projectManager = projectManager;
            _consoleHandle = consoleHandle;
        }

        public bool IsProcessRunning => _process != null;

        public void Update()
        {
            if (_project != _projectManager.Project)
            {
                _project = _projectManager.Project;

                AutoCSharpProject();
            }
        }

        public void AutoCSharpProject()
        {
            if (_process != null)
            {
                return;
            }

            StartProtobuild("--resync", () =>
            {
                StartProtobuild("--build", null);
            });
        }

        public void BuildCSharpProject()
        {
            if (_process != null)
            {
                return;
            }

            StartProtobuild("--build", null);
        }

        public void ResyncCSharpProject()
        {
            if (_process != null)
            {
                return;
            }

            StartProtobuild("--resync", null);
        }

        public void SyncCSharpProject()
        {
            if (_process != null)
            {
                return;
            }

            StartProtobuild("--sync", null);
        }

        public void GenerateCSharpProject()
        {
            if (_process != null)
            {
                return;
            }

            StartProtobuild("--generate", null);
        }

        public void UpgradeAllPackages()
        {
            if (_process != null)
            {
                return;
            }

            StartProtobuild("--upgrade-all", null);
        }

        public void OpenCSharpProject()
        {
            FindVsInstall((wasSuccessful, paths) =>
            {
                if (!wasSuccessful)
                {
                    // We need to install vswhere first.
                    StartProtobuild("--install vswhere", () =>
                    {
                        OpenCSharpProject();
                    });
                }
                else
                {
                    foreach (var p in paths)
                    {
                        var path = Path.Combine(p, "Common7\\IDE\\devenv.exe");
                        if (File.Exists(path))
                        {
                            var processStartInfo = new ProcessStartInfo
                            {
                                FileName = path,
                                Arguments = _project.SolutionFile.FullName,
                                WorkingDirectory = _project.ProjectPath.FullName,
                            };
                            Process.Start(processStartInfo);
                            return;
                        }
                    }
                }
            });
        }

        private void FindVsInstall(Action<bool, string[]> callback)
        {
            var buffer = "";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(_project.ProjectPath.FullName, "Protobuild.exe"),
                Arguments = "--execute vswhere -legacy -property installationPath",
                WorkingDirectory = _project.ProjectPath.FullName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            try
            {
                _process = Process.Start(processStartInfo);
                _process.Exited += (sender, e) =>
                {
                    callback?.Invoke(_process.ExitCode == 0, buffer.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                    _process = null;
                };
                _process.EnableRaisingEvents = true;
                _process.OutputDataReceived += (sender, e) =>
                {
                    buffer += e.Data + Environment.NewLine;
                };
                _process.BeginOutputReadLine();
                if (_process.HasExited)
                {
                    _process = null;
                }
            }
            catch (Exception ex)
            {
                _process = null;
                throw;
            }
        }

        private void StartProtobuild(string args, Action callback)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(_project.ProjectPath.FullName, "Protobuild.exe"),
                Arguments = args,
                WorkingDirectory = _project.ProjectPath.FullName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            try
            {
                _process = Process.Start(processStartInfo);
                _process.Exited += (sender, e) =>
                {
                    _process = null;
                    callback?.Invoke();
                };
                _process.EnableRaisingEvents = true;
                _process.OutputDataReceived += (sender, e) =>
                {
                    _consoleHandle.LogDebug(e.Data);
                };
                _process.ErrorDataReceived += (sender, e) =>
                {
                    _consoleHandle.LogWarning(e.Data);
                };
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
                if (_process.HasExited)
                {
                    _process = null;
                }
            }
            catch (Exception ex)
            {
                _process = null;
                throw;
            }
        }
    }
}
