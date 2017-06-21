using Protoinject;

namespace Protogame.Editor.SharedRendering
{
    public interface ISharedRendererHostFactory : IGenerateFactory
    {
        SharedRendererHost CreateSharedRendererHost();
    }
}
