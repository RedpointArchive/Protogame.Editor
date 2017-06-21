using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Protogame.Editor.CommonHost
{
    public class EditorClientProvider : IEditorClientProvider
    {
        private Channel _channel;
        private Dictionary<Type, object> _clients = new Dictionary<Type, object>();

        public void CreateChannel(string url)
        {
            _channel = new Channel(url, ChannelCredentials.Insecure);
        }

        public T GetClient<T>()
        {
            if (_channel == null)
            {
                throw new InvalidOperationException();
            }

            var t = typeof(T);

            if (_clients.ContainsKey(t))
            {
                return (T)_clients[t];
            }

            _clients[t] = t.GetConstructor(new[] { typeof(Channel) }).Invoke(new object[] { _channel });
            return (T)_clients[t];
        }
    }
}
