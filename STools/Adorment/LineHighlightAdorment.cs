using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;

namespace LineHighlightAdorment
{
    ///<summary>
    ///LineHighlightAdorment places red boxes behind all the "A"s in the editor window
    ///</summary>
    public class LineHighlightAdorment
    {
        IAdornmentLayer _layer;
        IWpfTextView _view;
        Brush _brush;

        static Dictionary<string, BitVector32> _map = null;

        public static void SetDefinition(Dictionary<string, BitVector32> map)
        {
            _map = map;
        }

        public LineHighlightAdorment(IWpfTextView view)
        {
            _view = view;
            _layer = view.GetAdornmentLayer("LineHighlightAdorment");

            //Listen to any event that changes the layout (text changes, scrolling, etc)
            _view.LayoutChanged += OnLayoutChanged;

            //Create the pen and brush to color the box behind the a's
            Brush brush = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x00, 0xff));
            brush.Freeze();

            _brush = brush;
        }

        /// <summary>
        /// On layout change add the adornment to any reformatted lines
        /// </summary>
        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                this.CreateVisuals(line);
            }
        }

        /// <summary>
        /// Within the given line add the scarlet box behind the a
        /// </summary>
        private void CreateVisuals(ITextViewLine line)
        {
            //grab a reference to the lines in the current TextView 
            IWpfTextViewLineCollection textViewLines = _view.TextViewLines;
            int start = line.Start;
            int end = line.End;

            //Loop through each character, and place a box around any a 
            for (int i = start; (i < end); ++i)
            {
                if (_view.TextSnapshot[i] == 'a')
                {
                    SnapshotSpan span = new SnapshotSpan(_view.TextSnapshot, Span.FromBounds(0, end));
                    Geometry g = textViewLines.GetMarkerGeometry(span);
                    if (g != null)
                    {
                        GeometryDrawing drawing = new GeometryDrawing(_brush, null, g);
                        drawing.Freeze();

                        DrawingImage drawingImage = new DrawingImage(drawing);
                        drawingImage.Freeze();

                        Image image = new Image();
                        image.Source = drawingImage;

                        //Align the image with the top of the bounds of the text geometry
                        Canvas.SetLeft(image, g.Bounds.Left);
                        Canvas.SetTop(image, g.Bounds.Top);

                        _layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
                    }
                }
            }
        }
    }
}
