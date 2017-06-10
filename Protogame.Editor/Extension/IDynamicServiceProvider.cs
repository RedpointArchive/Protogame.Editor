namespace Protogame.Editor.Extension
{
    public interface IDynamicServiceProvider
    {
        T[] GetAll<T>();
    }
}
