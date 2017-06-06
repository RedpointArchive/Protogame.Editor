using Microsoft.Xna.Framework;
using Protogame.Editor.Api.Version1.ProjectManagement;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Protogame.Editor.ProjectManagement
{
    public class ProjectManagerUi : MarshalByRefObject, IProjectManagerUi
    {
        private readonly IProjectManager _projectManager;

        public ProjectManagerUi(IProjectManager projectManager)
        {
            _projectManager = projectManager;
        }

        public async Task LoadProject()
        {
#if PLATFORM_WINDOWS
            string project = null;

            var t = new Thread(new ThreadStart(() =>
            {
                var dialog = new Ookii.Dialogs.VistaFolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    project = dialog.SelectedPath;
                }
            }));
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();

            while (t.IsAlive)
            {
                await Task.Yield();
            }

            _projectManager.LoadProject(project);
#endif
        }
    }
}
