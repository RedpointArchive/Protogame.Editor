namespace Protogame.Editor.ExtHost
{
    public interface IEditorClientProvider
    {
        void CreateChannel(string url);
        
        T GetClient<T>();
    }
}
