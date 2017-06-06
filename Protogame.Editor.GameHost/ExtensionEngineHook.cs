using Protogame.Editor.Api.Game.Version1;
using Protogame.Editor.Ext.Inspector.Game;
using System.Collections.Generic;

namespace Protogame.Editor.GameHost
{
    public class ExtensionEngineHook : IEngineHook
    {
        private readonly List<IGameHostExtension> _extensions;

        public ExtensionEngineHook(
            InspectorGameHostExtension extension)
        {
            _extensions = new List<IGameHostExtension>();
            _extensions.Add(extension);
        }

        public void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            foreach (var ext in _extensions)
            {
                ext.Render(gameContext, renderContext);
            }
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            foreach (var ext in _extensions)
            {
                ext.Update(gameContext, updateContext);
            }
        }

        public void Update(IServerContext serverContext, IUpdateContext updateContext)
        {
        }
    }
}
