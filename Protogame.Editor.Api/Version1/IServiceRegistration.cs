namespace Protogame.Editor.Api.Version1
{
    public interface IServiceRegistration
    {
        void BindTransient<TInterface, TImplementation>() where TImplementation : TInterface;

        void BindSingleton<TInterface, TImplementation>() where TImplementation : TInterface;
    }
}