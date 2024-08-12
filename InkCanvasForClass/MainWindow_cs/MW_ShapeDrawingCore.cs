using System;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace Ink_Canvas {

    public partial class MainWindow : Window {

        public StrokeCollection DrawShapeCore(PointCollection pts, ShapeDrawingType type) {
            // 线
            if (type == MainWindow.ShapeDrawingType.Line || 
                type == MainWindow.ShapeDrawingType.DashedLine || 
                type == MainWindow.ShapeDrawingType.DottedLine) {
                if (pts.Count != 2) throw new Exception("传入的点个数不是2个");
                var stk = new IccStroke(new StylusPointCollection() {
                    new StylusPoint(pts[0].X, pts[0].Y),
                    new StylusPoint(pts[1].X, pts[1].Y),
                }, inkCanvas.DefaultDrawingAttributes.Clone());
                stk.AddPropertyData(IccStroke.StrokeIsShapeGuid, true);
                stk.AddPropertyData(IccStroke.StrokeShapeTypeGuid, (int)type);
                return new StrokeCollection() { stk };
            }

            return new StrokeCollection();
        }
    }
}
