using Protogame;

namespace ProtogameUIStylingTest
{
    public class SingleTabbedContainer : SingleContainer, ITabbableContainer
    {
        public string Title { get; set; }
        public IAssetReference<TextureAsset> Icon { get; set; }
    }
}
