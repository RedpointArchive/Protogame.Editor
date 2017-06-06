using System;

namespace Protogame.Editor.Api.Version1
{
    /// <summary>
    /// Declares an editor extension.
    /// </summary>
    public class ExtensionAttribute : Attribute
    {
        public ExtensionAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
    }
}
