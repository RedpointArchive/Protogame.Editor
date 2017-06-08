using Protogame.Editor.Api.Version1;
using System.Collections.Generic;

namespace Protogame.Editor.Extension
{
    public interface IDynamicServiceProvider : IServiceRegistration
    {
        T[] GetAll<T>();
    }
}
