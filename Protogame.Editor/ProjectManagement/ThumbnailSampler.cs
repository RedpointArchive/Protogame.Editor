using Microsoft.Xna.Framework.Graphics;
using Protogame.Editor.LoadedGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protogame.Editor.ProjectManagement
{
    public class ThumbnailSampler : IThumbnailSampler
    {
        private readonly IProjectManager _projectManager;
        private readonly ILoadedGame _loadedGame;
        private readonly IConsoleHandle _consoleHandle;
        private readonly IGraphicsBlit _graphicsBlit;

        public ThumbnailSampler(
            IProjectManager projectManager,
            ILoadedGame loadedGame,
            IConsoleHandle consoleHandle,
            IGraphicsBlit graphicsBlit)
        {
            _projectManager = projectManager;
            _loadedGame = loadedGame;
            _consoleHandle = consoleHandle;
            _graphicsBlit = graphicsBlit;
        }

        public void WriteThumbnailIfNecessary(IGameContext gameContext, IRenderContext renderContext)
        {
            var path = _projectManager?.Project?.ProjectPath;
            if (path == null || !path.Exists)
            {
                return;
            }

            var editorPath = Path.Combine(Path.Combine(path.FullName, "Build", "Editor"));
            Directory.CreateDirectory(editorPath);

            var thumbnailFile = new FileInfo(Path.Combine(editorPath, "Thumbnail.png"));

            if (_loadedGame.PlayingFor.TotalMinutes >= 1)
            {
                if (!thumbnailFile.Exists || thumbnailFile.LastWriteTimeUtc < DateTime.UtcNow.AddHours(-4))
                {
                    _consoleHandle.LogInfo("Sampling current game screen as thumbnail for project...");

                    var srt = _loadedGame.GetCurrentGameRenderTarget();
                    var rt = new RenderTarget2D(renderContext.GraphicsDevice, 128, 128, false, SurfaceFormat.Color, DepthFormat.None);
                    _graphicsBlit.Blit(renderContext, srt, rt);

                    try
                    {
                        using (var writer = new FileStream(thumbnailFile.FullName, FileMode.Create, FileAccess.Write))
                        {
                            rt.SaveAsPng(writer, 128, 128);
                        }
                    }
                    catch
                    {
                        thumbnailFile.Delete();
                        throw;
                    }

                    rt.Dispose();
                }
            }
        }
    }

    public interface IThumbnailSampler
    {
        void WriteThumbnailIfNecessary(IGameContext gameContext, IRenderContext renderContext);
    }
}
