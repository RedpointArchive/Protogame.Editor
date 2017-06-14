﻿using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Protogame.Editor.Extension
{
    public class Extension
    {
        private Dictionary<Type, object> _clients = new Dictionary<Type, object>();

        public Extension(Channel channel)
        {
            Channel = channel;
        }

        public Channel Channel { get; }

        public T GetClient<T>()
        {
            var t = typeof(T);

            if (_clients.ContainsKey(t))
            {
                return (T)_clients[t];
            }

            _clients[t] = t.GetConstructor(new[] { typeof(Channel) }).Invoke(new object[] { Channel });
            return (T)_clients[t];
        }
    }
}
