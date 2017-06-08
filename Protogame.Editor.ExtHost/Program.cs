using Protogame.Editor.Api.Version1;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace Protogame.Editor.ExtHost
{
    public static class Program
    {
        private static ExtensionHostServer _extensionHostServer;

        public static void Main(string[] args)
        {
            var serverProvider = new BinaryServerFormatterSinkProvider();
            serverProvider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            var clientProvider = new BinaryClientFormatterSinkProvider();
            var properties = new Hashtable();
            properties["port"] = 0;

            var channel = new TcpChannel(properties, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(channel, false);
            var channelData = (ChannelDataStore)channel.ChannelData;
            var port = new System.Uri(channelData.ChannelUris[0]).Port;

            int? trackProcessId = null;
            if (args.Length == 2)
            {
                if (args[0] == "--track")
                {
                    var process = Process.GetProcessById(int.Parse(args[1]));
                    process.Exited += (sender, e) =>
                    {
                        Console.WriteLine("Parent process " + args[1] + " has exited, closing extension host process.");
                        Environment.Exit(0);
                    };
                    process.EnableRaisingEvents = true;
                }
            }

            var hostUri = "tcp://localhost:" + port + "/";

            _extensionHostServer = new ExtensionHostServer();
            RemotingServices.Marshal(_extensionHostServer, "HostServer", typeof(IExtensionHostServer));
            
            Console.WriteLine(hostUri);
            Console.Error.WriteLine(hostUri);

            while (_extensionHostServer.Running)
            {
                // TODO: Do a proper tick loop.

                _extensionHostServer.Update();

                Thread.Sleep(16);
            }
        }
    }
}
