using System;
using System.Xml;

namespace Protogame.Editor.ProjectManagement
{
    public class DefinitionInfo : IDefinitionInfo
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Path { get; set; }

        public XmlDocument LoadedDocument { get; set; }
    }
}
