using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Protogame.Editor.ProjectManagement
{
    public class Project : IProject
    {
        public DirectoryInfo ProjectPath { get; set; }

        public string LoadingStatus { get; set; }

        public string Name { get; set; }

        public List<PackageInfo> Packages { get; set; }

        ReadOnlyCollection<IPackageInfo> IProject.Packages => Packages == null ? null : Packages.OfType<IPackageInfo>().ToList().AsReadOnly();

        public List<DefinitionInfo> Definitions { get; set; }

        ReadOnlyCollection<IDefinitionInfo> IProject.Definitions => Definitions == null ? null : Definitions.OfType<IDefinitionInfo>().ToList().AsReadOnly();

        public IDefinitionInfo DefaultGame { get; set; }

        public FileInfo DefaultGameBinPath { get; set; }
    }
}
