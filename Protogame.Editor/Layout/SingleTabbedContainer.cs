using Protogame;

namespace Protogame.Editor.Layout
{
    public class SingleTabbedContainer : SingleContainer, ITabbableContainer
    {
        public string Title { get; set; }
        public IAssetReference<TextureAsset> Icon { get; set; }
    }
}
