using Microsoft.Xna.Framework.Graphics;
using Protogame.Editor.LoadedGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Protogame.Editor.ProjectManagement
{
    public class RecentProjects : IRecentProjects
    {
        private readonly IEditorUserDataPathProvider _editorUserDataPathProvider;
        private readonly List<Texture2D> _loadedTextures;
        private readonly IConsoleHandle _consoleHandle;

        public RecentProjects(
            IEditorUserDataPathProvider editorUserDataPathProvider,
            IConsoleHandle consoleHandle)
        {
            _editorUserDataPathProvider = editorUserDataPathProvider;
            _consoleHandle = consoleHandle;
            _loadedTextures = new List<Texture2D>();
        }

        public async Task<List<RecentProject>> GetRecentProjects(IRenderContext renderContext)
        {
            var recentProjectsFile = Path.Combine(_editorUserDataPathProvider.GetPath().FullName, "RecentProjects.txt");
            var recentProjectPaths = new List<string>();
            if (!File.Exists(recentProjectsFile))
            {
                return new List<RecentProject>();
            }

            using (var reader = new StreamReader(recentProjectsFile))
            {
                while (!reader.EndOfStream)
                {
                    var dir = (await reader.ReadLineAsync()).Trim();
                    if (Directory.Exists(dir) && File.Exists(Path.Combine(dir, "Build", "Module.xml")))
                    {
                        recentProjectPaths.Add(dir);
                    }
                }
            }

            return recentProjectPaths.Select(path =>
            {
                var document = new XmlDocument();
                document.Load(Path.Combine(path, "Build", "Module.xml"));
                var name = document.SelectSingleNode("/Module/Name").InnerText.Trim();

                var thumbnailPath = Path.Combine(path, "Build", "Editor", "Thumbnail.png");
                Texture2D tex = null;
                if (File.Exists(thumbnailPath))
                {
                    try
                    {
                        using (var stream = new FileStream(thumbnailPath, FileMode.Open, FileAccess.Read))
                        {
                            tex = Texture2D.FromStream(renderContext.GraphicsDevice, stream);
                            _loadedTextures.Add(tex);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignore invalid thumbnails.
                        _consoleHandle.LogError(ex);
                    }
                }

                return new RecentProject
                {
                    Name = name,
                    Path = path,
                    Thumbnail = tex
                };
            }).ToList();
        }

        public void DisposeAllLoadedTextures()
        {
            if (_loadedTextures.Count > 0)
            {
                foreach (var tex in _loadedTextures)
                {
                    tex.Dispose();
                }

                _loadedTextures.Clear();
            }
        }

        public async Task AddProjectToRecentProjects(string path)
        {
            var recentProjectsFile = Path.Combine(_editorUserDataPathProvider.GetPath().FullName, "RecentProjects.txt");
            var recentProjectPaths = new List<string>();
            if (File.Exists(recentProjectsFile))
            {
                using (var reader = new StreamReader(recentProjectsFile))
                {
                    while (!reader.EndOfStream)
                    {
                        var dir = (await reader.ReadLineAsync()).Trim();
                        if (Directory.Exists(dir) && File.Exists(Path.Combine(dir, "Build", "Module.xml")))
                        {
                            recentProjectPaths.Add(dir);
                        }
                    }
                }
            }

            if (recentProjectPaths.Contains(path))
            {
                recentProjectPaths.Remove(path);
            }

            recentProjectPaths.Insert(0, path);

            using (var writer = new StreamWriter(recentProjectsFile, false))
            {
                foreach (var p in recentProjectPaths)
                {
                    await writer.WriteLineAsync(p);
                }
            }
        }
    }

    public interface IRecentProjects
    {
        Task<List<RecentProject>> GetRecentProjects(IRenderContext renderContext);

        Task AddProjectToRecentProjects(string path);

        void DisposeAllLoadedTextures();
    }

    public class RecentProject
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public Texture2D Thumbnail { get; set; }
    }
}
