using System;

namespace Protogame.Editor.Api.Version1
{
    [Serializable]
    public class RegisteredService
    {
        public Type Interface { get; set; }

        public string ImplementationUri { get; set; }

        public bool IsSingleton { get; set; }
    }
}
