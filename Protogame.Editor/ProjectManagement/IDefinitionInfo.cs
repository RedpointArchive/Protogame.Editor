using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Protogame.Editor.ProjectManagement
{
    public interface IDefinitionInfo
    {
        string Name { get; }

        string Type { get; }

        string Role { get; }

        XmlDocument LoadedDocument { get; }

        List<FileInfo> ScannedContent { get; }
    }
}
