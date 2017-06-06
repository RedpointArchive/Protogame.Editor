using Protogame.Editor.Api.Version1.Core;

namespace Protogame.Editor.Ext.CodeManager
{
    public class CodeManagerUpdateSignal : IWantsUpdateSignal
    {
        private ICodeManagerService _codeManagerService;

        public CodeManagerUpdateSignal(ICodeManagerService codeManagerService)
        {
            _codeManagerService = codeManagerService;
        }

        public void Update()
        {
            _codeManagerService.Update();
        }
    }
}
