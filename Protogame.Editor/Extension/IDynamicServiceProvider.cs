using System.Collections.Generic;

namespace Protogame.Editor.Extension
{
    public interface IDynamicServiceProvider
    {
        void BindSingleton<TInterface, TImplementation>() where TImplementation : TInterface;

        void BindTransient<TInterface, TImplementation>() where TImplementation : TInterface;

        T[] GetAll<T>();
    }
}
