using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protogame.Editor
{
    public class EditorUserDataPathProvider : IEditorUserDataPathProvider
    {
        public DirectoryInfo GetPath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Protogame.Editor");
            Directory.CreateDirectory(path);
            return new DirectoryInfo(path);
        }
    }

    public interface IEditorUserDataPathProvider
    {
        DirectoryInfo GetPath();
    }
}
