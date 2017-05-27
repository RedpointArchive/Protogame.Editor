using Microsoft.Xna.Framework;
using Protogame;
using Protoinject;

namespace ProtogameUIStylingTest
{
    public class ProtogameUIStylingTestGameConfiguration : IGameConfiguration
    {
        public void ConfigureKernel(IKernel kernel)
        {
            kernel.Load<ProtogameCoreModule>();
            kernel.Load<ProtogameAssetModule>();
            kernel.Load<ProtogameEventsModule>();
            kernel.Load<ProtogameUserInterfaceModule>();
            kernel.Load<ProtogameUIStylingTestModule>();
        }
        
        public ICoreGame ConstructGame(IKernel kernel)
        {
            return new ProtogameUIStylingTestGame(kernel);
        }
    }
}