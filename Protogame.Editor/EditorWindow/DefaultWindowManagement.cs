using System.Linq;
using Protogame.Editor.Layout;
using Protoinject;

namespace Protogame.Editor.EditorWindow
{
    public class DefaultWindowManagement : IWindowManagement
    {
        private DockableLayoutContainer _workspaceContainer;
        private readonly IKernel _kernel;

        public DefaultWindowManagement(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void OpenDocument<T>(object parameters) where T : EditorWindow
        {
            var existingDocument = _workspaceContainer.InnerRegions.OfType<T>().FirstOrDefault(x => x.Userdata == parameters);
            if (existingDocument != null)
            {
                // Focus on existing document.
                _workspaceContainer.ActivateWhere(x => x == existingDocument);
                return;
            }

            var child = _kernel.Get<T>();
            child.Userdata = parameters;
            _workspaceContainer.AddInnerRegion(child);
            _workspaceContainer.ActivateWhere(x => x == child);
        }

        public void SetMainDocumentContainer(DockableLayoutContainer workspaceContainer)
        {
            _workspaceContainer = workspaceContainer;
        }
    }
}
