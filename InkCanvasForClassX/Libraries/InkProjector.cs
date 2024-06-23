using System;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace InkCanvasForClassX.Libraries
{
    /// <summary>
    /// 該控件提供一個基於<c>DrawingVisual</c>的最小的<c>StrokeCollection</c>渲染控件，用以替代重量級的<c>InkCanvas</c>控件
    /// </summary>
    public class InkProjector : FrameworkElement {
        private VisualCollection _children;
        private DrawingVisual _layer = new DrawingVisual();
        private StrokeCollection _strokes;

        public InkProjector()
        {
            _children = new VisualCollection(this) {
                _layer // 初始化DrawingVisual
            };
        }

        public StrokeCollection Strokes {
            get => _strokes;
            set {
                _strokes = value;
                DrawInk();
            }
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index) {
            if (index < 0 || index >= _children.Count) throw new ArgumentOutOfRangeException();
            return _children[index];
        }

        private void DrawInk() {
            DrawingContext context = _layer.RenderOpen();
            _strokes.Draw(context);
            context.Close();
        }

    }
}
