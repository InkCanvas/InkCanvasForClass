using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using Vanara.PInvoke;
using Color = System.Windows.Media.Color;

namespace Ink_Canvas.Popups
{
    /// <summary>
    /// ScreenshotWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenshotWindow : Window {

        private MainWindow mainWindow;

        public ScreenshotWindow(MainWindow mainWin) {
            InitializeComponent();

            mainWindow = mainWin;

            iconList = new Border[] {
                FullScreenIcon,
                WindowIcon,
                SelectionIcon
            };
            iconGeometryList = new GeometryDrawing[] {
                FullScreenIconGeometry,
                WindowIconsGeometry,
                SelectionIconGeometry,
            };
            iconTextList = new TextBlock[] {
                FullScreenIconText,
                WindowIconText,
                SelectionIconText,
            };
            
            foreach (var b in iconList) {
                b.MouseLeave += IconMouseLeave;
                b.MouseUp += IconMouseUp;
                b.MouseDown += IconMouseDown;
            }

            CaptureButton.MouseUp += CaptureButton_MouseUp;
            CaptureButton.MouseDown += CaptureButton_MouseDown;
            CaptureButton.MouseLeave += CaptureButton_MouseLeave;

            UpdateModeIconSelection();
            ReArrangeWindowPosition();
            mainWin.Hide();
        }

        private Border lastDownIcon;
        private int selectedMode = 0;

        private void ReArrangeWindowPosition() {
            var workAreaWidth = SystemParameters.WorkArea.Width;
            var workAreaHeight = SystemParameters.WorkArea.Height;
            var toolbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight -
                                SystemParameters.WindowCaptionHeight;
            Left = (workAreaWidth - Width) / 2;
            Top = workAreaHeight - Height - toolbarHeight - 64;
        }

        private void UpdateModeIconSelection() {
            foreach (var b in iconList) b.Background = new SolidColorBrush(Colors.Transparent);
            foreach (var g in iconGeometryList) g.Brush = new SolidColorBrush(Color.FromRgb(24, 24, 27));
            foreach (var t in iconTextList) t.Foreground = new SolidColorBrush(Color.FromRgb(24, 24, 27));
            iconList[selectedMode].Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            iconGeometryList[selectedMode].Brush = new SolidColorBrush(Colors.White);
            iconTextList[selectedMode].Foreground = new SolidColorBrush(Colors.White);
        }

        private bool isCaptureButtonDown = false;

        private void CaptureButton_MouseDown(object sender, MouseButtonEventArgs e) {
            if (isCaptureButtonDown) return;

            isCaptureButtonDown = true;
            var sb = new Storyboard();
            var animation = new DoubleAnimation {
                From = 1,
                To = 0.9,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            var animation2 = new DoubleAnimation {
                From = 1,
                To = 0.9,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            var animation3 = new ThicknessAnimation() {
                From = new Thickness(5),
                To = new Thickness(7),
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(animation2, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            Storyboard.SetTargetProperty(animation3, new PropertyPath(Border.BorderThicknessProperty));
            animation.EasingFunction = new CubicEase();
            animation2.EasingFunction = new CubicEase();
            animation3.EasingFunction = new CubicEase();
            sb.Children.Add(animation);
            sb.Children.Add(animation2);
            sb.Children.Add(animation3);
            sb.Begin(CaptureButton);
        }

        private void CaptureButton_MouseUp(object sender, MouseButtonEventArgs e) {
            if (isCaptureButtonDown != true) return;
            CaptureButton_MouseLeave(sender, null);

            if (selectedMode == 0) CaptureFullScreen();
        }

        private async void CaptureFullScreen() {
            try {
                var bm = await mainWindow.FullscreenSnapshot(new MainWindow.SnapshotConfig() {
                    BitmapSavePath =
                        new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)),
                    ExcludedHwnds = new HWND[] {
                        new HWND(new WindowInteropHelper(this).Handle)
                    },
                    IsCopyToClipboard = true,
                    IsSaveToLocal = true,
                    OutputMIMEType = MainWindow.OutputImageMIMEFormat.Png,
                });
                bm.Dispose();
                mainWindow.ShowNewToast("已保存截图到桌面！", MW_Toast.ToastType.Success, 3000);
                await Task.Delay(1);
                Close();
            }
            catch (Exception e) {
                mainWindow.ShowNewToast($"截图失败！{e.Message}", MW_Toast.ToastType.Error, 3000);
                await Task.Delay(1);
                Close();
            }
        }

        private void CaptureButton_MouseLeave(object sender, MouseEventArgs e) {
            if (isCaptureButtonDown != true) return;

            var sb = new Storyboard();
            var animation = new DoubleAnimation {
                From = 0.9,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            var animation2 = new DoubleAnimation {
                From = 0.9,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            var animation3 = new ThicknessAnimation() {
                From = new Thickness(7),
                To = new Thickness(5),
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(animation2, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            Storyboard.SetTargetProperty(animation3, new PropertyPath(Border.BorderThicknessProperty));
            animation.EasingFunction = new CubicEase();
            animation2.EasingFunction = new CubicEase();
            animation3.EasingFunction = new CubicEase();
            sb.Children.Add(animation);
            sb.Children.Add(animation2);
            sb.Children.Add(animation3);
            sb.Begin(CaptureButton);

            isCaptureButtonDown = false;
        } 

        private void IconMouseLeave(object sender, MouseEventArgs e) {
            if (lastDownIcon == null) return;
            lastDownIcon = null;
            var b = (Border)sender;
            if (Array.IndexOf(iconList,b)!=selectedMode)
                b.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void IconMouseDown(object sender, MouseButtonEventArgs e) {
            if (lastDownIcon != null) return;
            lastDownIcon = (Border)sender;
            var b = (Border)sender;
            if (Array.IndexOf(iconList,b)!=selectedMode)
                b.Background = new SolidColorBrush(Color.FromArgb(22, 24, 24, 27));
        }

        private WindowScreenshotGridWindow _screenshotGridWindow = null;

        private async void IconMouseUp(object sender, MouseButtonEventArgs e) {
            if (lastDownIcon == null) return;
            IconMouseLeave(sender, null);
            var index = Array.IndexOf(iconList, (Border)sender);
            selectedMode = index;
            UpdateModeIconSelection();

            if (selectedMode == 1) {
                try {
                    MainWindow.WindowInformation[] windows = await mainWindow.GetAllWindowsAsync(new HWND[] {
                        new HWND(new WindowInteropHelper(this).Handle), new HWND(new WindowInteropHelper(mainWindow).Handle)
                    });
                    _screenshotGridWindow = new WindowScreenshotGridWindow(windows, mainWindow);
                    _screenshotGridWindow.Show();
                }
                catch (TaskCanceledException) {}
                catch (Exception ex) {}
            } else {
                try {
                    _screenshotGridWindow.Close();
                } catch (Exception ex) { }
                
            }
        }

        private Border[] iconList = new Border[] { };
        private GeometryDrawing[] iconGeometryList = new GeometryDrawing[] { };
        private TextBlock[] iconTextList = new TextBlock[] { };

        private void CloseButton_CloseWindow(object sender, MouseButtonEventArgs e) {
            Close();
        }

        protected override void OnClosed(EventArgs e) {
            mainWindow.Show();
            base.OnClosed(e);
        }
    }
}
