using Protogame.Editor.Api.Version1.ProjectManagement;
using System;

namespace Protogame.Editor.ProjectManagement
{
    public class PackageInfo : MarshalByRefObject, IPackageInfo
    {
        public string Repository { get; set; }

        public string Package { get; set; }

        public string Version { get; set; }
    }
}
