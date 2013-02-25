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
        IVsEditorAdaptersFactoryService AdaptersFactory = null;
        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }
        [Import]
        internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        static DTE _dte = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            _dte = (DTE)ServiceProvider.GetService(typeof(DTE));

            var wpfTextView = AdaptersFactory.GetWpfTextView(textViewAdapter);
            if (wpfTextView == null)
            {
                Debug.Fail("Unable to get IWpfTextView from text view adapter");
                return;
            }

            CommandFilter filter = new CommandFilter(wpfTextView);

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
        IWpfTextView _view;
        List<Commands.ICommand> _commandList = new List<Commands.ICommand>();

        public CommandFilter(IWpfTextView view)
        {
            _view = view;
            _commandList.Add(new Commands.DocumentThis(_view));
        }

        internal IOleCommandTarget Next { get; set; }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup != GuidList.guidSToolsCmdSet) return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            Commands.ICommand command = _commandList.Find(c => c.IsYourId(nCmdID));
            Debug.Assert(command != null);
            
            SToolsPackage package = new SToolsPackage();
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