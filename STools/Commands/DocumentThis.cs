using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace S2.STools.Commands
{
    class DocumentThis : ICommand
    {
        IWpfTextView _view;

        public DocumentThis(IWpfTextView view)
        {
            _view = view;
        }

        public bool IsYourId(uint commandId)
        {
            return commandId == PkgCmdIDList.CommandIdDocumentThis;
        }

        public bool IsEnable()
        {
            return _view.Caret.Position.BufferPosition.GetContainingLine().GetText().Contains("=");
        }

        public void Execute()
        {
            AlignAssignments();
        }

        private void AlignAssignments()
        {
            // Find all lines above and below with = signs
            ITextSnapshot snapshot = _view.TextSnapshot;

            if (snapshot != snapshot.TextBuffer.CurrentSnapshot)
                return;

            int currentLineNumber = snapshot.GetLineNumberFromPosition(_view.Caret.Position.BufferPosition);

            Dictionary<int, ColumnAndOffset> lineNumberToEqualsColumn = new Dictionary<int, ColumnAndOffset>();

            // Start with the current line
            ColumnAndOffset columnAndOffset = GetColumnNumberOfFirstEquals(snapshot.GetLineFromLineNumber(currentLineNumber));
            if (columnAndOffset.Column == -1)
                return;

            lineNumberToEqualsColumn[currentLineNumber] = columnAndOffset;

            int lineNumber = currentLineNumber;
            int minLineNumber = 0;
            int maxLineNumber = snapshot.LineCount - 1;

            // If the selection spans multiple lines, only attempt to fix the lines in the selection
            if (!_view.Selection.IsEmpty)
            {
                var selectionStartLine = _view.Selection.Start.Position.GetContainingLine();
                if (_view.Selection.End.Position > selectionStartLine.End)
                {
                    minLineNumber = selectionStartLine.LineNumber;
                    maxLineNumber = snapshot.GetLineNumberFromPosition(_view.Selection.End.Position);
                }
            }

            // Moving backwards
            for (lineNumber = currentLineNumber - 1; lineNumber >= minLineNumber; lineNumber--)
            {
                columnAndOffset = GetColumnNumberOfFirstEquals(snapshot.GetLineFromLineNumber(lineNumber));
                if (columnAndOffset.Column == -1)
                    break;

                lineNumberToEqualsColumn[lineNumber] = columnAndOffset;
            }

            // Moving forwards
            for (lineNumber = currentLineNumber + 1; lineNumber <= maxLineNumber; lineNumber++)
            {
                columnAndOffset = GetColumnNumberOfFirstEquals(snapshot.GetLineFromLineNumber(lineNumber));
                if (columnAndOffset.Column == -1)
                    break;

                lineNumberToEqualsColumn[lineNumber] = columnAndOffset;
            }

            // Perform the actual edit
            if (lineNumberToEqualsColumn.Count > 1)
            {
                int columnToIndentTo = lineNumberToEqualsColumn.Values.Max(c => c.Column);

                using (var edit = snapshot.TextBuffer.CreateEdit())
                {
                    foreach (var pair in lineNumberToEqualsColumn.Where(p => p.Value.Column < columnToIndentTo))
                    {
                        ITextSnapshotLine line = snapshot.GetLineFromLineNumber(pair.Key);
                        string spaces = new string(' ', columnToIndentTo - pair.Value.Column);

                        if (!edit.Insert(line.Start.Position + pair.Value.Offset, spaces))
                            return;
                    }

                    edit.Apply();
                }
            }
        }

        private ColumnAndOffset GetColumnNumberOfFirstEquals(ITextSnapshotLine line)
        {
            ITextSnapshot snapshot = line.Snapshot;
            int tabSize = _view.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);

            int column = 0;
            int nonWhiteSpaceCount = 0;
            for (int i = line.Start.Position; i < line.End.Position; i++)
            {
                char ch = snapshot[i];
                if (ch == '=')
                    return new ColumnAndOffset()
                    {
                        Column = column,
                        Offset = (i - line.Start.Position) - nonWhiteSpaceCount
                    };

                // For the sake of associating characters with the '=', include only 
                if (!CharAssociatesWithEquals(ch))
                    nonWhiteSpaceCount = 0;
                else
                    nonWhiteSpaceCount++;

                if (ch == '\t')
                    column += tabSize - (column % tabSize);
                else
                    column++;

                // Also, check to see if this is a surrogate pair.  If so, skip the next character by incrementing
                // the loop counter and increment the nonWhiteSpaceCount without incrementing the column
                // count.
                if (char.IsHighSurrogate(ch) &&
                    i < line.End.Position - 1 && char.IsLowSurrogate(snapshot[i + 1]))
                {
                    nonWhiteSpaceCount++;
                    i++;
                }
            }

            return new ColumnAndOffset() { Column = -1, Offset = -1 };
        }

        struct ColumnAndOffset
        {
            public int Column;
            public int Offset;
        }

        static HashSet<char> charsThatAssociateWithEquals = new HashSet<char>() { '+', '-', '.', '<', '>', '/', ':', '\\', '*', '&', '^', '%', '$', '#', '@', '!', '~' };
        private bool CharAssociatesWithEquals(char ch)
        {
            return charsThatAssociateWithEquals.Contains(ch);
        }
    }
}
