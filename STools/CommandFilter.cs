using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using EnvDTE;


namespace S2.STools
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    class VsTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        internal SVsServiceProvider ServiceProvider = null;
        
        static DTE _dte = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            if (textViewAdapter == null) return;

            _dte = (DTE)ServiceProvider.GetService(typeof(DTE));

            CommandFilter filter = new CommandFilter();

            IOleCommandTarget next;
            if (ErrorHandler.Succeeded(textViewAdapter.AddCommandFilter(filter, out next)))
                filter.Next = next;
        }

        public static DTE GetDTE()
        {
            return _dte;
        }
    }

    class CommandFilter : IOleCommandTarget
    {
        List<Commands.ICommand> _commandList = new List<Commands.ICommand>();

        public CommandFilter()
        {
            _commandList.Add(new Commands.DocumentThis());
            _commandList.Add(new Commands.NamingRuleChecker());
        }

        internal IOleCommandTarget Next { get; set; }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup != GuidList.guidSToolsCmdSet) return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            Commands.ICommand command = _commandList.Find(c => c.IsYourId(nCmdID));
            Debug.Assert(command != null);
            
            command.Execute(VsTextViewCreationListener.GetDTE());
            return VSConstants.S_OK;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup != GuidList.guidSToolsCmdSet) return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
            Commands.ICommand command = _commandList.Find(c => c.IsYourId(prgCmds[0].cmdID));
            Debug.Assert(command != null);
            if (command.IsEnable(VsTextViewCreationListener.GetDTE()))
            {
                prgCmds[0].cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
            }
            return VSConstants.S_OK;
        }
    }
}