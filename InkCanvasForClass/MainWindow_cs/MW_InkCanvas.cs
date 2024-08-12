using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using iNKORE.UI.WPF.Helpers;

namespace Ink_Canvas {

    public class IccStroke : Stroke {

        public IccStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes)
            : base(stylusPoints, drawingAttributes) { }

        public static Guid StrokeShapeTypeGuid = new Guid("6537b29c-557f-487f-800b-cb30a8f1de78");
        public static Guid StrokeIsShapeGuid = new Guid("40eff5db-9346-4e42-bd46-7b0eb19d0018");

        // 自定义的墨迹渲染
        protected override void DrawCore(DrawingContext drawingContext,
            DrawingAttributes drawingAttributes) {
            if (!(this.ContainsPropertyData(StrokeIsShapeGuid) &&
                  (bool)this.GetPropertyData(StrokeIsShapeGuid) == true)) {
                base.DrawCore(drawingContext, drawingAttributes);
                return;
            }

            if ((int)this.GetPropertyData(StrokeShapeTypeGuid) == (int)MainWindow.ShapeDrawingType.DashedLine ||
                (int)this.GetPropertyData(StrokeShapeTypeGuid) == (int)MainWindow.ShapeDrawingType.DottedLine) {
                StreamGeometry geometry = new StreamGeometry();
                var pts = new List<Point>(this.StylusPoints.ToPoints());
                using (StreamGeometryContext ctx = geometry.Open()) {
                    ctx.BeginFigure(pts[0], false , false);
                    pts.RemoveAt(0);
                    ctx.PolyLineTo(pts,true, true);
                }
                var pen = new Pen(new SolidColorBrush(DrawingAttributes.Color),
                    (drawingAttributes.Width + drawingAttributes.Height) / 2) {
                    DashCap = PenLineCap.Round,
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round,
                    DashStyle = (int)this.GetPropertyData(StrokeShapeTypeGuid) == (int)MainWindow.ShapeDrawingType.DottedLine ? DashStyles.Dot : DashStyles.Dash
                };
                drawingContext.DrawGeometry(new SolidColorBrush(Colors.Transparent),pen, geometry);
            }
            
        }
    }

    public class IccInkCanvas : InkCanvas {
        public IccInkCanvas() {
            // 通过反射移除InkCanvas自带的默认 Delete按键事件
            var commandBindingsField =
                typeof(CommandManager).GetField("_classCommandBindings", BindingFlags.NonPublic | BindingFlags.Static);
            var bnds = commandBindingsField.GetValue(null) as HybridDictionary;
            var inkCanvasBindings = bnds[typeof(InkCanvas)] as CommandBindingCollection;
            var enumerator = inkCanvasBindings.GetEnumerator();
            while (enumerator.MoveNext()) {
                var item = (CommandBinding)enumerator.Current;
                if (item.Command == ApplicationCommands.Delete) {
                    var executedField =
                        typeof(CommandBinding).GetField("Executed", BindingFlags.NonPublic | BindingFlags.Instance);
                    var canExecuteField =
                        typeof(CommandBinding).GetField("CanExecute", BindingFlags.NonPublic | BindingFlags.Instance);
                    executedField.SetValue(item, new ExecutedRoutedEventHandler((sender, args) => { }));
                    canExecuteField.SetValue(item, new CanExecuteRoutedEventHandler((sender, args) => { }));
                }
            }

            // 为IccInkCanvas注册自定义的 Delete按键Command并Invoke OnDeleteCommandFired。
            CommandManager.RegisterClassCommandBinding(typeof(IccInkCanvas), new CommandBinding(ApplicationCommands.Delete,
                (sender, args) => {
                    DeleteKeyCommandFired?.Invoke(this, new RoutedEventArgs());
                }, (sender, args) => {
                    args.CanExecute = GetSelectedStrokes().Count != 0;
                }));
        }

        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            // Remove the original stroke and add a custom stroke.
            this.Strokes.Remove(e.Stroke);
            IccStroke customStroke = new IccStroke(e.Stroke.StylusPoints, e.Stroke.DrawingAttributes);
            this.Strokes.Add(customStroke);

            // Pass the custom stroke to base class' OnStrokeCollected method.
            InkCanvasStrokeCollectedEventArgs args =
                new InkCanvasStrokeCollectedEventArgs(customStroke);
            base.OnStrokeCollected(args);
        }

        public event EventHandler<RoutedEventArgs> DeleteKeyCommandFired;
    }
}