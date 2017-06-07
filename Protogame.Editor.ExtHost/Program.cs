using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace Protogame.Editor.ExtHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var channel = new TcpChannel(0);
            ChannelServices.RegisterChannel(channel, false);
            var channelData = (ChannelDataStore)channel.ChannelData;
            var port = new System.Uri(channelData.ChannelUris[0]).Port;

            var extensionHostService = new ExtensionHostServer();
            RemotingServices.Marshal(extensionHostService, channelData.ChannelUris[0] + "/HostServer");

            Console.WriteLine(channelData.ChannelUris[0] + "/HostServer");

            while (extensionHostService.Running)
            {
                // TODO: Do a proper tick loop.

                extensionHostService.Update();

                Thread.Sleep(16);
            }
        }
    }
}
