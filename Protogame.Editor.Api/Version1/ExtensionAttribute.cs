using System;

namespace Protogame.Editor.Api.Version1
{
    /// <summary>
    /// Declares an editor extension.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ExtensionAttribute : Attribute
    {
        public ExtensionAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
    }
}
