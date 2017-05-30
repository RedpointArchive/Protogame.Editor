using System;
using System.IO;
using System.Reflection;

namespace Protogame.Editor.GameHost
{
    [Serializable]
    public class GameLoaderContext
    {
        private readonly string _editorBase;
        private readonly string _gameBase;

        public GameLoaderContext(string editorBase, string gameBase)
        {
            _editorBase = editorBase;
            _gameBase = gameBase;
        }

        public Assembly ResolveAssembly(object sender, ResolveEventArgs e)
        {
            var asmName = new AssemblyName(e.Name);

            if (asmName.Name == "Protogame")
            {
                //_consoleHandle.LogDebug("Loading Protogame from {0}", Path.Combine(domaininfo.ApplicationBase, "Protogame.dll"));
                return Assembly.LoadFile(Path.Combine(_gameBase, "Protogame.dll"));
            }

            if (asmName.Name == "MonoGame.Framework")
            {
                //_consoleHandle.LogDebug("Loading MonoGame from {0}", monogameType.Assembly.Location);
                return Assembly.LoadFile(_editorBase);
            }

            var asmdllPath = Path.Combine(_gameBase, asmName.Name + ".dll");
            var asmexePath = Path.Combine(_gameBase, asmName.Name + ".exe");
            if (File.Exists(asmdllPath))
            {
                return Assembly.LoadFile(asmdllPath);
            }
            if (File.Exists(asmexePath))
            {
                return Assembly.LoadFile(asmexePath);
            }

            //_consoleHandle.LogDebug("Loading {0} using .NET resolution", asmName.Name);
            return Assembly.Load(asmName);
        }
    }
}
