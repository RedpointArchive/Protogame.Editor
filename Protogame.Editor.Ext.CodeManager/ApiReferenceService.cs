using System;
using Protogame.Editor.Api.Version1.ProjectManagement;
using System.IO;
using Protogame.Editor.Api.Version1;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Protogame.Editor.Ext.CodeManager
{
    public class ApiReferenceService : IApiReferenceService
    {
        private readonly IProjectManager _projectManager;
        private string _lastDirectory;

        public ApiReferenceService(IProjectManager projectManager)
        {
            _projectManager = projectManager;
        }

        public void Update()
        {
            if (_projectManager.Project != null)
            {
                var directory = Path.Combine(_projectManager.Project.ProjectPath.FullName, "Protogame.Editor");
                if (directory != _lastDirectory)
                {
                    ForceCheck();
                }
            }
        }

        public void ForceCheck()
        {
            var directory = Path.Combine(_projectManager.Project.ProjectPath.FullName, "Protogame.Editor");
            var redirect = Path.Combine(directory, ".redirect");

            Directory.CreateDirectory(directory);
            using (var writer = new StreamWriter(redirect, false))
            {
                writer.Write(GetBuildReferencesFolder());
            }

            _lastDirectory = directory;
        }

        private string GetBuildReferencesFolder()
        {
            var apiReferencePath = typeof(ExtensionAttribute).Assembly.Location;
            string tempDirName;
            using (var sha = SHA1.Create())
            {
                tempDirName = "PGEditorReferences_v1_" + BitConverter.ToString(sha.ComputeHash(Encoding.ASCII.GetBytes(apiReferencePath))).Replace("-", "");
            }
            tempDirName = Path.Combine(Path.GetTempPath(), tempDirName);

            if (Directory.Exists(tempDirName))
            {
                return tempDirName;
            }

            Directory.CreateDirectory(tempDirName);
            Directory.CreateDirectory(Path.Combine(tempDirName, "Build", "Projects"));
            
            var document = new XmlDocument();
            document.AppendChild(document.CreateXmlDeclaration("1.0", "utf-8", null));

            var module = document.CreateElement("Module");
            var nameNode = document.CreateElement("Name");
            nameNode.InnerText = "Protogame.Editor.References";
            module.AppendChild(nameNode);
            document.AppendChild(module);

            document.Save(Path.Combine(tempDirName, "Build", "Module.xml"));

            document = new XmlDocument();

            document.AppendChild(document.CreateXmlDeclaration("1.0", "utf-8", null));

            var externalProject = document.CreateElement("ExternalProject");
            externalProject.SetAttribute("Name", "Protogame.Editor.Api");
            var binaryNode = document.CreateElement("Binary");
            binaryNode.SetAttribute("Name", "Protogame.Editor.Api");
            binaryNode.SetAttribute("Path", apiReferencePath);
            externalProject.AppendChild(binaryNode);
            document.AppendChild(externalProject);

            document.Save(Path.Combine(tempDirName, "Build", "Projects", "Protogame.Editor.Api.definition"));

            File.Copy(Path.Combine(_projectManager.Project.ProjectPath.FullName, "Protobuild.exe"), Path.Combine(tempDirName, "Protobuild.exe"), true);

            return tempDirName;
        }
    }

    public interface IApiReferenceService
    {
        void Update();

        void ForceCheck();
    }
}
