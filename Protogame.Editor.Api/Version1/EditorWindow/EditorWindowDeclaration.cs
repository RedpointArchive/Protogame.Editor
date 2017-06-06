using System;

namespace Protogame.Editor.Api.Version1.EditorWindow
{
    [Serializable]
    public class EditorWindowDeclaration
    {
        public EditorWindowDeclaration(Type type, string name, EditorWindowLevel level)
        {
            if (!typeof(IEditorWindow).IsAssignableFrom(type))
            {
                throw new InvalidOperationException("Declared editor window " + type.FullName + " does not implement IEditorWindow");
            }

            EditorWindowType = type;
            Name = name;
            EditorWindowLevel = level;
        }

        public Type EditorWindowType { get; }

        public string Name { get; }

        public EditorWindowLevel EditorWindowLevel { get; }
    }
}