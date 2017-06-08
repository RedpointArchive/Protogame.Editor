using System;

namespace Protogame.Editor.Api.Version1
{
    public interface IServiceRegistration
    {
        void BindTransient<TInterface, TImplementation>() where TImplementation : TInterface;

        void BindSingleton<TInterface, TImplementation>() where TImplementation : TInterface;

        void BindTransient(Type @interface, Func<object> factory);

        void BindSingleton(Type @interface, Func<object> factory);
    }
}