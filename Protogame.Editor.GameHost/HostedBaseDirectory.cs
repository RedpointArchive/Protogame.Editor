using Protogame.Editor.CommonHost;
using static Protogame.Editor.Grpc.Editor.GameHoster;
using Protogame.Editor.Grpc.Editor;

namespace Protogame.Editor.GameHost
{
    public class HostedBaseDirectory : IBaseDirectory
    {
        private readonly GameHosterClient _gameHosterClient;
        private string _fullPath;

        public HostedBaseDirectory(IEditorClientProvider editorClientProvider)
        {
            _gameHosterClient = editorClientProvider.GetClient<GameHosterClient>();
        }

        public string FullPath
        {
            get
            {
                if (_fullPath == null)
                {
                    _fullPath = _gameHosterClient.GetBaseDirectory(new GetBaseDirectoryRequest()).BaseDirectory;
                }

                return _fullPath;
            }
        }
    }
}
