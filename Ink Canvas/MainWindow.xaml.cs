using Ink_Canvas.Helpers;
using iNKORE.UI.WPF.Modern;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Diagnostics;
using File = System.IO.File;
using MessageBox = System.Windows.MessageBox;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Ink_Canvas {
    public partial class MainWindow : Window {

        #region Window Initialization

        public MainWindow() {
            /*
                处于画板模式内：Topmost == false / currentMode != 0
                处于 PPT 放映内：BtnPPTSlideShowEnd.Visibility
            */
            InitializeComponent();

            BlackboardLeftSide.Visibility = Visibility.Collapsed;
            BlackboardCenterSide.Visibility = Visibility.Collapsed;
            BlackboardRightSide.Visibility = Visibility.Collapsed;
            BorderTools.Visibility = Visibility.Collapsed;
            BorderSettings.Visibility = Visibility.Collapsed;
            LeftSidePanelForPPTNavigation.Visibility = Visibility.Collapsed;
            RightSidePanelForPPTNavigation.Visibility = Visibility.Collapsed;
            BorderSettings.Margin = new Thickness(0, 0, 0, 0);
            TwoFingerGestureBorder.Visibility = Visibility.Collapsed;
            BoardTwoFingerGestureBorder.Visibility = Visibility.Collapsed;
            BorderDrawShape.Visibility = Visibility.Collapsed;
            BoardBorderDrawShape.Visibility = Visibility.Collapsed;
            GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

            //if (!App.StartArgs.Contains("-o"))

            ViewBoxStackPanelMain.Visibility = Visibility.Collapsed;
            ViewBoxStackPanelShapes.Visibility = Visibility.Collapsed;
            ViewboxFloatingBar.Margin = new Thickness((SystemParameters.WorkArea.Width - 284) / 2, SystemParameters.WorkArea.Height - 60, -2000, -200);
            ViewboxFloatingBarMarginAnimation(100,true);

            try {
                if (File.Exists("debug.ini")) Label.Visibility = Visibility.Visible;
            } catch (Exception ex) {
                LogHelper.WriteLogToFile(ex.ToString(), LogHelper.LogType.Error);
            }
            try {
                if (File.Exists("Log.txt")) {
                    FileInfo fileInfo = new FileInfo("Log.txt");
                    long fileSizeInKB = fileInfo.Length / 1024;
                    if (fileSizeInKB > 512) {
                        try {
                            File.Delete("Log.txt");
                            LogHelper.WriteLogToFile("The Log.txt file has been successfully deleted. Original file size: " + fileSizeInKB + " KB", LogHelper.LogType.Info);
                        } catch (Exception ex) {
                            LogHelper.WriteLogToFile(ex + " | Can not delete the Log.txt file. File size: " + fileSizeInKB + " KB", LogHelper.LogType.Error);
                        }
                    }
                }
            } catch (Exception ex) {
                LogHelper.WriteLogToFile(ex.ToString(), LogHelper.LogType.Error);
            }

            InitTimers();
            timeMachine.OnRedoStateChanged += TimeMachine_OnRedoStateChanged;
            timeMachine.OnUndoStateChanged += TimeMachine_OnUndoStateChanged;
            inkCanvas.Strokes.StrokesChanged += StrokesOnStrokesChanged;

            Microsoft.Win32.SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            try {
                if (File.Exists("SpecialVersion.ini")) SpecialVersionResetToSuggestion_Click();
            } catch (Exception ex) {
                LogHelper.WriteLogToFile(ex.ToString(), LogHelper.LogType.Error);
            }

            CheckColorTheme(true);
            CheckPenTypeUIState();
        }

        #endregion

        #region Ink Canvas Functions
        
        Color Ink_DefaultColor = Colors.Red;

        DrawingAttributes drawingAttributes;
        private void loadPenCanvas() {
            try
            {

                double alpha = Settings.Canvas.InkAlpha;
                Trace.WriteLine(alpha);
                //drawingAttributes = new DrawingAttributes();
                drawingAttributes = inkCanvas.DefaultDrawingAttributes;
                drawingAttributes.Color = Ink_DefaultColor;

                drawingAttributes.Height = 2.5;
                drawingAttributes.Width = 2.5;
                drawingAttributes.IsHighlighter = false;
                drawingAttributes.FitToCurve = Settings.Canvas.FitToCurve;

                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                inkCanvas.Gesture += InkCanvas_Gesture;
                inkCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
            } catch { }
        }
        //ApplicationGesture lastApplicationGesture = ApplicationGesture.AllGestures;
        DateTime lastGestureTime = DateTime.Now;
        private void InkCanvas_Gesture(object sender, InkCanvasGestureEventArgs e) {
            ReadOnlyCollection<GestureRecognitionResult> gestures = e.GetGestureRecognitionResults();
            try {
                foreach (GestureRecognitionResult gest in gestures) {
                    //Trace.WriteLine(string.Format("Gesture: {0}, Confidence: {1}", gest.ApplicationGesture, gest.RecognitionConfidence));
                    if (StackPanelPPTControls.Visibility == Visibility.Visible) {
                        if (gest.ApplicationGesture == ApplicationGesture.Left) {
                            BtnPPTSlidesDown_Click(BtnPPTSlidesDown, null);
                        }
                        if (gest.ApplicationGesture == ApplicationGesture.Right) {
                            BtnPPTSlidesUp_Click(BtnPPTSlidesUp, null);
                        }
                    }
                }
            } catch { }
        }

        private void inkCanvas_EditingModeChanged(object sender, RoutedEventArgs e) {
            var inkCanvas1 = sender as InkCanvas;
            if (inkCanvas1 == null) return;
            if (Settings.Canvas.IsShowCursor) {
                if (inkCanvas1.EditingMode == InkCanvasEditingMode.Ink || drawingShapeMode != 0) {
                    inkCanvas1.ForceCursor = true;
                } else {
                    inkCanvas1.ForceCursor = false;
                }
            } else {
                inkCanvas1.ForceCursor = false;
            }
            if (inkCanvas1.EditingMode == InkCanvasEditingMode.Ink) forcePointEraser = !forcePointEraser;
        }

        #endregion Ink Canvas

        #region Definations and Loading

        public static Settings Settings = new Settings();
        public static string settingsFileName = "Settings.json";
        bool isLoaded = false;

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            loadPenCanvas();
            //加载设置
            LoadSettings(true);
            // HasNewUpdateWindow hasNewUpdateWindow = new HasNewUpdateWindow();
            if (Environment.Is64BitProcess) {
                GroupBoxInkRecognition.Visibility = Visibility.Collapsed;
            }

            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            SystemEvents_UserPreferenceChanged(null, null);

            //TextBlockVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            LogHelper.WriteLogToFile("Ink Canvas Loaded", LogHelper.LogType.Event);
            isLoaded = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            LogHelper.WriteLogToFile("Ink Canvas closing", LogHelper.LogType.Event);
            if (!CloseIsFromButton && Settings.Advanced.IsSecondConfimeWhenShutdownApp) {
                e.Cancel = true;
                if (MessageBox.Show("是否继续关闭 InkCanvasForClass，这将丢失当前未保存的墨迹。", "InkCanvasForClass", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK) {
                    if (MessageBox.Show("真的狠心关闭 InkCanvasForClass吗？", "InkCanvasForClass", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK) {
                        if (MessageBox.Show("是否取消关闭 InkCanvasForClass？", "InkCanvasForClass", MessageBoxButton.OKCancel, MessageBoxImage.Error) != MessageBoxResult.OK) {
                            e.Cancel = false;
                        }
                    }
                }
            }
            if (e.Cancel) {
                LogHelper.WriteLogToFile("Ink Canvas closing cancelled", LogHelper.LogType.Event);
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            LogHelper.WriteLogToFile("Ink Canvas closed", LogHelper.LogType.Event);
        }

        private async void AutoUpdate() {
            AvailableLatestVersion = await AutoUpdateHelper.CheckForUpdates();

            if (AvailableLatestVersion != null) {
                bool IsDownloadSuccessful = false;
                IsDownloadSuccessful = await AutoUpdateHelper.DownloadSetupFileAndSaveStatus(AvailableLatestVersion);

                if (IsDownloadSuccessful) {
                    if (!Settings.Startup.IsAutoUpdateWithSilence) {
                        if (MessageBox.Show("InkCanvasForClass 新版本安装包已下载完成，是否立即更新？", "InkCanvasForClass New Version Available", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                            AutoUpdateHelper.InstallNewVersionApp(AvailableLatestVersion, false);
                        }
                    } else {
                        timerCheckAutoUpdateWithSilence.Start();
                    }
                }
            } else {
                AutoUpdateHelper.DeleteUpdatesFolder();
            }
        }

        #endregion Definations and Loading

    }
}