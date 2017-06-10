using Protoinject;

namespace Protogame.Editor.Api.Version1
{
    public interface IEditorExtension
    {
        void RegisterServices(IKernel kernel);
    }
}
