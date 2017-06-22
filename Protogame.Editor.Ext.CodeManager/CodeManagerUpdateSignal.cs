using Protogame.Editor.Api.Version1.Core;

namespace Protogame.Editor.Ext.CodeManager
{
    public class CodeManagerUpdateSignal : IWantsUpdateSignal
    {
        private readonly IApiReferenceService _apiReferenceService;
        private readonly ICodeManagerService _codeManagerService;

        public CodeManagerUpdateSignal(
            IApiReferenceService apiReferenceService,
            ICodeManagerService codeManagerService)
        {
            _apiReferenceService = apiReferenceService;
            _codeManagerService = codeManagerService;
        }

        public void Update()
        {
            _apiReferenceService.Update();
            _codeManagerService.Update();
        }
    }
}
