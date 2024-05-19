﻿using Ink_Canvas.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using static System.Windows.Forms.AxHost;
using Point = System.Windows.Point;

namespace Ink_Canvas
{
    public partial class MainWindow : Window
    {
        #region Multi-Touch

        bool isInMultiTouchMode = false;
        private void BorderMultiTouchMode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isInMultiTouchMode)
            {
                inkCanvas.StylusDown -= MainWindow_StylusDown;
                inkCanvas.StylusMove -= MainWindow_StylusMove;
                inkCanvas.StylusUp -= MainWindow_StylusUp;
                inkCanvas.TouchDown -= MainWindow_TouchDown;
                inkCanvas.TouchDown += Main_Grid_TouchDown;
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                inkCanvas.Children.Clear();
                isInMultiTouchMode = false;
            }
            else
            {
                inkCanvas.StylusDown += MainWindow_StylusDown;
                inkCanvas.StylusMove += MainWindow_StylusMove;
                inkCanvas.StylusUp += MainWindow_StylusUp;
                inkCanvas.TouchDown += MainWindow_TouchDown;
                inkCanvas.TouchDown -= Main_Grid_TouchDown;
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
                inkCanvas.Children.Clear();
                isInMultiTouchMode = true;
            }
        }

        private void MainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            if (inkCanvas.EditingMode == InkCanvasEditingMode.EraseByPoint
                || inkCanvas.EditingMode == InkCanvasEditingMode.EraseByStroke
                || inkCanvas.EditingMode == InkCanvasEditingMode.Select) return;

            if (!isHidingSubPanelsWhenInking)
            {
                isHidingSubPanelsWhenInking = true;
                HideSubPanels(); // 书写时自动隐藏二级菜单
            }

            double boundWidth = e.GetTouchPoint(null).Bounds.Width, eraserMultiplier = 1.0;
            if (!Settings.Advanced.EraserBindTouchMultiplier && Settings.Advanced.IsSpecialScreen)
            {
                eraserMultiplier = 1 / Settings.Advanced.TouchMultiplier;
            }

            if ((Settings.Advanced.TouchMultiplier != 0 || !Settings.Advanced.IsSpecialScreen) //启用特殊屏幕且触摸倍数为 0 时禁用橡皮
                && (boundWidth > BoundsWidth * 2.5))
            {
                if (drawingShapeMode == 0 && forceEraser) return;
                double k = 1;
                switch (Settings.Canvas.EraserSize)
                {
                    case 0:
                        k = 0.5;
                        break;
                    case 1:
                        k = 0.8;
                        break;
                    case 3:
                        k = 1.25;
                        break;
                    case 4:
                        k = 1.8;
                        break;
                }
                inkCanvas.EraserShape = new EllipseStylusShape(boundWidth * k * eraserMultiplier * 0.25, boundWidth * k * eraserMultiplier * 0.25);
                TouchDownPointsList[e.TouchDevice.Id] = InkCanvasEditingMode.EraseByPoint;
                inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            }
            else
            {
                TouchDownPointsList[e.TouchDevice.Id] = InkCanvasEditingMode.None;
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
            }
        }

        private void MainWindow_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (inkCanvas.EditingMode == InkCanvasEditingMode.EraseByPoint
                || inkCanvas.EditingMode == InkCanvasEditingMode.EraseByStroke
                || inkCanvas.EditingMode == InkCanvasEditingMode.Select) return;

            TouchDownPointsList[e.StylusDevice.Id] = InkCanvasEditingMode.None;
        }

        private async void MainWindow_StylusUp(object sender, StylusEventArgs e)
        {
            try
            {
                inkCanvas.Strokes.Add(GetStrokeVisual(e.StylusDevice.Id).Stroke);
                await Task.Delay(5); // 避免渲染墨迹完成前预览墨迹被删除导致墨迹闪烁
                inkCanvas.Children.Remove(GetVisualCanvas(e.StylusDevice.Id));

                inkCanvas_StrokeCollected(inkCanvas, new InkCanvasStrokeCollectedEventArgs(GetStrokeVisual(e.StylusDevice.Id).Stroke));
            }
            catch (Exception ex)
            {
                Label.Content = ex.ToString();
            }
            try
            {
                StrokeVisualList.Remove(e.StylusDevice.Id);
                VisualCanvasList.Remove(e.StylusDevice.Id);
                TouchDownPointsList.Remove(e.StylusDevice.Id);
                if (StrokeVisualList.Count == 0 || VisualCanvasList.Count == 0 || TouchDownPointsList.Count == 0)
                {
                    inkCanvas.Children.Clear();
                    StrokeVisualList.Clear();
                    VisualCanvasList.Clear();
                    TouchDownPointsList.Clear();
                }
            }
            catch { }
        }

        private void MainWindow_StylusMove(object sender, StylusEventArgs e)
        {
            try
            {
                if (GetTouchDownPointsList(e.StylusDevice.Id) != InkCanvasEditingMode.None) return;
                try
                {
                    if (e.StylusDevice.StylusButtons[1].StylusButtonState == StylusButtonState.Down) return;
                }
                catch { }
                var strokeVisual = GetStrokeVisual(e.StylusDevice.Id);
                var stylusPointCollection = e.GetStylusPoints(this);
                foreach (var stylusPoint in stylusPointCollection)
                {
                    strokeVisual.Add(new StylusPoint(stylusPoint.X, stylusPoint.Y, stylusPoint.PressureFactor));
                }
                strokeVisual.Redraw();
            }
            catch { }
        }

        private StrokeVisual GetStrokeVisual(int id)
        {
            if (StrokeVisualList.TryGetValue(id, out var visual))
            {
                return visual;
            }

            var strokeVisual = new StrokeVisual(inkCanvas.DefaultDrawingAttributes.Clone());
            StrokeVisualList[id] = strokeVisual;
            StrokeVisualList[id] = strokeVisual;
            var visualCanvas = new VisualCanvas(strokeVisual);
            VisualCanvasList[id] = visualCanvas;
            inkCanvas.Children.Add(visualCanvas);

            return strokeVisual;
        }

        private VisualCanvas GetVisualCanvas(int id)
        {
            if (VisualCanvasList.TryGetValue(id, out var visualCanvas))
            {
                return visualCanvas;
            }
            return null;
        }

        private InkCanvasEditingMode GetTouchDownPointsList(int id)
        {
            if (TouchDownPointsList.TryGetValue(id, out var inkCanvasEditingMode))
            {
                return inkCanvasEditingMode;
            }
            return inkCanvas.EditingMode;
        }

        private Dictionary<int, InkCanvasEditingMode> TouchDownPointsList { get; } = new Dictionary<int, InkCanvasEditingMode>();
        private Dictionary<int, StrokeVisual> StrokeVisualList { get; } = new Dictionary<int, StrokeVisual>();
        private Dictionary<int, VisualCanvas> VisualCanvasList { get; } = new Dictionary<int, VisualCanvas>();

        #endregion



        int lastTouchDownTime = 0, lastTouchUpTime = 0;

        Point iniP = new Point(0, 0);
        bool isLastTouchEraser = false;
        private bool forcePointEraser = true;

        private void Main_Grid_TouchDown(object sender, TouchEventArgs e)
        {
            if (!isHidingSubPanelsWhenInking)
            {
                isHidingSubPanelsWhenInking = true;
                HideSubPanels(); // 书写时自动隐藏二级菜单
            }

            if (NeedUpdateIniP())
            {
                iniP = e.GetTouchPoint(inkCanvas).Position;
            }
            if (drawingShapeMode == 9 && isFirstTouchCuboid == false)
            {
                MouseTouchMove(iniP);
            }
            inkCanvas.Opacity = 1;
            double boundsWidth = GetTouchBoundWidth(e), eraserMultiplier = 1.0;
            if (!Settings.Advanced.EraserBindTouchMultiplier && Settings.Advanced.IsSpecialScreen)
            {
                eraserMultiplier = 1 / Settings.Advanced.TouchMultiplier;
            }
            if (boundsWidth > BoundsWidth)
            {
                isLastTouchEraser = true;
                if (drawingShapeMode == 0 && forceEraser) return;
                if (boundsWidth > BoundsWidth * 2.5)
                {
                    double k = 1;
                    switch (Settings.Canvas.EraserSize)
                    {
                        case 0:
                            k = 0.5;
                            break;
                        case 1:
                            k = 0.8;
                            break;
                        case 3:
                            k = 1.25;
                            break;
                        case 4:
                            k = 1.8;
                            break;
                    }
                    inkCanvas.EraserShape = new EllipseStylusShape(boundsWidth * k * eraserMultiplier, boundsWidth * k * eraserMultiplier);
                    inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                }
                else
                {
                    if (StackPanelPPTControls.Visibility == Visibility.Visible && inkCanvas.Strokes.Count == 0 && Settings.PowerPointSettings.IsEnableFingerGestureSlideShowControl)
                    {
                        isLastTouchEraser = false;
                        inkCanvas.EditingMode = InkCanvasEditingMode.GestureOnly;
                        inkCanvas.Opacity = 0.1;
                    }
                    else
                    {
                        inkCanvas.EraserShape = new EllipseStylusShape(5, 5);
                        inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    }
                }
            }
            else
            {
                isLastTouchEraser = false;
                inkCanvas.EraserShape = forcePointEraser ? new EllipseStylusShape(50, 50) : new EllipseStylusShape(5, 5);
                if (forceEraser) return;
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            }
        }

        public double GetTouchBoundWidth(TouchEventArgs e)
        {
            var args = e.GetTouchPoint(null).Bounds;
            double value;
            if (!Settings.Advanced.IsQuadIR) value = args.Width;
            else value = Math.Sqrt(args.Width * args.Height); //四边红外
            if (Settings.Advanced.IsSpecialScreen) value *= Settings.Advanced.TouchMultiplier;
            return value;
        }

        //记录触摸设备ID
        private List<int> dec = new List<int>();
        //中心点
        System.Windows.Point centerPoint;
        InkCanvasEditingMode lastInkCanvasEditingMode = InkCanvasEditingMode.Ink;
        bool isSingleFingerDragMode = false;

        private void inkCanvas_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            dec.Add(e.TouchDevice.Id);
            //设备1个的时候，记录中心点
            if (dec.Count == 1)
            {
                TouchPoint touchPoint = e.GetTouchPoint(inkCanvas);
                centerPoint = touchPoint.Position;

                //记录第一根手指点击时的 StrokeCollection
                lastTouchDownStrokeCollection = inkCanvas.Strokes.Clone();
            }
            //设备两个及两个以上，将画笔功能关闭
            if (dec.Count > 1 || isSingleFingerDragMode || !Settings.Gesture.IsEnableTwoFingerGesture)
            {
                if (isInMultiTouchMode || !Settings.Gesture.IsEnableTwoFingerGesture) return;
                if (inkCanvas.EditingMode != InkCanvasEditingMode.None && inkCanvas.EditingMode != InkCanvasEditingMode.Select)
                {
                    lastInkCanvasEditingMode = inkCanvas.EditingMode;
                    inkCanvas.EditingMode = InkCanvasEditingMode.None;
                }
            }
        }

        private void inkCanvas_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            //手势完成后切回之前的状态
            if (dec.Count > 1)
            {
                if (inkCanvas.EditingMode == InkCanvasEditingMode.None)
                {
                    inkCanvas.EditingMode = lastInkCanvasEditingMode;
                }
            }
            dec.Remove(e.TouchDevice.Id);
            inkCanvas.Opacity = 1;
            if (dec.Count == 0)
            {
                if (lastTouchDownStrokeCollection.Count() != inkCanvas.Strokes.Count() &&
                    !(drawingShapeMode == 9 && !isFirstTouchCuboid))
                {
                    int whiteboardIndex = CurrentWhiteboardIndex;
                    if (currentMode == 0)
                    {
                        whiteboardIndex = 0;
                    }
                    strokeCollections[whiteboardIndex] = lastTouchDownStrokeCollection;
                }
            }
        }

        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs args)
        {
            Trace.WriteLine("Strokes_StrokesChanged");
        }

        private void StylusPointsChanged(object sender, EventArgs e)
        {
            ((StylusPointCollection)sender).Changed -= StylusPointsChanged;
            FakeInkCanvas.Visibility = Visibility.Hidden;
            inkCanvas.Opacity = 1;
        }

        private void inkCanvas_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.Mode = ManipulationModes.All;
        }

        // 指定是否鎖定Manipulation，鎖定後ManipulationDelta不會觸發RenderTransform。
        // 當雙指操作開始慣性時鎖定，並開始應用變換，Manipulation結束後取消鎖定。
        bool isLockManipulation = false;

        // 可能不會發生Inertia事件，這裡指示是否發生inertia事件
        bool isInertiaEventFired = false;

        private void ExecuteManipulationTransform()
        {
            double TranslateX = FakeICTranslateTransform.X;
            double TranslateY = FakeICTranslateTransform.Y;
            double ScaleX = FakeICScaleTransform.ScaleX;
            double ScaleY = FakeICScaleTransform.ScaleY;
            double CenterX = FakeICScaleTransform.CenterX;
            double CenterY = FakeICScaleTransform.CenterY;

            Matrix m1 = new Matrix();
            //Matrix m2 = new Matrix();
            Matrix m3 = new Matrix();
            Matrix mfinal = new Matrix();
            m1.Translate(TranslateX, TranslateY);
            //m2.RotateAt(FakeICRotateTransform.Angle, FakeICRotateTransform.CenterX, FakeICRotateTransform.CenterY);
            m3.ScaleAt(ScaleX, ScaleY, CenterX, CenterY);
            mfinal.Prepend(m1);
            //mfinal.Prepend(m2);
            mfinal.Prepend(m3);

            foreach (Stroke stroke in inkCanvas.Strokes)
            {
                if (inkCanvas.Strokes.IndexOf(stroke) == inkCanvas.Strokes.Count - 1)
                {
                    stroke.StylusPoints.Changed += StylusPointsChanged;
                }
                stroke.Transform(mfinal, false);
                try
                {
                    stroke.DrawingAttributes.Width *= FakeICScaleTransform.ScaleX;
                    stroke.DrawingAttributes.Height *= FakeICScaleTransform.ScaleY;
                }
                catch { }
            };

            FakeICTranslateTransform.X = 0;
            FakeICTranslateTransform.Y = 0;
            FakeICRotateTransform.Angle = 0;
            FakeICRotateTransform.CenterX = 0;
            FakeICRotateTransform.CenterY = 0;
            FakeICScaleTransform.ScaleX = 1;
            FakeICScaleTransform.ScaleY = 1;
            FakeICScaleTransform.CenterX = 0;
            FakeICScaleTransform.CenterY = 0;

            TTT.Text = $"tx:{FakeICTranslateTransform.X}\n" +
                $"ty:{FakeICTranslateTransform.Y}\n" +
                $"ssx:{FakeICScaleTransform.ScaleX}\n" +
                $"ssy:{FakeICScaleTransform.ScaleY}\n" +
                $"scx:{FakeICScaleTransform.CenterX}\n" +
                $"scy:{FakeICScaleTransform.CenterY}";
        }

        private void inkCanvas_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            if (e.Manipulators.Count() == 0 && FakeICTranslateTransform.X != 0 && FakeICTranslateTransform.Y != 0)
            {
                isLockManipulation = true;
                isInertiaEventFired = true;

                ExecuteManipulationTransform();
            }
        }

        private void StylusPoints_Changed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Main_Grid_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (e.Manipulators.Count() == 0)
            {

                if (isInertiaEventFired == false)
                {
                    ExecuteManipulationTransform();
                }

                if (forceEraser) return;
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            }
        }

        private void inkCanvas_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (isInMultiTouchMode || !Settings.Gesture.IsEnableTwoFingerGesture || inkCanvas.Strokes.Count == 0 || dec.Count() < 2) return;
            _currentCommitType = CommitReason.Manipulation;
            StrokeCollection strokes = inkCanvas.GetSelectedStrokes();
            if (strokes.Count != 0)
            {
                inkCanvas.Strokes.Replace(strokes, strokes.Clone());
            }
            else
            {
                var originalStrokes = inkCanvas.Strokes;
                var targetStrokes = originalStrokes.Clone();
                originalStrokes.Replace(originalStrokes, targetStrokes);
            }
            _currentCommitType = CommitReason.UserInput;

        }

        private void Main_Grid_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (isInMultiTouchMode || !Settings.Gesture.IsEnableTwoFingerGesture) return;
            if ((dec.Count >= 2 && (Settings.PowerPointSettings.IsEnableTwoFingerGestureInPresentationMode || StackPanelPPTControls.Visibility != Visibility.Visible || StackPanelPPTButtons.Visibility == Visibility.Collapsed)) || isSingleFingerDragMode)
            {

                inkCanvas.Opacity = 0;
                FakeInkCanvas.Visibility = Visibility.Visible;

                ManipulationDelta md = e.DeltaManipulation;
                Vector trans = md.Translation;  // 获得位移矢量

                Matrix m = new Matrix();

                if (Settings.Gesture.IsEnableTwoFingerTranslate)
                    m.Translate(trans.X, trans.Y);  // 移动

                if (Settings.Gesture.IsEnableTwoFingerGestureTranslateOrRotation)
                {
                    double rotate = md.Rotation;  // 获得旋转角度
                    Vector scale = md.Scale;  // 获得缩放倍数

                    // Find center of element and then transform to get current location of center
                    FrameworkElement fe = e.Source as FrameworkElement;
                    Point center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2);
                    center = m.Transform(center);  // 转换为矩阵缩放和旋转的中心点

                    if (Settings.Gesture.IsEnableTwoFingerRotation)
                        m.RotateAt(rotate, center.X, center.Y);  // 旋转
                    if (Settings.Gesture.IsEnableTwoFingerZoom)
                        m.ScaleAt(scale.X, scale.Y, center.X, center.Y);  // 缩放
                }

                StrokeCollection strokes = inkCanvas.GetSelectedStrokes();
                if (strokes.Count != 0)
                {
                    foreach (Stroke stroke in strokes)
                    {
                        stroke.Transform(m, true);

                        foreach (Circle circle in circles)
                        {
                            if (stroke == circle.Stroke)
                            {
                                circle.R = GetDistance(circle.Stroke.StylusPoints[0].ToPoint(), circle.Stroke.StylusPoints[circle.Stroke.StylusPoints.Count / 2].ToPoint()) / 2;
                                circle.Centroid = new Point((circle.Stroke.StylusPoints[0].X + circle.Stroke.StylusPoints[circle.Stroke.StylusPoints.Count / 2].X) / 2,
                                                            (circle.Stroke.StylusPoints[0].Y + circle.Stroke.StylusPoints[circle.Stroke.StylusPoints.Count / 2].Y) / 2);
                                break;
                            }
                        }

                        if (Settings.Gesture.IsEnableTwoFingerZoom)
                        {
                            try
                            {
                                //stroke.DrawingAttributes.Width *= md.Scale.X;
                                //stroke.DrawingAttributes.Height *= md.Scale.Y;
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    //if (Settings.Gesture.IsEnableTwoFingerZoom)
                    //{
                    //foreach (Stroke stroke in inkCanvas.Strokes)
                    //{
                    //    try
                    //    {
                    //        stroke.DrawingAttributes.Width *= md.Scale.X;
                    //        stroke.DrawingAttributes.Height *= md.Scale.Y;
                    //    }
                    //    catch { }
                    //};

                    // inkCanvas.Strokes.Transform(m, true);

                    //MatrixTransform mt = new MatrixTransform(m);
                    //inkCanvas.RenderTransform = mt;


                    //}
                    //else
                    //{
                    //foreach (Stroke stroke in inkCanvas.Strokes)
                    //{
                    //    stroke.Transform(m, false);
                    //};

                    // inkCanvas.Strokes.Transform(m, true);

                    //TranslateTransform tt = new TranslateTransform(trans.X,trans.Y);
                    //inkCanvas.RenderTransform = tt;
                    //TTT.Text = "transX:" + trans.X + ",transY:" + trans.Y + "!";
                    //}

                    if (Settings.Gesture.IsEnableTwoFingerTranslate && isLockManipulation == false)
                    {
                        FakeICTranslateTransform.X = Math.Round(FakeICTranslateTransform.X + trans.X,4);
                        FakeICTranslateTransform.Y = Math.Round(FakeICTranslateTransform.Y + trans.Y, 4);
                    }
                        

                    if (Settings.Gesture.IsEnableTwoFingerGestureTranslateOrRotation && isLockManipulation == false)
                    {
                        double rotate = md.Rotation;  // 获得旋转角度
                        Vector scale = md.Scale;  // 获得缩放倍数

                        // Find center of element and then transform to get current location of center
                        FrameworkElement fe = e.Source as FrameworkElement;
                        Point center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2);
                        center = m.Transform(center);  // 转换为矩阵缩放和旋转的中心点

                        if (Settings.Gesture.IsEnableTwoFingerRotation)
                        {
                            //FakeICRotateTransform.Angle = Math.Round(FakeICRotateTransform.Angle+rotate,4);
                            //FakeICRotateTransform.CenterX = Math.Round(FakeICRotateTransform.CenterX+center.X,4);
                            //FakeICRotateTransform.CenterY = Math.Round(FakeICRotateTransform.CenterY + center.Y, 4);
                        }

                        if (Settings.Gesture.IsEnableTwoFingerZoom)
                        {
                            FakeICScaleTransform.ScaleX = Math.Round(FakeICScaleTransform.ScaleX+ scale.X -1,4);
                            FakeICScaleTransform.ScaleY = Math.Round(FakeICScaleTransform.ScaleY + scale.Y - 1, 4);
                            FakeICScaleTransform.CenterX = Math.Round(center.X,4);
                            FakeICScaleTransform.CenterY = Math.Round(center.Y, 4);
                        }

                        TTT.Text = $"tx:{FakeICTranslateTransform.X}\n" +
                            $"ty:{FakeICTranslateTransform.Y}\n" +
                            $"ssx:{FakeICScaleTransform.ScaleX}\n" +
                            $"ssy:{FakeICScaleTransform.ScaleY}\n" +
                            $"scx:{FakeICScaleTransform.CenterX}\n" +
                            $"scy:{FakeICScaleTransform.CenterY}";
                    }

                    foreach (Circle circle in circles)
                    {
                        circle.R = GetDistance(circle.Stroke.StylusPoints[0].ToPoint(), circle.Stroke.StylusPoints[circle.Stroke.StylusPoints.Count / 2].ToPoint()) / 2;
                        circle.Centroid = new Point(
                            (circle.Stroke.StylusPoints[0].X + circle.Stroke.StylusPoints[circle.Stroke.StylusPoints.Count / 2].X) / 2,
                            (circle.Stroke.StylusPoints[0].Y + circle.Stroke.StylusPoints[circle.Stroke.StylusPoints.Count / 2].Y) / 2
                        );
                    };
                }
            } 
            //else
            //{
            //    Matrix m = new Matrix();
            //    m.Translate(FakeICTranslateTransform.X, FakeICTranslateTransform.Y);
            //    m.RotateAt(FakeICRotateTransform.Angle,FakeICRotateTransform.CenterX,FakeICRotateTransform.CenterY);
            //    m.ScaleAt(FakeICScaleTransform.ScaleX,FakeICScaleTransform.ScaleY,FakeICScaleTransform.CenterX,FakeICScaleTransform.CenterY);
            //    inkCanvas.Strokes.Transform(m, false);

            //    FakeICTranslateTransform.X = 0;
            //    FakeICTranslateTransform.Y = 0;
            //    FakeICRotateTransform.Angle = 0;
            //    FakeICRotateTransform.CenterX = 0;
            //    FakeICRotateTransform.CenterY = 0;
            //    FakeICScaleTransform.ScaleX = 0;
            //    FakeICScaleTransform.ScaleY = 0;
            //    FakeICScaleTransform.CenterX = 1;
            //    FakeICScaleTransform.CenterY = 1;
            //}
        }
    }
}