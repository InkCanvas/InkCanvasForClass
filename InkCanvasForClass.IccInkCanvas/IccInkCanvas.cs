using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using InkCanvasForClass.IccInkCanvas.Settings;

namespace InkCanvasForClass.IccInkCanvas {
    class CustomDynamicRenderer : DynamicRenderer {

        private Point prevPoint;

        private IccInkCanvas _inkCanvas;

        private List<StylusPoint> pointsList = new List<StylusPoint>();

        private void ClearPointsList() {
            pointsList.Clear();
        }

        private void PushPoint(StylusPoint point) {
            pointsList.Add(point);
            if (pointsList.Count > 15) {
                pointsList.RemoveRange(0,pointsList.Count - 15);
            }
        }

        public CustomDynamicRenderer(IccInkCanvas iccInkCanvas) {
            _inkCanvas = iccInkCanvas;
        }

        protected override void OnStylusDown(RawStylusInput rawStylusInput) {
            prevPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
            ClearPointsList();
            var pts = rawStylusInput.GetStylusPoints();
            if (pts.Count == 1) {
                PushPoint(pts.Single());
            } else if (pts.Count > 1) {
                PushPoint(pts[pts.Count-1]);
            }
            Trace.WriteLine(pointsList.Count);
            base.OnStylusDown(rawStylusInput);
        }

        protected override void OnStylusMove(RawStylusInput rawStylusInput) {
            base.OnStylusMove(rawStylusInput);
            var pts = rawStylusInput.GetStylusPoints();
            if (pts.Count == 1) {
                PushPoint(pts.Single());
            } else if (pts.Count > 1) {
                PushPoint(pts[pts.Count-1]);
            }
            Trace.WriteLine(pointsList.Count);
        }

        protected override void OnDraw(DrawingContext drawingContext,
            StylusPointCollection stylusPoints,
            Geometry geometry, Brush fillBrush) {
            if (_inkCanvas.BoardSettings.StrokeNibStyle == StrokeNibStyle.Beautiful) {
                try {
                    var sp = new StylusPointCollection();
                    var n = pointsList.Count - 1;
                    var pressure = 0.1;
                    var x = 10;
                    if (n == 1) return;
                    if (n >= x) {
                        for (var i = 0; i < n - x; i++) {
                            var point = new StylusPoint();

                            point.PressureFactor = (float)0.5;
                            point.X = pointsList[i].X;
                            point.Y = pointsList[i].Y;
                            sp.Add(point);
                        }

                        for (var i = n - x; i <= n; i++) {
                            var point = new StylusPoint();

                            point.PressureFactor = (float)((0.5 - pressure) * (n - i) / x + pressure);
                            point.X = pointsList[i].X;
                            point.Y = pointsList[i].Y;
                            sp.Add(point);
                        }
                    } else {
                        for (var i = 0; i <= n; i++) {
                            var point = new StylusPoint();

                            point.PressureFactor = (float)(0.4 * (n - i) / n + pressure);
                            point.X = pointsList[i].X;
                            point.Y = pointsList[i].Y;
                            sp.Add(point);
                        }
                    }

                    var da = DrawingAttributes.Clone();
                    da.Width -= 0.5;
                    da.Height -= 0.5;
                    var stk = new Stroke(sp, da);
                    stk.Draw(drawingContext);
                } catch {}
            } else {
                base.OnDraw(drawingContext, stylusPoints, geometry, fillBrush);
            }
            
        }
    }

    public class IccInkCanvas : InkCanvas {

        CustomDynamicRenderer customDynamicRenderer;

        public BoardSettings BoardSettings { get; set; } = new BoardSettings();

        public IccInkCanvas() {
            customDynamicRenderer = new CustomDynamicRenderer(this);
            DynamicRenderer = customDynamicRenderer;

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

        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e) {
            IccStroke customStroke = new IccStroke(e.Stroke.StylusPoints, e.Stroke.DrawingAttributes);
            if (e.Stroke is IccStroke) {
                this.Strokes.Add(e.Stroke);
            } else {
                this.Strokes.Remove(e.Stroke);
                this.Strokes.Add(customStroke);
            }

            if (BoardSettings.StrokeNibStyle == StrokeNibStyle.Beautiful) {
                try {
                    var stylusPoints = new StylusPointCollection();
                    var n = customStroke.StylusPoints.Count - 1;
                    var pressure = 0.1;
                    var x = 10;
                    if (n == 1) return;
                    if (n >= x) {
                        for (var i = 0; i < n - x; i++) {
                            var point = new StylusPoint();

                            point.PressureFactor = (float)0.5;
                            point.X = customStroke.StylusPoints[i].X;
                            point.Y = customStroke.StylusPoints[i].Y;
                            stylusPoints.Add(point);
                        }

                        for (var i = n - x; i <= n; i++) {
                            var point = new StylusPoint();

                            point.PressureFactor = (float)((0.5 - pressure) * (n - i) / x + pressure);
                            point.X = customStroke.StylusPoints[i].X;
                            point.Y = customStroke.StylusPoints[i].Y;
                            stylusPoints.Add(point);
                        }
                    }
                    else {
                        for (var i = 0; i <= n; i++) {
                            var point = new StylusPoint();

                            point.PressureFactor = (float)(0.4 * (n - i) / n + pressure);
                            point.X = customStroke.StylusPoints[i].X;
                            point.Y = customStroke.StylusPoints[i].Y;
                            stylusPoints.Add(point);
                        }
                    }

                    customStroke.StylusPoints = stylusPoints;
                } catch { }
            }

            InkCanvasStrokeCollectedEventArgs args =
                new InkCanvasStrokeCollectedEventArgs(customStroke);
            base.OnStrokeCollected(args);
        }

        public event EventHandler<RoutedEventArgs> DeleteKeyCommandFired;
    }
}
