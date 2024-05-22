using Ink_Canvas.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using iNKORE.UI.WPF.Modern;
using System.Threading;
using Application = System.Windows.Application;
using Point = System.Windows.Point;
using System.Diagnostics;
using iNKORE.UI.WPF.Modern.Controls;
using System.IO;
using System.Windows.Media.Effects;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Xml.Linq;

namespace Ink_Canvas
{

    public partial class MainWindow : Window
    {
        #region TwoFingZoomBtn



        private void TwoFingerGestureBorder_MouseUp(object sender, RoutedEventArgs e)
        {
            
            if (TwoFingerGestureBorder.Visibility == Visibility.Visible)
            {
                AnimationsHelper.HideWithSlideAndFade(TwoFingerGestureBorder);
                AnimationsHelper.HideWithSlideAndFade(BoardTwoFingerGestureBorder);
            }
            else
            {
                AnimationsHelper.ShowWithSlideFromBottomAndFade(TwoFingerGestureBorder);
                AnimationsHelper.ShowWithSlideFromBottomAndFade(BoardTwoFingerGestureBorder);
            }
        }

        private void CheckEnableTwoFingerGestureBtnColorPrompt()
        {
            if (ToggleSwitchEnableMultiTouchMode.IsOn)
            {
                TwoFingerGestureSimpleStackPanel.Opacity = 0.5;
                TwoFingerGestureSimpleStackPanel.IsHitTestVisible = false;
                EnableTwoFingerGestureBtn.Source = new BitmapImage(new Uri("/Resources/new-icons/gesture.png", UriKind.Relative));
                BoardEnableTwoFingerGestureBtn.Source = new BitmapImage(new Uri("/Resources/new-icons/gesture.png", UriKind.Relative));
            }
            else
            {
                TwoFingerGestureSimpleStackPanel.Opacity = 1;
                TwoFingerGestureSimpleStackPanel.IsHitTestVisible = true;
                if (Settings.Gesture.IsEnableTwoFingerGesture)
                {
                    EnableTwoFingerGestureBtn.Source = new BitmapImage(new Uri("/Resources/new-icons/gesture-enabled.png", UriKind.Relative));
                    BoardEnableTwoFingerGestureBtn.Source = new BitmapImage(new Uri("/Resources/new-icons/gesture-enabled.png", UriKind.Relative));
                }
                else
                {
                    EnableTwoFingerGestureBtn.Source = new BitmapImage(new Uri("/Resources/new-icons/gesture.png", UriKind.Relative));
                    BoardEnableTwoFingerGestureBtn.Source = new BitmapImage(new Uri("/Resources/new-icons/gesture.png", UriKind.Relative));
                }
            }
        }

        private void CheckEnableTwoFingerGestureBtnVisibility(bool isVisible)
        {
            if (StackPanelCanvasControls.Visibility != Visibility.Visible
                || BorderFloatingBarMainControls.Visibility != Visibility.Visible)
            {
                EnableTwoFingerGestureBorder.Visibility = Visibility.Collapsed;
            }
            else if (isVisible == true)
            {
                if (BtnPPTSlideShowEnd.Visibility == Visibility.Visible) EnableTwoFingerGestureBorder.Visibility = Visibility.Collapsed;
                else EnableTwoFingerGestureBorder.Visibility = Visibility.Visible;
            }
            else EnableTwoFingerGestureBorder.Visibility = Visibility.Collapsed;
        }

        #endregion TwoFingZoomBtn

        #region Drag

        bool isDragDropInEffect = false;
        Point pos = new Point();
        Point downPos = new Point();
        Point pointDesktop = new Point(-1, -1); //用于记录上次在桌面时的坐标
        Point pointPPT = new Point(-1, -1); //用于记录上次在PPT中的坐标

