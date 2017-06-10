using System.Collections.ObjectModel;
using System.IO;

namespace Protogame.Editor.ProjectManagement
{
    public interface IProject
    {
        DirectoryInfo ProjectPath { get; }

        string LoadingStatus { get; }

        string Name { get; }

        ReadOnlyCollection<IPackageInfo> Packages { get; }

        ReadOnlyCollection<IDefinitionInfo> Definitions { get; }

        IDefinitionInfo DefaultGame { get; }
        
        FileInfo SolutionFile { get; }

        FileInfo DefaultGameBinPath { get; }
    }
}
