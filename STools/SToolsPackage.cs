using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace S2.STools
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da52}")]
    [Guid(GuidList.guidSToolsPkgString)]
    public sealed class SToolsPackage : Package
    {
        private DTE _dte = null;

        public SToolsPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        protected override void Initialize()
        {
            base.Initialize();
            _dte = (DTE)GetService(typeof(DTE));
        }

        public DTE GetDTE()
        {
            return _dte;
        }
    }
}
