using Protoinject;

namespace Protogame.Editor.EditorWindow
{
    public interface IEditorWindowFactory : IGenerateFactory
    {
        ProjectEditorWindow CreateProjectEditorWindow();

        HierarchyEditorWindow CreateHierarchyEditorWindow();

        ConsoleEditorWindow CreateConsoleEditorWindow();

        InspectorEditorWindow CreateInspectorEditorWindow();

        ProfilerEditorWindow CreateProfilerEditorWindow();

        WorldEditorWindow CreateWorldEditorWindow();

        GameEditorWindow CreateGameEditorWindow();

        StartEditorWindow CreateStartEditorWindow();
    }
}
