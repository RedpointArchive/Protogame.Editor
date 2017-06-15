using Protogame.Editor.Api.Version1;
using Protogame.Editor.Api.Version1.Core;
using Protogame.Editor.Api.Version1.ProjectManagement;
using Protogame.Editor.Grpc.Editor;
using Protogame.Editor.Grpc.ExtensionHost;
using Protoinject;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Protogame.Editor.ExtHost
{
    public class ExtensionHost
    {
        private IEditorExtension[] _editorExtensions;
        private IWantsUpdateSignal[] _wantsUpdateSignal;

        public bool Running => true;
        
        public async Task<string> Start(IGrpcServer grpcServer, IEditorClientProvider editorClientProvider, string assemblyFile)
        {
            var console = editorClientProvider.GetClient<Protogame.Editor.Grpc.Editor.Console.ConsoleClient>();

            console.LogDebug(new LogRequest { Message = string.Format("Loading assembly from: {0}", assemblyFile) });
            var assembly = Assembly.LoadFrom(assemblyFile);

            // Find the attributes that describe the extensions provided by this assembly.
            var editorExtensions = new List<IEditorExtension>();
            foreach (var attr in assembly.GetCustomAttributes<ExtensionAttribute>())
            {
                try
                {
                    editorExtensions.Add((IEditorExtension)Activator.CreateInstance(attr.Type));
                }
                catch (Exception e)
                {
                    console.LogError(new LogRequest { Message = e.ToString() });
                }
            }
            _editorExtensions = editorExtensions.ToArray();

            // Perform service registration.
            var localKernel = new StandardKernel();
            localKernel.Bind<IEditorClientProvider>().ToMethod(x => editorClientProvider).InSingletonScope();
            localKernel.Bind<IProjectManager>().To<ProjectManager>().InSingletonScope();
            localKernel.Bind<IWantsUpdateSignal>().To<ProjectManagerUpdateSignal>().InSingletonScope();
            localKernel.Bind<IConsoleHandle>().To<ConsoleHandle>().InSingletonScope();
            foreach (var ext in _editorExtensions)
            {
                ext.RegisterServices(localKernel);
            }
            
            _wantsUpdateSignal = await localKernel.GetAllAsync<IWantsUpdateSignal>();

            // Add services to gRPC server now that we can resolve implementations.
            return await grpcServer.StartAndGetRuntimeServerUrlAsync(localKernel);
        }

        public void Update()
        {
            if (_wantsUpdateSignal != null)
            {
                foreach (var u in _wantsUpdateSignal)
                {
                    u.Update();
                }
            }
        }
    }
}
