﻿using Protoinject;
using System.Threading.Tasks;

namespace Protogame.Editor.Api.Version1
{
    public interface IExtensionHostServer
    {
        Task AcceptMarshalledKernel(IKernel kernel);

        Task LoadAssembly(string assembly);
    }
}