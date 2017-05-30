using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Protogame.Editor.ProjectManagement
{
    public class DefinitionInfo : IDefinitionInfo
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Path { get; set; }

        public string Role { get; set; }

        public XmlDocument LoadedDocument { get; set; }

        public List<FileInfo> ScannedContent { get; set; }
    }
}
