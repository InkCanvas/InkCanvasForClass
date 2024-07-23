using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Ink_Canvas.Helpers
{
    
    public class RectangleSelectionViewer : FrameworkElement
    {
        private VisualCollection _children;
        private DrawingVisual _layer = new DrawingVisual();
        private Pen defaultPen = new Pen();

        public RectangleSelectionViewer()
        {
            _children = new VisualCollection(this) {
                _layer // 初始化DrawingVisual
            };
            defaultPen.Thickness = 2;
            defaultPen.Brush = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            defaultPen.DashStyle = DashStyles.Dash;
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count) throw new ArgumentOutOfRangeException();
            return _children[index];
        }

        public void DrawSelectionBox(Rect rect) {
            DrawingContext context = _layer.RenderOpen();
            context.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(78, 96, 165, 250)), defaultPen, rect, 4, 4);
            context.Close();
        }

        public void ClearDrawing() {
            DrawingContext context = _layer.RenderOpen();
            context.Close();
        }
    }
}
