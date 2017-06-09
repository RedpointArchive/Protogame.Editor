using System;
using Protogame.Editor.Api.Version1;
using Protoinject;
using Protogame.Editor.Api.Version1.Core;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Collections;

namespace Protogame.Editor.Extension
{
    public class ExtensionManager : IExtensionManager
    {
        private bool _hasLoadedBundledExtensions;
        private Dictionary<string, ManagedExtension> _extensions;
        private readonly IConsoleHandle _consoleHandle;
        private readonly IServiceRegistration _serviceRegistration;
        private readonly IKernel _kernel;
        private readonly ExtensionManagerRemoteResolve _extensionRemoteResolve;

        public ExtensionManager(
            IKernel kernel,
            IConsoleHandle consoleHandle,
            IServiceRegistration serviceRegistration)
        {
            _kernel = kernel;
            _consoleHandle = consoleHandle;
            _extensions = new Dictionary<string, ManagedExtension>();
            _serviceRegistration = serviceRegistration;
            _extensionRemoteResolve = new ExtensionManagerRemoteResolve(_kernel);
            
            var serverProvider = new BinaryServerFormatterSinkProvider();
            serverProvider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            var clientProvider = new BinaryClientFormatterSinkProvider();
            var properties = new Hashtable();
            properties["port"] = 0;

            var channel = new TcpChannel(properties, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(channel, false);
        }

        public void Update()
        {
            if (!_hasLoadedBundledExtensions)
            {
                // We need to load extensions bundled with the editor.
                var editorDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
                var extensionFiles = editorDirectory.GetFiles("Protogame.Editor.Ext.*.dll");

                foreach (var file in extensionFiles)
                {
                    _consoleHandle.LogDebug("Added bundled extension {0}", file.FullName);
                    _extensions.Add(file.FullName, new ManagedExtension
                    {
                        File = file,
                        Owner = "bundled",
                        ExtensionProcess = null,
                        ExtensionHostServer = null
                    });
                }

                _hasLoadedBundledExtensions = true;
            }

            foreach (var extension in _extensions)
            {
                var ext = extension;
                if (ext.Value.ExtensionProcess == null ||
                    ext.Value.ExtensionProcess.HasExited)
                    // TODO: Also check if extension file has changed...
                {
                    var extHostPath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "Protogame.Editor.ExtHost.exe");
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = extHostPath,
                        Arguments = "--track " + Process.GetCurrentProcess().Id,
                        WorkingDirectory = ext.Value.File.DirectoryName,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    ext.Value.ExtensionProcess = Process.Start(processStartInfo);
                    ext.Value.ExtensionProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            return;
                        }
                        if (ext.Value.ExtensionHostServer != null)
                        {
                            _consoleHandle.LogDebug(e.Data);
                            return;
                        }

                        var url = e.Data?.Trim();
                        _consoleHandle.LogDebug("Connecting to extension host server on {0}...", url);
                        ext.Value.ExtensionHostServer = (IExtensionHostServer)Activator.GetObject(
                            typeof(IExtensionHostServer),
                            url + "HostServer");
                        _consoleHandle.LogDebug("Connected to extension host server on {0}", url);

                        _consoleHandle.LogDebug("Requesting extension load of {0}...", ext.Value.File.FullName);
                        ext.Value.ExtensionHostServer.RegisterRemoteResolve(_extensionRemoteResolve);
                        var registeredServices = ext.Value.ExtensionHostServer.Start(ext.Value.File.FullName);
                        _consoleHandle.LogDebug("Extension loaded: {0}", ext.Value.File.FullName);

                        foreach (var _svc in registeredServices)
                        {
                            var svc = _svc;
                            if (svc.IsSingleton)
                            {
                                _serviceRegistration.BindSingleton(
                                    svc.Interface,
                                    () =>
                                    {
                                        return ((IRemoteFactory)Activator.GetObject(
                                            svc.Interface,
                                            url + svc.ImplementationUri)).GetInstance();
                                    });
                            }
                            else
                            {
                                _serviceRegistration.BindTransient(
                                    svc.Interface,
                                    () =>
                                    {
                                        return ((IRemoteFactory)Activator.GetObject(
                                            svc.Interface,
                                            url + svc.ImplementationUri)).GetInstance();
                                    });
                            }
                        }
                    };
                    ext.Value.ExtensionProcess.BeginOutputReadLine();
                }
            }
        }

        private class ManagedExtension
        {
            public FileInfo File { get; set; }

            public string Owner { get; set; }

            public Process ExtensionProcess { get; set; }

            public IExtensionHostServer ExtensionHostServer { get; set; }
        }
    }
}
