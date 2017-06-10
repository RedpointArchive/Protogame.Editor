using Protogame.Editor.Api.Version1;
using Protogame.Editor.Api.Version1.Core;
using Protogame.Editor.Grpc.Editor;
using Protoinject;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Protogame.Editor.ExtHost
{
    public class ExtensionHost
    {
        private IEditorExtension[] _editorExtensions;
        private IWantsUpdateSignal[] _wantsUpdateSignal;

        public bool Running => true;
        
        public void Start(IEditorClientProvider editorClientProvider, string assemblyFile)
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
            foreach (var ext in _editorExtensions)
            {
                ext.RegisterServices(localKernel);
            }

            try
            {
                _wantsUpdateSignal = localKernel.GetAll<IWantsUpdateSignal>();
            }
            catch (Exception ex)
            {
                // Maybe no update signals are registered - so ignore.
            }
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
