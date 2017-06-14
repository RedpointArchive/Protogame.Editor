using Microsoft.Xna.Framework;
using Protogame;
using Protoinject;

namespace Protogame.Editor
{
    public class ProtogameEditorGameConfiguration : IGameConfiguration
    {
        public void ConfigureKernel(IKernel kernel)
        {
            kernel.Load<ProtogameCoreModule>();
            kernel.Load<ProtogameAssetModule>();
            kernel.Load<ProtogameEventsModule>();
            kernel.Load<ProtogameUserInterfaceModule>();
            kernel.Load<ProtogameProfilingModule>();
            kernel.Load<ProtogameEditorModule>();
        }
        
        public ICoreGame ConstructGame(IKernel kernel)
        {
            return new ProtogameEditorGame(kernel);
        }
    }
}