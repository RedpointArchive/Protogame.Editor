using System;

namespace Protogame.Editor.Api.Version1
{
    public interface IExtensionHostServerRemoteResolve
    {
        object GetInstance(Type interfaceType);
    }
}