        void SymbolIconEmoji_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragDropInEffect)
            {
                double xPos = e.GetPosition(null).X - pos.X + ViewboxFloatingBar.Margin.Left;
                double yPos = e.GetPosition(null).Y - pos.Y + ViewboxFloatingBar.Margin.Top;
                ViewboxFloatingBar.Margin = new Thickness(xPos, yPos, -2000, -200);

                pos = e.GetPosition(null);
                if (BtnPPTSlideShowEnd.Visibility == Visibility.Visible)
                {
                    pointPPT = new Point(xPos, yPos);
                }
                else
                {
                    pointDesktop = new Point(xPos, yPos);
                }
            }
        }

        void SymbolIconEmoji_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isViewboxFloatingBarMarginAnimationRunning)
            {
                ViewboxFloatingBar.BeginAnimation(FrameworkElement.MarginProperty, null);
                isViewboxFloatingBarMarginAnimationRunning = false;
            }
            isDragDropInEffect = true;
            pos = e.GetPosition(null);
            downPos = e.GetPosition(null);
            GridForFloatingBarDraging.Visibility = Visibility.Visible;
        }

        void SymbolIconEmoji_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragDropInEffect = false;

            if (e is null || Math.Abs(downPos.X - e.GetPosition(null).X) <= 10 && Math.Abs(downPos.Y - e.GetPosition(null).Y) <= 10)
            {
                if (BorderFloatingBarMainControls.Visibility == Visibility.Visible)
                {
                    BorderFloatingBarMainControls.Visibility = Visibility.Collapsed;
                    CheckEnableTwoFingerGestureBtnVisibility(false);
                }
                else
                {
                    BorderFloatingBarMainControls.Visibility = Visibility.Visible;
                    CheckEnableTwoFingerGestureBtnVisibility(true);
                }
            }

            GridForFloatingBarDraging.Visibility = Visibility.Collapsed;
        }

        #endregion

        private void HideSubPanelsImmediately()
        {
            BorderTools.Visibility = Visibility.Collapsed;
            BorderTools.Visibility = Visibility.Collapsed;
            BoardBorderTools.Visibility = Visibility.Collapsed;
            PenPalette.Visibility = Visibility.Collapsed;
            BoardPenPalette.Visibility = Visibility.Collapsed;
            BoardDeleteIcon.Visibility = Visibility.Collapsed;
            BorderSettings.Visibility = Visibility.Collapsed;
        }

        #region 按鈕高亮背景
        private async void HideSubPanels(String mode = null, bool autoAlignCenter = false)
        {
            AnimationsHelper.HideWithSlideAndFade(BorderTools);
            AnimationsHelper.HideWithSlideAndFade(BoardBorderTools);
            AnimationsHelper.HideWithSlideAndFade(PenPalette);
            AnimationsHelper.HideWithSlideAndFade(BoardPenPalette);
            AnimationsHelper.HideWithSlideAndFade(BoardDeleteIcon);

            if (BorderSettings.Visibility == Visibility.Visible)
            {
                // hide settings panel using new animation
                BorderSettingsMask.IsHitTestVisible = false;
                BorderSettingsMask.Background = null;
                var sb = new Storyboard();

                var slideAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = BorderSettings.RenderTransform.Value.OffsetX - 440,
                    Duration = TimeSpan.FromSeconds(0.6)
                };
                slideAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

                sb.Children.Add(slideAnimation);

                sb.Completed += (s, _) =>
                {
                    BorderSettings.Visibility = Visibility.Collapsed;
                };

                BorderSettings.Visibility = Visibility.Visible;
                BorderSettings.RenderTransform = new TranslateTransform();

                sb.Begin((FrameworkElement)BorderSettings);
            }
            
            AnimationsHelper.HideWithSlideAndFade(TwoFingerGestureBorder);
            AnimationsHelper.HideWithSlideAndFade(EraserSizePanel);
            AnimationsHelper.HideWithSlideAndFade(BoardTwoFingerGestureBorder);
            if (ToggleSwitchDrawShapeBorderAutoHide.IsOn)
            {
                AnimationsHelper.HideWithSlideAndFade(BorderDrawShape);
                AnimationsHelper.HideWithSlideAndFade(BoardBorderDrawShape);
            }

            if (mode != null)
            {
                
                if (mode != "clear")
                {
                    Pen_Icon.Background = null;
                    BoardPen.Background = new SolidColorBrush(Colors.LightGray);
                    Eraser_Icon.Background = null;
                    BoardEraser.Background = new SolidColorBrush(Colors.LightGray);
                    SymbolIconSelect.Background = null;
                    BoardSelect.Background = new SolidColorBrush(Colors.LightGray);
                    EraserByStrokes_Icon.Background = null;
                    BoardEraserByStrokes.Background = new SolidColorBrush(Colors.LightGray);

                    ImageSource cursorSolidIS = new BitmapImage(new Uri("/Resources/new-icons/cursor-solid.png", UriKind.Relative));
                    ImageSource penLinedIS = new BitmapImage(new Uri("/Resources/new-icons/pen-lined.png", UriKind.Relative));
                    ImageSource strokeEraserLinedIS = new BitmapImage(new Uri("/Resources/new-icons/eraser-lined.png", UriKind.Relative));
                    ImageSource circleEraserLinedIS = new BitmapImage(new Uri("/Resources/new-icons/circle-eraser-lined.png", UriKind.Relative));
                    ImageSource selectLinedIS = new BitmapImage(new Uri("/Resources/new-icons/lasso-select-lined.png", UriKind.Relative));
                    
                    // 修改鼠标icon为solid
                    CursorToolbarIconImage.Source = cursorSolidIS;
                    // 修改批注icon为lined
                    PenIcon.Source = penLinedIS;
                    // 修改笔记擦icon为lined
                    StrokeEraserToolbarIconImage.Source = strokeEraserLinedIS;
                    // 修改擦icon为lined
                    CircleEraserToolbarIconImage.Source = circleEraserLinedIS;
                    // 修改select icon为lined
                    LassoSelect.Source = selectLinedIS;
                }
                switch (mode)
                {
                    case "pen":
                    case "color":
                    {
                        BoardPen.Background = new SolidColorBrush(Color.FromRgb(103, 156, 244));
                        Pen_Icon.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons-png/check-box-background.png"))) { Opacity = 1 };

                        ImageSource penSolidIS = new BitmapImage(new Uri("/Resources/new-icons/pen-solid.png", UriKind.Relative));
                        // 修改批注icon为solid
                        PenIcon.Source = penSolidIS;
                        break;
                    }
                    case "eraser":
                    {
                        Eraser_Icon.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons-png/check-box-background.png"))) { Opacity = 1 };
                        BoardEraser.Background = new SolidColorBrush(Color.FromRgb(103, 156, 244));

                        ImageSource circleEraserSolidIS = new BitmapImage(new Uri("/Resources/new-icons/circle-eraser-solid.png", UriKind.Relative));
                        // 修改擦icon为solid
                        CircleEraserToolbarIconImage.Source = circleEraserSolidIS;
                        break;
                    }
                    case "eraserByStrokes":
                    {
                        EraserByStrokes_Icon.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons-png/check-box-background.png"))) { Opacity = 1 };
                        BoardEraserByStrokes.Background = new SolidColorBrush(Color.FromRgb(103, 156, 244));

                        ImageSource strokeEraserSolidIS = new BitmapImage(new Uri("/Resources/new-icons/eraser-solid.png", UriKind.Relative));
                        // 修改笔记擦icon为solid
                        StrokeEraserToolbarIconImage.Source = strokeEraserSolidIS;
                        break;
                    }
                    case "select":
                    {
                        BoardSelect.Background = new SolidColorBrush(Color.FromRgb(103, 156, 244));
                        SymbolIconSelect.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons-png/check-box-background.png"))) { Opacity = 1 };

                        ImageSource selectSolidIS = new BitmapImage(new Uri("/Resources/new-icons/lasso-select-solid.png", UriKind.Relative));
                        // 修改select icon为solid
                        LassoSelect.Source = selectSolidIS;
                        break;
                    }
                }


                if (autoAlignCenter) // 控制居中
                {
                    if (BtnPPTSlideShowEnd.Visibility == Visibility.Visible)
                    {
                        await Task.Delay(50);
                        ViewboxFloatingBarMarginAnimation(60);
                    }
                    else if (Topmost == true) //非黑板
                    {
                        await Task.Delay(50);
                        ViewboxFloatingBarMarginAnimation(100, true);
                    }
                    else //黑板
                    {
                        await Task.Delay(50);
                        ViewboxFloatingBarMarginAnimation(60);
                    }
                }
            }
            await Task.Delay(150);
            isHidingSubPanelsWhenInking = false;
        }
        #endregion

        private void BorderPenColorBlack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BtnColorBlack_Click(null, null);
            HideSubPanels();
        }

        private void BorderPenColorRed_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BtnColorRed_Click(null, null);
            HideSubPanels();
        }

        private void BorderPenColorGreen_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BtnColorGreen_Click(null, null);
            HideSubPanels();
        }

        private void BorderPenColorBlue_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BtnColorBlue_Click(null, null);
            HideSubPanels();
        }

        private void BorderPenColorYellow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BtnColorYellow_Click(null, null);
            HideSubPanels();
        }

        private void BorderPenColorWhite_MouseUp(object sender, MouseButtonEventArgs e)
        {
            inkCanvas.DefaultDrawingAttributes.Color = StringToColor("#FFFEFEFE");
            inkColor = 5;
            ColorSwitchCheck();
            HideSubPanels();
        }

        private void SymbolIconUndo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;
            
            if (!BtnUndo.IsEnabled) return;
            BtnUndo_Click(BtnUndo, null);
            HideSubPanels();
        }

        private void SymbolIconRedo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;
            
            if (!BtnRedo.IsEnabled) return;
            BtnRedo_Click(BtnRedo, null);
            HideSubPanels();
        }

        private async void SymbolIconCursor_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode != 0)
            {
                ImageBlackboard_MouseUp(null, null);
            }
            else
            {
                BtnHideInkCanvas_Click(BtnHideInkCanvas, null);

                if (BtnPPTSlideShowEnd.Visibility == Visibility.Visible)
                {
                    await Task.Delay(100);
                    ViewboxFloatingBarMarginAnimation(60);
                }
            }
        }

        private void SymbolIconDelete_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            if (inkCanvas.GetSelectedStrokes().Count > 0)
            {
                inkCanvas.Strokes.Remove(inkCanvas.GetSelectedStrokes());
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
            }
            else if (inkCanvas.Strokes.Count > 0)
            {
                if (Settings.Automation.IsAutoSaveStrokesAtClear && inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber)
                {
                    if (BtnPPTSlideShowEnd.Visibility == Visibility.Visible)
                        SaveScreenShot(true, $"{pptName}/{previousSlideID}_{DateTime.Now:HH-mm-ss}");
                    else
                        SaveScreenShot(true);
                }
                BtnClear_Click(null, null);
            }
        }

        private void SymbolIconSettings_Click(object sender, RoutedEventArgs e)
        {
            HideSubPanels();
            BtnSettings_Click(null, null);
        }

        private void SymbolIconSelect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FloatingbarSelectionBG.Visibility = Visibility.Visible;
            System.Windows.Controls.Canvas.SetLeft(FloatingbarSelectionBG, 140);
            BtnSelect_Click(null, null);
            HideSubPanels("select");
        }

        private async void SymbolIconScreenshot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HideSubPanelsImmediately();
            await Task.Delay(50);
            SaveScreenShotToDesktop();
        }

        bool Not_Enter_Blackboard_fir_Mouse_Click = true;
        bool isDisplayingOrHidingBlackboard = false;
        private void ImageBlackboard_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            LeftUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            RightUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            UnFoldFloatingBar_MouseUp(null, null);
            if (isDisplayingOrHidingBlackboard) return;
            isDisplayingOrHidingBlackboard = true;

            if (inkCanvas.EditingMode == InkCanvasEditingMode.Select) PenIcon_Click(null, null);

            if (currentMode == 0)
            {
                BottomViewboxPPTSidesControl.Visibility = Visibility.Collapsed;
                LeftSidePanelForPPTNavigation.Visibility = Visibility.Collapsed;
                RightSidePanelForPPTNavigation.Visibility = Visibility.Collapsed;
                //进入黑板

                /*
                if (Not_Enter_Blackboard_fir_Mouse_Click) {// BUG-Fixed_tmp：程序启动后直接进入白板会导致后续撤销功能、退出白板无法恢复墨迹
                    BtnColorRed_Click(BorderPenColorRed, null);
                    await Task.Delay(200);
                    SimulateMouseClick.SimulateMouseClickAtTopLeft();
                    await Task.Delay(10);
                    Not_Enter_Blackboard_fir_Mouse_Click = false;
                }
                */
                new Thread(new ThreadStart(() => {
                    Thread.Sleep(100);
                    Application.Current.Dispatcher.Invoke(() => {
                        ViewboxFloatingBarMarginAnimation(60);
                    });
                })).Start();

                if (Settings.Canvas.UsingWhiteboard)
                {
                    BorderPenColorBlack_MouseUp(BorderPenColorBlack, null);
                }
                else
                {
                    BorderPenColorWhite_MouseUp(BorderPenColorWhite, null);
                }

                if (Settings.Gesture.AutoSwitchTwoFingerGesture) // 自动关闭多指书写、开启双指移动
                {
                    ToggleSwitchEnableTwoFingerTranslate.IsOn = true;
                    if (isInMultiTouchMode) ToggleSwitchEnableMultiTouchMode.IsOn = false;
                }
                if (Settings.Appearance.EnableTimeDisplayInWhiteboardMode == true)
                {
                    WaterMarkTime.Visibility = Visibility.Visible;
                    WaterMarkDate.Visibility = Visibility.Visible;
                }
                BlackBoardWaterMark.Visibility = Visibility.Visible;
            }
            else
            {
                //关闭黑板
                HideSubPanelsImmediately();

                if (StackPanelPPTControls.Visibility == Visibility.Visible)
                {
                    if (Settings.PowerPointSettings.IsShowBottomPPTNavigationPanel)
                    {
                        AnimationsHelper.ShowWithSlideFromBottomAndFade(BottomViewboxPPTSidesControl);
                    }
                    if (Settings.PowerPointSettings.IsShowSidePPTNavigationPanel)
                    {
                        AnimationsHelper.ShowWithScaleFromLeft(LeftSidePanelForPPTNavigation);
                        AnimationsHelper.ShowWithScaleFromRight(RightSidePanelForPPTNavigation);
                    }
                }

                if (Settings.Automation.IsAutoSaveStrokesAtClear && inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber)
                {
                    SaveScreenShot(true);
                }

                if (BtnPPTSlideShowEnd.Visibility == Visibility.Collapsed)
                {
                    new Thread(new ThreadStart(() => {
                        Thread.Sleep(100);
                        Application.Current.Dispatcher.Invoke(() => {
                            ViewboxFloatingBarMarginAnimation(100, true);
                        });
                    })).Start();
                }
                else
                {
                    new Thread(new ThreadStart(() => {
                        Thread.Sleep(100);
                        Application.Current.Dispatcher.Invoke(() => {
                            ViewboxFloatingBarMarginAnimation(60);
                        });
                    })).Start();
                }
                if (Pen_Icon.Background == null)
                {
                    PenIcon_Click(null, null);
                }

                if (Settings.Gesture.AutoSwitchTwoFingerGesture) // 自动启用多指书写
                {
                    ToggleSwitchEnableTwoFingerTranslate.IsOn = false;
                    // 2024.5.2 need to be tested
                    // if (!isInMultiTouchMode) ToggleSwitchEnableMultiTouchMode.IsOn = true;
                }
                WaterMarkTime.Visibility = Visibility.Collapsed;
                WaterMarkDate.Visibility = Visibility.Collapsed;
                BlackBoardWaterMark.Visibility = Visibility.Collapsed;
            }

            BtnSwitch_Click(BtnSwitch, null);

            if (currentMode == 0 && inkCanvas.Strokes.Count == 0 && BtnPPTSlideShowEnd.Visibility != Visibility.Visible)
            {
                CursorIcon_Click(null, null);
            }

            BtnExit.Foreground = Brushes.White;
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;

            new Thread(new ThreadStart(() => {
                Thread.Sleep(200);
                Application.Current.Dispatcher.Invoke(() => {
                    isDisplayingOrHidingBlackboard = false;
                });
            })).Start();

            SwitchToDefaultPen(null, null);
            CheckColorTheme(true);
        }

        private void ImageCountdownTimer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            LeftUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            RightUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            AnimationsHelper.HideWithSlideAndFade(BorderTools);
            AnimationsHelper.HideWithSlideAndFade(BoardBorderTools);

            new CountdownTimerWindow().Show();
        }

        private void OperatingGuideWindowIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AnimationsHelper.HideWithSlideAndFade(BorderTools);
            AnimationsHelper.HideWithSlideAndFade(BoardBorderTools);

            new OperatingGuideWindow().Show();
        }

        private void SymbolIconRand_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            LeftUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            RightUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            if (lastBorderMouseDownObject != sender) return;

            AnimationsHelper.HideWithSlideAndFade(BorderTools);
            AnimationsHelper.HideWithSlideAndFade(BoardBorderTools);

            new RandWindow().Show();
        }

        public void CheckEraserTypeTab()
        {
            if (Settings.Canvas.EraserShapeType == 0)
            {
                CircleEraserTabButton.Background = new SolidColorBrush(Color.FromArgb(85, 59, 130, 246));
                CircleEraserTabButton.Opacity = 1;
                CircleEraserTabButtonText.FontWeight = FontWeights.Bold;
                CircleEraserTabButtonText.Margin = new Thickness(2, 0.5, 0, 0);
                CircleEraserTabButtonText.FontSize = 9.5;
                CircleEraserTabButtonIndicator.Visibility = Visibility.Visible;
                RectangleEraserTabButton.Background = new SolidColorBrush(Colors.Transparent);
                RectangleEraserTabButton.Opacity = 0.75;
                RectangleEraserTabButtonText.FontWeight = FontWeights.Normal;
                RectangleEraserTabButtonText.FontSize = 9;
                RectangleEraserTabButtonText.Margin = new Thickness(2, 1, 0, 0);
                RectangleEraserTabButtonIndicator.Visibility = Visibility.Collapsed;
            }
            else
            {
                RectangleEraserTabButton.Background = new SolidColorBrush(Color.FromArgb(85, 59, 130, 246));
                RectangleEraserTabButton.Opacity = 1;
                RectangleEraserTabButtonText.FontWeight = FontWeights.Bold;
                RectangleEraserTabButtonText.Margin = new Thickness(2, 0.5, 0, 0);
                RectangleEraserTabButtonText.FontSize = 9.5;
                RectangleEraserTabButtonIndicator.Visibility = Visibility.Visible;
                CircleEraserTabButton.Background = new SolidColorBrush(Colors.Transparent);
                CircleEraserTabButton.Opacity = 0.75;
                CircleEraserTabButtonText.FontWeight = FontWeights.Normal;
                CircleEraserTabButtonText.FontSize = 9;
                CircleEraserTabButtonText.Margin = new Thickness(2, 1, 0, 0);
                CircleEraserTabButtonIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SymbolIconRandOne_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            LeftUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            RightUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            if (lastBorderMouseDownObject != sender) return;

            AnimationsHelper.HideWithSlideAndFade(BorderTools);
            AnimationsHelper.HideWithSlideAndFade(BoardBorderTools);

            new RandWindow(true).ShowDialog();
        }

        private void GridInkReplayButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            AnimationsHelper.HideWithSlideAndFade(BorderTools);
            AnimationsHelper.HideWithSlideAndFade(BoardBorderTools);

            CollapseBorderDrawShape();

            InkCanvasForInkReplay.Visibility = Visibility.Visible;
            inkCanvas.Visibility = Visibility.Collapsed;
            isStopInkReplay = false;
            InkCanvasForInkReplay.Strokes.Clear();
            StrokeCollection strokes = inkCanvas.Strokes.Clone();
            if (inkCanvas.GetSelectedStrokes().Count != 0)
            {
                strokes = inkCanvas.GetSelectedStrokes().Clone();
            }
            int k = 1, i = 0;
            new Thread(new ThreadStart(() => {
                foreach (Stroke stroke in strokes)
                {
                    StylusPointCollection stylusPoints = new StylusPointCollection();
                    if (stroke.StylusPoints.Count == 629) //圆或椭圆
                    {
                        Stroke s = null;
                        foreach (StylusPoint stylusPoint in stroke.StylusPoints)
                        {
                            if (i++ >= 50)
                            {
                                i = 0;
                                Thread.Sleep(10);
                                if (isStopInkReplay) return;
                            }
                            Application.Current.Dispatcher.Invoke(() => {
                                try
                                {
                                    InkCanvasForInkReplay.Strokes.Remove(s);
                                }
                                catch { }
                                stylusPoints.Add(stylusPoint);
                                s = new Stroke(stylusPoints.Clone());
                                s.DrawingAttributes = stroke.DrawingAttributes;
                                InkCanvasForInkReplay.Strokes.Add(s);
                            });
                        }
                    }
                    else
                    {
                        Stroke s = null;
                        foreach (StylusPoint stylusPoint in stroke.StylusPoints)
                        {
                            if (i++ >= k)
                            {
                                i = 0;
                                Thread.Sleep(10);
                                if (isStopInkReplay) return;
                            }
                            Application.Current.Dispatcher.Invoke(() => {
                                try
                                {
                                    InkCanvasForInkReplay.Strokes.Remove(s);
                                }
                                catch { }
                                stylusPoints.Add(stylusPoint);
                                s = new Stroke(stylusPoints.Clone());
                                s.DrawingAttributes = stroke.DrawingAttributes;
                                InkCanvasForInkReplay.Strokes.Add(s);
                            });
                        }
                    }
                }
                Thread.Sleep(100);
                Application.Current.Dispatcher.Invoke(() => {
                    InkCanvasForInkReplay.Visibility = Visibility.Collapsed;
                    inkCanvas.Visibility = Visibility.Visible;
                });
            })).Start();
        }
        bool isStopInkReplay = false;
        private void InkCanvasForInkReplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                InkCanvasForInkReplay.Visibility = Visibility.Collapsed;
                inkCanvas.Visibility = Visibility.Visible;
                isStopInkReplay = true;
            }
        }

        private void SymbolIconTools_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            if (BorderTools.Visibility == Visibility.Visible)
            {
                AnimationsHelper.HideWithSlideAndFade(BorderTools);
                AnimationsHelper.HideWithSlideAndFade(BoardBorderTools);
            }
            else
            {
                AnimationsHelper.ShowWithSlideFromBottomAndFade(BorderTools);
                AnimationsHelper.ShowWithSlideFromBottomAndFade(BoardBorderTools);
            }
        }

        bool isViewboxFloatingBarMarginAnimationRunning = false;

        private async void ViewboxFloatingBarMarginAnimation(int MarginFromEdge, bool PosXCaculatedWithTaskbarHeight = false)
        {
            if (MarginFromEdge == 60)
            {
                MarginFromEdge = 55;
            }
            await Dispatcher.InvokeAsync(() => {
                if (Topmost == false)
                {
                    MarginFromEdge = -60;
                }
                else
                {
                    ViewboxFloatingBar.Visibility = Visibility.Visible;
                }
                isViewboxFloatingBarMarginAnimationRunning = true;

                double dpiScaleX = 1, dpiScaleY = 1;
                PresentationSource source = PresentationSource.FromVisual(this);
                if (source != null)
                {
                    dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                    dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
                }
                IntPtr windowHandle = new WindowInteropHelper(this).Handle;
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(windowHandle);
                double screenWidth = screen.Bounds.Width / dpiScaleX, screenHeight = screen.Bounds.Height / dpiScaleY;
                double toolbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight - SystemParameters.WindowCaptionHeight;
                pos.X = (screenWidth - ViewboxFloatingBar.ActualWidth * ViewboxFloatingBarScaleTransform.ScaleX) / 2;

                if (PosXCaculatedWithTaskbarHeight == false)
                {
                    pos.Y = screenHeight - MarginFromEdge * ViewboxFloatingBarScaleTransform.ScaleY;
                }
                else if (PosXCaculatedWithTaskbarHeight == true)
                {
                    pos.Y = screenHeight - ViewboxFloatingBar.ActualHeight * ViewboxFloatingBarScaleTransform.ScaleY - toolbarHeight - ViewboxFloatingBarScaleTransform.ScaleY * 3;
                }

                if (MarginFromEdge != -60)
                {
                    if (BtnPPTSlideShowEnd.Visibility == Visibility.Visible)
                    {
                        if (pointPPT.X != -1 || pointPPT.Y != -1)
                        {
                            if (Math.Abs(pointPPT.Y - pos.Y) > 50)
                            {
                                pos = pointPPT;
                            }
                            else
                            {
                                pointPPT = pos;
                            }
                        }
                    }
                    else
                    {
                        if (pointDesktop.X != -1 || pointDesktop.Y != -1)
                        {
                            if (Math.Abs(pointDesktop.Y - pos.Y) > 50)
                            {
                                pos = pointDesktop;
                            }
                            else
                            {
                                pointDesktop = pos;
                            }
                        }
                    }
                }

                ThicknessAnimation marginAnimation = new ThicknessAnimation
                {
                    Duration = TimeSpan.FromSeconds(0.3),
                    From = ViewboxFloatingBar.Margin,
                    To = new Thickness(pos.X, pos.Y, -2000, -200)
                };
                marginAnimation.EasingFunction = new CircleEase();
                ViewboxFloatingBar.BeginAnimation(FrameworkElement.MarginProperty, marginAnimation);
            });

            await Task.Delay(200);

            await Dispatcher.InvokeAsync(() => {
                ViewboxFloatingBar.Margin = new Thickness(pos.X, pos.Y, -2000, -200);
                if (Topmost == false) ViewboxFloatingBar.Visibility = Visibility.Hidden;
            });
        }

        private async void CursorIcon_Click(object sender, RoutedEventArgs e)
        {
            // 隱藏高亮
            FloatingbarSelectionBG.Visibility = Visibility.Hidden;

            // 切换前自动截图保存墨迹
            if (inkCanvas.Strokes.Count > 0 && inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber)
            {
                if (BtnPPTSlideShowEnd.Visibility == Visibility.Visible) SaveScreenShot(true, $"{pptName}/{previousSlideID}_{DateTime.Now:HH-mm-ss}");
                else SaveScreenShot(true);
            }

            if (BtnPPTSlideShowEnd.Visibility != Visibility.Visible)
            {
                if (Settings.Canvas.HideStrokeWhenSelecting)
                    inkCanvas.Visibility = Visibility.Collapsed;
                else
                {
                    inkCanvas.IsHitTestVisible = false;
                    inkCanvas.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (Settings.PowerPointSettings.IsShowStrokeOnSelectInPowerPoint)
                {
                    inkCanvas.Visibility = Visibility.Visible;
                    inkCanvas.IsHitTestVisible = true;
                }
                else
                {
                    if (Settings.Canvas.HideStrokeWhenSelecting)
                        inkCanvas.Visibility = Visibility.Collapsed;
                    else
                    {
                        inkCanvas.IsHitTestVisible = false;
                        inkCanvas.Visibility = Visibility.Visible;
                    }
                }
            }


            Main_Grid.Background = Brushes.Transparent;


            GridBackgroundCoverHolder.Visibility = Visibility.Collapsed;
            inkCanvas.Select(new StrokeCollection());
            GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

            if (currentMode != 0)
            {
                SaveStrokes();
                RestoreStrokes(true);
            }

            if (BtnSwitchTheme.Content.ToString() == "浅色")
            {
                BtnSwitch.Content = "黑板";
            }
            else
            {
                BtnSwitch.Content = "白板";
            }

            StackPanelPPTButtons.Visibility = Visibility.Visible;
            BtnHideInkCanvas.Content = "显示\n画板";
            CheckEnableTwoFingerGestureBtnVisibility(false);


            StackPanelCanvasControls.Visibility = Visibility.Collapsed;

            if (!isFloatingBarFolded)
            {
                HideSubPanels("cursor", true);
                await Task.Delay(50);

                if (BtnPPTSlideShowEnd.Visibility == Visibility.Visible)
                {
                    ViewboxFloatingBarMarginAnimation(60);
                }
                else
                {
                    ViewboxFloatingBarMarginAnimation(100, true);
                }
            }
        }

        private void PenIcon_Click(object sender, RoutedEventArgs e)
        {

            FloatingbarSelectionBG.Visibility = Visibility.Visible;
            System.Windows.Controls.Canvas.SetLeft(FloatingbarSelectionBG, 28);

            if (Pen_Icon.Background == null || StackPanelCanvasControls.Visibility == Visibility.Collapsed)
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;

                Main_Grid.Background = new SolidColorBrush(StringToColor("#01FFFFFF"));

                inkCanvas.IsHitTestVisible = true;
                inkCanvas.Visibility = Visibility.Visible;

                GridBackgroundCoverHolder.Visibility = Visibility.Visible;
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

                /*if (forceEraser && currentMode == 0)
                    BtnColorRed_Click(sender, null);*/

                if (GridBackgroundCover.Visibility == Visibility.Collapsed)
                {
                    if (BtnSwitchTheme.Content.ToString() == "浅色")
                    {
                        BtnSwitch.Content = "黑板";
                    }
                    else
                    {
                        BtnSwitch.Content = "白板";
                    }
                    StackPanelPPTButtons.Visibility = Visibility.Visible;
                }
                else
                {
                    BtnSwitch.Content = "屏幕";
                    StackPanelPPTButtons.Visibility = Visibility.Collapsed;
                }

                BtnHideInkCanvas.Content = "隐藏\n画板";

                StackPanelCanvasControls.Visibility = Visibility.Visible;
                //AnimationsHelper.ShowWithSlideFromLeftAndFade(StackPanelCanvasControls);
                CheckEnableTwoFingerGestureBtnVisibility(true);
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                ColorSwitchCheck();
                HideSubPanels("pen", true);
            }
            else
            {
                if (inkCanvas.EditingMode == InkCanvasEditingMode.Ink)
                {
                    if (PenPalette.Visibility == Visibility.Visible)
                    {
                        AnimationsHelper.HideWithSlideAndFade(PenPalette);
                        AnimationsHelper.HideWithSlideAndFade(BoardPenPalette);
                    }
                    else
                    {
                        AnimationsHelper.ShowWithSlideFromBottomAndFade(PenPalette);
                        AnimationsHelper.ShowWithSlideFromBottomAndFade(BoardPenPalette);
                    }
                }
                else
                {
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    ColorSwitchCheck();
                    HideSubPanels("pen", true);
                }

            }
        }

        private void ColorThemeSwitch_MouseUp(object sender, RoutedEventArgs e)
        {
            isUselightThemeColor = !isUselightThemeColor;
            if (currentMode == 0)
            {
                isDesktopUselightThemeColor = isUselightThemeColor;
            }
            CheckColorTheme();
        }

        private void EraserIcon_Click(object sender, RoutedEventArgs e)
        {
            FloatingbarSelectionBG.Visibility = Visibility.Visible;
            System.Windows.Controls.Canvas.SetLeft(FloatingbarSelectionBG, 84);

            forceEraser = true;
            forcePointEraser = true;
            if (Settings.Canvas.EraserShapeType == 0)
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
                inkCanvas.EraserShape = new EllipseStylusShape(k * 90, k * 90);
            }
            else if (Settings.Canvas.EraserShapeType == 1)
            {
                double k = 1;
                switch (Settings.Canvas.EraserSize)
                {
                    case 0:
                        k = 0.7;
                        break;
                    case 1:
                        k = 0.9;
                        break;
                    case 3:
                        k = 1.2;
                        break;
                    case 4:
                        k = 1.6;
                        break;
                }
                inkCanvas.EraserShape = new RectangleStylusShape(k * 90 * 0.6, k * 90);
            }

            if (inkCanvas.EditingMode == InkCanvasEditingMode.EraseByPoint)
            {
                HideSubPanels();
                HideSubPanelsImmediately();
                AnimationsHelper.ShowWithSlideFromBottomAndFade(EraserSizePanel);
            }
            else
            {
                HideSubPanels("eraser");
            }

            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            drawingShapeMode = 0;

            inkCanvas_EditingModeChanged(inkCanvas, null);
            CancelSingleFingerDragMode();
        }

        private void EraserIconByStrokes_Click(object sender, RoutedEventArgs e)
        {
            FloatingbarSelectionBG.Visibility = Visibility.Visible;
            System.Windows.Controls.Canvas.SetLeft(FloatingbarSelectionBG, 112);

            forceEraser = true;
            forcePointEraser = false;

            inkCanvas.EraserShape = new EllipseStylusShape(5, 5);
            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
            drawingShapeMode = 0;

            inkCanvas_EditingModeChanged(inkCanvas, null);
            CancelSingleFingerDragMode();

            HideSubPanels("eraserByStrokes");
        }

        private void CursorWithDelIcon_Click(object sender, RoutedEventArgs e)
        {
            
            SymbolIconDelete_MouseUp(sender, null);
            CursorIcon_Click(null, null);
        }

        private void SelectIcon_MouseUp(object sender, RoutedEvent e)
        {
            
            forceEraser = true;
            drawingShapeMode = 0;
            inkCanvas.IsManipulationEnabled = false;
            if (inkCanvas.EditingMode == InkCanvasEditingMode.Select)
            {
                StrokeCollection selectedStrokes = new StrokeCollection();
                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    if (stroke.GetBounds().Width > 0 && stroke.GetBounds().Height > 0)
                    {
                        selectedStrokes.Add(stroke);
                    }
                }
                inkCanvas.Select(selectedStrokes);
            }
            else
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.Select;
            }
        }

        private void CollapseBorderDrawShape(bool isLongPressSelected = false)
        {
            AnimationsHelper.HideWithSlideAndFade(BorderDrawShape);
            AnimationsHelper.HideWithSlideAndFade(BoardBorderDrawShape);
        }

        private void DrawShapePromptToPen()
        {
            if (isLongPressSelected == true)
            {
                HideSubPanels("pen");
            }
            else
            {
                if (StackPanelCanvasControls.Visibility == Visibility.Visible)
                {
                    HideSubPanels("pen");
                }
                else
                {
                    HideSubPanels("cursor");
                }
            }
        }

        private void CloseBordertools_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HideSubPanels();
        }

        #region Left Side Panel

        private void BtnFingerDragMode_Click(object sender, RoutedEventArgs e)
        {
            if (isSingleFingerDragMode)
            {
                isSingleFingerDragMode = false;
                BtnFingerDragMode.Content = "单指\n拖动";
            }
            else
            {
                isSingleFingerDragMode = true;
                BtnFingerDragMode.Content = "多指\n拖动";
            }
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.GetSelectedStrokes().Count != 0)
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                inkCanvas.Select(new StrokeCollection());
            }
            _currentCommitType = CommitReason.CodeInput;
            var item = timeMachine.Undo();
            if (item.CommitType == TimeMachineHistoryType.UserInput)
            {
                if (!item.StrokeHasBeenCleared)
                {
                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                    }
                }
                else
                {
                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                    }
                }
            }
            else if (item.CommitType == TimeMachineHistoryType.ShapeRecognition)
            {
                if (item.StrokeHasBeenCleared)
                {

                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                    }
                    foreach (var strokes in item.ReplacedStroke)
                    {
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                    }
                }
                else
                {
                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                    }
                    foreach (var strokes in item.ReplacedStroke)
                    {
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                    }
                }
            }
            else if (item.CommitType == TimeMachineHistoryType.Manipulation)
            {
                if (item.StrokeHasBeenCleared)
                {

                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                    }
                    foreach (var strokes in item.ReplacedStroke)
                    {
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                    }
                }
                else
                {
                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                    }
                    foreach (var strokes in item.ReplacedStroke)
                    {
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                    }
                }
            }
            else if (item.CommitType == TimeMachineHistoryType.Clear)
            {
                if (!item.StrokeHasBeenCleared)
                {
                    if (item.CurrentStroke != null)
                    {
                        foreach (var currentStroke in item.CurrentStroke)
                        {
                            if (!inkCanvas.Strokes.Contains(currentStroke)) inkCanvas.Strokes.Add(currentStroke);
                        }

                    }
                    if (item.ReplacedStroke != null)
                    {
                        foreach (var replacedStroke in item.ReplacedStroke)
                        {
                            if (inkCanvas.Strokes.Contains(replacedStroke)) inkCanvas.Strokes.Remove(replacedStroke);
                        }
                    }

                }
                else
                {
                    if (item.ReplacedStroke != null)
                    {
                        foreach (var replacedStroke in item.ReplacedStroke)
                        {
                            if (!inkCanvas.Strokes.Contains(replacedStroke)) inkCanvas.Strokes.Add(replacedStroke);
                        }
                    }
                    if (item.CurrentStroke != null)
                    {
                        foreach (var currentStroke in item.CurrentStroke)
                        {
                            if (inkCanvas.Strokes.Contains(currentStroke)) inkCanvas.Strokes.Remove(currentStroke);
                        }
                    }
                }
            }
            _currentCommitType = CommitReason.UserInput;
        }

        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.GetSelectedStrokes().Count != 0)
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                inkCanvas.Select(new StrokeCollection());
            }

            _currentCommitType = CommitReason.CodeInput;
            var item = timeMachine.Redo();
            if (item.CommitType == TimeMachineHistoryType.UserInput)
            {
                if (!item.StrokeHasBeenCleared)
                {
                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                    }
                }
                else
                {
                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                    }
                }
            }
            else if (item.CommitType == TimeMachineHistoryType.ShapeRecognition)
            {
                if (item.StrokeHasBeenCleared)
                {

                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                    }
                    foreach (var strokes in item.ReplacedStroke)
                    {
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                    }
                }
                else
                {
                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                    }
                    foreach (var strokes in item.ReplacedStroke)
                    {
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                    }
                }
            }
            else if (item.CommitType == TimeMachineHistoryType.Manipulation)
            {
                if (item.StrokeHasBeenCleared)
                {

                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                    }
                    foreach (var strokes in item.ReplacedStroke)
                    {
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                    }
                }
                else
                {
                    foreach (var strokes in item.CurrentStroke)
                    {
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                    }
                    foreach (var strokes in item.ReplacedStroke)
                    {
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                    }
                }
            }
            else if (item.CommitType == TimeMachineHistoryType.Clear)
            {
                if (!item.StrokeHasBeenCleared)
                {
                    if (item.CurrentStroke != null)
                    {
                        foreach (var currentStroke in item.CurrentStroke)
                        {
                            if (!inkCanvas.Strokes.Contains(currentStroke)) inkCanvas.Strokes.Add(currentStroke);
                        }

                    }
                    if (item.ReplacedStroke != null)
                    {
                        foreach (var replacedStroke in item.ReplacedStroke)
                        {
                            if (inkCanvas.Strokes.Contains(replacedStroke)) inkCanvas.Strokes.Remove(replacedStroke);
                        }
                    }

                }
                else
                {
                    if (item.ReplacedStroke != null)
                    {
                        foreach (var replacedStroke in item.ReplacedStroke)
                        {
                            if (!inkCanvas.Strokes.Contains(replacedStroke)) inkCanvas.Strokes.Add(replacedStroke);
                        }
                    }
                    if (item.CurrentStroke != null)
                    {
                        foreach (var currentStroke in item.CurrentStroke)
                        {
                            if (inkCanvas.Strokes.Contains(currentStroke)) inkCanvas.Strokes.Remove(currentStroke);
                        }
                    }
                }
            }
            _currentCommitType = CommitReason.UserInput;
        }

        private void Btn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!isLoaded) return;
            try
            {
                if (((Button)sender).IsEnabled)
                {
                    ((UIElement)((Button)sender).Content).Opacity = 1;
                }
                else
                {
                    ((UIElement)((Button)sender).Content).Opacity = 0.25;
                }
            }
            catch { }
        }

        #endregion Left Side Panel

        #region Right Side Panel

        public static bool CloseIsFromButton = false;
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            CloseIsFromButton = true;
            Close();
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(System.Windows.Forms.Application.ExecutablePath, "-m");

            CloseIsFromButton = true;
            Application.Current.Shutdown();
        }

        private void SettingsOverlayClick(object sender, MouseButtonEventArgs e)
        {
            BtnSettings_Click(null, null);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (BorderSettings.Visibility == Visibility.Visible)
            {
                BorderSettingsMask.IsHitTestVisible = false;
                BorderSettingsMask.Background = null;
                var sb = new Storyboard();

                // 滑动动画
                var slideAnimation = new DoubleAnimation
                {
                    From = 0, // 滑动距离
                    To = BorderSettings.RenderTransform.Value.OffsetX - 440,
                    Duration = TimeSpan.FromSeconds(0.6)
                };
                slideAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

                sb.Children.Add(slideAnimation);

                sb.Completed += (s, _) =>
                {
                    BorderSettings.Visibility = Visibility.Collapsed;
                };

                BorderSettings.Visibility = Visibility.Visible;
                BorderSettings.RenderTransform = new TranslateTransform();

                sb.Begin((FrameworkElement)BorderSettings);
            }
            else
            {
                BorderSettingsMask.IsHitTestVisible = true;
                BorderSettingsMask.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                SettingsPanelScrollViewer.ScrollToTop();
                var sb = new Storyboard();

                // 滑动动画
                var slideAnimation = new DoubleAnimation
                {
                    From = BorderSettings.RenderTransform.Value.OffsetX - 440, // 滑动距离
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.6)
                };
                slideAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

                sb.Children.Add(slideAnimation);

                BorderSettings.Visibility = Visibility.Visible;
                BorderSettings.RenderTransform = new TranslateTransform();

                sb.Begin((FrameworkElement)BorderSettings);
            }
        }

        private void BtnThickness_Click(object sender, RoutedEventArgs e)
        {

        }

        bool forceEraser = false;


        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            forceEraser = false;
            //BorderClearInDelete.Visibility = Visibility.Collapsed;

            if (currentMode == 0)
            { // 先回到画笔再清屏，避免 TimeMachine 的相关 bug 影响
                if (Pen_Icon.Background == null && StackPanelCanvasControls.Visibility == Visibility.Visible)
                {
                    PenIcon_Click(null, null);
                }
            }
            else
            {
                if (Pen_Icon.Background == null)
                {
                    PenIcon_Click(null, null);
                }
            }

            if (inkCanvas.Strokes.Count != 0)
            {
                int whiteboardIndex = CurrentWhiteboardIndex;
                if (currentMode == 0)
                {
                    whiteboardIndex = 0;
                }
                strokeCollections[whiteboardIndex] = inkCanvas.Strokes.Clone();

            }

            ClearStrokes(false);
            inkCanvas.Children.Clear();

            CancelSingleFingerDragMode();

            if (Settings.Canvas.ClearCanvasAndClearTimeMachine)
            {
                timeMachine.ClearStrokeHistory();
            }
        }

        bool lastIsInMultiTouchMode = false;
        private void CancelSingleFingerDragMode()
        {
            if (ToggleSwitchDrawShapeBorderAutoHide.IsOn)
            {
                CollapseBorderDrawShape();
            }

            GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

            if (isSingleFingerDragMode)
            {
                BtnFingerDragMode_Click(BtnFingerDragMode, null);
            }
            isLongPressSelected = false;
        }

        private void BtnHideControl_Click(object sender, RoutedEventArgs e)
        {
            if (StackPanelControl.Visibility == Visibility.Visible)
            {
                StackPanelControl.Visibility = Visibility.Hidden;
            }
            else
            {
                StackPanelControl.Visibility = Visibility.Visible;
            }
        }

        int currentMode = 0;

        private void BtnSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (Main_Grid.Background == Brushes.Transparent)
            {
                if (currentMode == 0)
                {
                    currentMode++;
                    GridBackgroundCover.Visibility = Visibility.Collapsed;
                    AnimationsHelper.HideWithSlideAndFade(BlackboardLeftSide);
                    AnimationsHelper.HideWithSlideAndFade(BlackboardCenterSide);
                    AnimationsHelper.HideWithSlideAndFade(BlackboardRightSide);

                    SaveStrokes(true);
                    ClearStrokes(true);
                    RestoreStrokes();

                    if (BtnSwitchTheme.Content.ToString() == "浅色")
                    {
                        BtnSwitch.Content = "黑板";
                        BtnExit.Foreground = Brushes.White;
                    }
                    else
                    {
                        BtnSwitch.Content = "白板";
                        if (isPresentationHaveBlackSpace)
                        {
                            BtnExit.Foreground = Brushes.White;
                            //SymbolIconBtnColorBlackContent.Foreground = Brushes.White;
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        }
                        else
                        {
                            BtnExit.Foreground = Brushes.Black;
                            //SymbolIconBtnColorBlackContent.Foreground = Brushes.White;
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        }
                    }
                    StackPanelPPTButtons.Visibility = Visibility.Visible;
                }
                Topmost = true;
                BtnHideInkCanvas_Click(BtnHideInkCanvas, e);
            }
            else
            {
                switch ((++currentMode) % 2)
                {
                    case 0: //屏幕模式
                        currentMode = 0;
                        GridBackgroundCover.Visibility = Visibility.Collapsed;
                        AnimationsHelper.HideWithSlideAndFade(BlackboardLeftSide);
                        AnimationsHelper.HideWithSlideAndFade(BlackboardCenterSide);
                        AnimationsHelper.HideWithSlideAndFade(BlackboardRightSide);

                        SaveStrokes();
                        ClearStrokes(true);
                        RestoreStrokes(true);

                        if (BtnSwitchTheme.Content.ToString() == "浅色")
                        {
                            BtnSwitch.Content = "黑板";
                            BtnExit.Foreground = Brushes.White;
                            //SymbolIconBtnColorBlackContent.Foreground = Brushes.Black;
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        }
                        else
                        {
                            BtnSwitch.Content = "白板";
                            if (isPresentationHaveBlackSpace)
                            {
                                BtnExit.Foreground = Brushes.White;
                                //SymbolIconBtnColorBlackContent.Foreground = Brushes.White;
                                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                            }
                            else
                            {
                                BtnExit.Foreground = Brushes.Black;
                                //SymbolIconBtnColorBlackContent.Foreground = Brushes.White;
                                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                            }
                        }

                        StackPanelPPTButtons.Visibility = Visibility.Visible;
                        Topmost = true;
                        break;
                    case 1: //黑板或白板模式
                        currentMode = 1;
                        GridBackgroundCover.Visibility = Visibility.Visible;
                        AnimationsHelper.ShowWithSlideFromBottomAndFade(BlackboardLeftSide);
                        AnimationsHelper.ShowWithSlideFromBottomAndFade(BlackboardCenterSide);
                        AnimationsHelper.ShowWithSlideFromBottomAndFade(BlackboardRightSide);

                        SaveStrokes(true);
                        ClearStrokes(true);
                        RestoreStrokes();

                        BtnSwitch.Content = "屏幕";
                        if (BtnSwitchTheme.Content.ToString() == "浅色")
                        {
                            BtnExit.Foreground = Brushes.White;
                            //SymbolIconBtnColorBlackContent.Foreground = Brushes.Black;
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        }
                        else
                        {
                            BtnExit.Foreground = Brushes.Black;
                            //SymbolIconBtnColorBlackContent.Foreground = Brushes.White;
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        }

                        StackPanelPPTButtons.Visibility = Visibility.Collapsed;
                        Topmost = false;
                        break;
                }
            }
        }

        int BoundsWidth = 5;

        private void BtnHideInkCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (Main_Grid.Background == Brushes.Transparent)
            {
                Main_Grid.Background = new SolidColorBrush(StringToColor("#01FFFFFF"));
                inkCanvas.IsHitTestVisible = true;
                inkCanvas.Visibility = Visibility.Visible;

                GridBackgroundCoverHolder.Visibility = Visibility.Visible;

                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

                if (GridBackgroundCover.Visibility == Visibility.Collapsed)
                {
                    if (BtnSwitchTheme.Content.ToString() == "浅色")
                    {
                        BtnSwitch.Content = "黑板";
                    }
                    else
                    {
                        BtnSwitch.Content = "白板";
                    }
                    StackPanelPPTButtons.Visibility = Visibility.Visible;
                }
                else
                {
                    BtnSwitch.Content = "屏幕";
                    StackPanelPPTButtons.Visibility = Visibility.Collapsed;
                }

                BtnHideInkCanvas.Content = "隐藏\n画板";
            }
            else
            {
                // Auto-clear Strokes 要等待截图完成再清理笔记
                if (BtnPPTSlideShowEnd.Visibility != Visibility.Visible)
                {
                    if (isLoaded && Settings.Automation.IsAutoClearWhenExitingWritingMode)
                    {
                        if (inkCanvas.Strokes.Count > 0)
                        {
                            if (Settings.Automation.IsAutoSaveStrokesAtClear && inkCanvas.Strokes.Count >
                                Settings.Automation.MinimumAutomationStrokeNumber)
                            {
                                SaveScreenShot(true);
                            }

                            BtnClear_Click(null, null);
                        }
                    }
                    inkCanvas.IsHitTestVisible = true;
                    inkCanvas.Visibility = Visibility.Visible;
                }
                else
                {
                    if (isLoaded && Settings.Automation.IsAutoClearWhenExitingWritingMode && !Settings.PowerPointSettings.IsNoClearStrokeOnSelectWhenInPowerPoint)
                    {
                        if (inkCanvas.Strokes.Count > 0)
                        {
                            if (Settings.Automation.IsAutoSaveStrokesAtClear && inkCanvas.Strokes.Count >
                                Settings.Automation.MinimumAutomationStrokeNumber)
                            {
                                SaveScreenShot(true);
                            }

                            BtnClear_Click(null, null);
                        }
                    }


                    if (Settings.PowerPointSettings.IsShowStrokeOnSelectInPowerPoint)
                    {
                        inkCanvas.Visibility = Visibility.Visible;
                        inkCanvas.IsHitTestVisible = true;
                    }
                    else
                    {
                        inkCanvas.IsHitTestVisible = true;
                        inkCanvas.Visibility = Visibility.Visible;
                    }
                }

                Main_Grid.Background = Brushes.Transparent;

                GridBackgroundCoverHolder.Visibility = Visibility.Collapsed;

                if (currentMode != 0)
                {
                    SaveStrokes();
                    RestoreStrokes(true);
                }

                if (BtnSwitchTheme.Content.ToString() == "浅色")
                {
                    BtnSwitch.Content = "黑板";
                }
                else
                {
                    BtnSwitch.Content = "白板";
                }

                StackPanelPPTButtons.Visibility = Visibility.Visible;
                BtnHideInkCanvas.Content = "显示\n画板";
            }

            if (Main_Grid.Background == Brushes.Transparent)
            {
                StackPanelCanvasControls.Visibility = Visibility.Collapsed;
                CheckEnableTwoFingerGestureBtnVisibility(false);
                HideSubPanels("cursor");
            }
            else
            {
                AnimationsHelper.ShowWithSlideFromLeftAndFade(StackPanelCanvasControls);
                CheckEnableTwoFingerGestureBtnVisibility(true);
            }
        }

        private void BtnSwitchSide_Click(object sender, RoutedEventArgs e)
        {
            if (ViewBoxStackPanelMain.HorizontalAlignment == HorizontalAlignment.Right)
            {
                ViewBoxStackPanelMain.HorizontalAlignment = HorizontalAlignment.Left;
                ViewBoxStackPanelShapes.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                ViewBoxStackPanelMain.HorizontalAlignment = HorizontalAlignment.Right;
                ViewBoxStackPanelShapes.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

        private void StackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((StackPanel)sender).Visibility == Visibility.Visible)
            {
                GridForLeftSideReservedSpace.Visibility = Visibility.Collapsed;
            }
            else
            {
                GridForLeftSideReservedSpace.Visibility = Visibility.Visible;
            }
        }

        #endregion

    }
}