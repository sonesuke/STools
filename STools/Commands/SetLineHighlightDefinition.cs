using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S2.STools.Commands
{
    class SetLineHighlightDefinition : ICommand
    {
        public bool IsEnable(EnvDTE.DTE dte)
        {
            return true;
        }

        public void Execute(EnvDTE.DTE dte)
        {
            LineHighlightAdorment.LineHighlightAdorment.SetDefinition("hoge");
        }

        public bool IsYourId(uint commandId)
        {
            return commandId == PkgCmdIDList.CommandIdLineHighlight;
        }
    }
}
