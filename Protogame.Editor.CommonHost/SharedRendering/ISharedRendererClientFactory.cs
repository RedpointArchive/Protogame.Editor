using Protoinject;

namespace Protogame.Editor.CommonHost.SharedRendering
{
    public interface ISharedRendererClientFactory : IGenerateFactory
    {
        SharedRendererClient CreateSharedRendererClient();
    }
}
