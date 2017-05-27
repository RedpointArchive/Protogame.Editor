using System.Xml;

namespace Protogame.Editor.ProjectManagement
{
    public interface IDefinitionInfo
    {
        string Name { get; }

        string Type { get; }

        XmlDocument LoadedDocument { get; }
    }
}
