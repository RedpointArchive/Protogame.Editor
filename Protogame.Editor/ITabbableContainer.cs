using Protogame;

namespace ProtogameUIStylingTest
{
    public interface ITabbableContainer : IContainer
    {
        string Title { get; set; }

        IAssetReference<TextureAsset> Icon { get; set; }
    }
}