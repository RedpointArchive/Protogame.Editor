namespace Protogame.Editor.Extension
{
    public interface IExtensionManager
    {
        Extension[] Extensions { get; }

        void Update();

        void DebugExtension(Extension extension);
    }
}