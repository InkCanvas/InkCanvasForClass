using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using OSVersionExtension;
using Vanara.PInvoke;
using OperatingSystem = OSVersionExtension.OperatingSystem;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Ink_Canvas
{
    public partial class MainWindow : Window
    {
        private void SaveScreenshot(bool isHideNotification, string fileName = null)
        {
            var bitmap = GetScreenshotBitmap();
            string savePath = Settings.Automation.AutoSavedStrokesLocation + @"\Auto Saved - Screenshots";
            if (fileName == null) fileName = DateTime.Now.ToString("u").Replace(":", "-");
            if (Settings.Automation.IsSaveScreenshotsInDateFolders)
            {
                savePath += @"\" + DateTime.Now.ToString("yyyy-MM-dd");
            }
            savePath += @"\" + fileName + ".png";
            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }
            bitmap.Save(savePath, ImageFormat.Png);
            if (Settings.Automation.IsAutoSaveStrokesAtScreenshot)
            {
                SaveInkCanvasStrokes(false, false);
            }
            if (!isHideNotification)
            {
                ShowNewToast("截图成功保存至 " + savePath, MW_Toast.ToastType.Success, 3000);
            }
        }

        #region MagnificationAPI 獲取屏幕截圖並過濾ICC窗口

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint="SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint="SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint="GetWindowLong")]
        static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError=true)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            string lpClassName,
            string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        public void SaveScreenshotToDesktopByMagnificationAPI(HWND[] excludedHwnds, Action<Bitmap> callbackAction) {

            if (OSVersion.GetOperatingSystem() < OperatingSystem.Windows8 && OSVersion.GetOperatingSystem() > OperatingSystem.Windows10) return;
            if (!Magnification.MagInitialize()) return;

            // 創建宿主窗體
            var mainWinMag = new Window();
            mainWinMag.WindowState = WindowState.Maximized;
            mainWinMag.WindowStyle = WindowStyle.None;
            mainWinMag.ResizeMode = ResizeMode.NoResize;
            mainWinMag.Background = new SolidColorBrush(Colors.Transparent);
            mainWinMag.AllowsTransparency = true;
            mainWinMag.Show();
            var handle = new WindowInteropHelper(mainWinMag).Handle;
            SetWindowPos(handle, new IntPtr(1), 0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, 0x0080); // SWP_HIDEWINDOW
            SetWindowLongPtr(handle, -20, new IntPtr((int)GetWindowLongPtr(handle, -20) | 0x00080000));
            SetLayeredWindowAttributes(handle,0, 255, (byte)0x2);  

            // 創建放大鏡窗體（使用Win32方法）
            var hwndMag = CreateWindowEx(0,"Magnifier", "ICCMagnifierWindow", (uint)(0x40000000L | 0x10000000L), 0, 0,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            // 過濾窗口
            var hwnds = new List<HWND> { new HWND(hwndMag) };
            hwnds.AddRange(excludedHwnds);
            if (!Magnification.MagSetWindowFilterList(new HWND(hwndMag),
                    Magnification.MW_FILTERMODE.MW_FILTERMODE_EXCLUDE, hwnds.Count, hwnds.ToArray())) return;

            // 保存數據
            if (!Magnification.MagSetImageScalingCallback(new HWND(hwndMag),
                    (hwnd, srcdata, srcheader, destdata, destheader, unclipped, clipped, dirty) => {
                        Bitmap bm = new Bitmap((int)srcheader.width, (int)srcheader.height, (int)srcheader.width * 4,
                            PixelFormat.Format32bppRgb, srcdata);
                        callbackAction(bm);
                        return true;
                    })) return;

            // 設置窗口Source
            if (!Magnification.MagSetWindowSource(new HWND(hwndMag),
                    new RECT(0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                        System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height))) return;

            // 關閉宿主窗體
            mainWinMag.Close();
        }

        #endregion

        #region 窗口截图

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size > 4)
                return GetClassLongPtr64(hWnd, nIndex);
            else
                return new IntPtr(GetClassLongPtr32(hWnd, nIndex));
        }
        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);
 
        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        public static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, Delegate lpEnumCallbackFunction, IntPtr lParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "GetWindowText",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr handle, out RECT rect);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        public Icon GetAppIcon(IntPtr hwnd) {
            IntPtr iconHandle = SendMessage(hwnd,0x7F,2,0);
            if(iconHandle == IntPtr.Zero)
                iconHandle = SendMessage(hwnd,0x7F,0,0);
            if(iconHandle == IntPtr.Zero)
                iconHandle = SendMessage(hwnd,0x7F,1,0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, -14);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, -34);
            if(iconHandle == IntPtr.Zero)
                return null;
            Icon icn = System.Drawing.Icon.FromHandle(iconHandle);
            return icn;
        }

        public struct WindowInformation {
            public string Title;
            public Bitmap WindowBitmap;
            public Icon AppIcon;
            public bool IsVisible;
            public int Width;
            public int Height;
            public RECT Rect;
            public WINDOWPLACEMENT Placement;
        }

        public struct WINDOWPLACEMENT {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        public delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);

        public WindowInformation[] GetAllWindows() {
            var windows = new List<WindowInformation>();
            if (!EnumDesktopWindows(IntPtr.Zero, new EnumDesktopWindowsDelegate((hwnd, param) => {
                        var isvisible = IsWindowVisible(hwnd);
                        if (!isvisible) return true;
                        var icon = GetAppIcon(hwnd);
                        var length = GetWindowTextLength(hwnd) + 1;
                        var title = new StringBuilder(length);
                        GetWindowText(hwnd, title, length);
                        if (title.ToString().Length == 0) return true;
                        RECT rect;
                        WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                        GetWindowPlacement(hwnd, ref placement);
                        if (placement.showCmd == 2) return true;
                        GetWindowRect(hwnd, out rect);
                        var w = rect.Width;
                        var h = rect.Height;
                        if (w == 0 || h == 0) return true;
                        Bitmap bmp = new Bitmap(rect.Width, rect.Height);
                        Graphics memoryGraphics = Graphics.FromImage(bmp);
                        IntPtr hdc = memoryGraphics.GetHdc();
                        PrintWindow(hwnd, hdc, 2);
                        windows.Add(new WindowInformation() {
                            AppIcon = icon,
                            Title = title.ToString(),
                            IsVisible = isvisible,
                            WindowBitmap = bmp,
                            Width = w,
                            Height = h,
                            Rect = rect,
                            Placement = placement,
                        });
                        memoryGraphics.ReleaseHdc(hdc);
                        return true;
                    }),
                    IntPtr.Zero)) return new WindowInformation[]{};
            return windows.ToArray();
        }

        #endregion

        private void SaveScreenShotToDesktop() {
            var bitmap = GetScreenshotBitmap();
            string savePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            bitmap.Save(savePath + @"\" + DateTime.Now.ToString("u").Replace(':', '-') + ".png", ImageFormat.Png);
            ShowNewToast("截图成功保存至【桌面" + @"\" + DateTime.Now.ToString("u").Replace(':', '-') + ".png】", MW_Toast.ToastType.Success, 3000);
            if (Settings.Automation.IsAutoSaveStrokesAtScreenshot) SaveInkCanvasStrokes(false, false);
        }

        private void SavePPTScreenshot(string fileName)
        {
            var bitmap = GetScreenshotBitmap();
            string savePath = Settings.Automation.AutoSavedStrokesLocation + @"\Auto Saved - PPT Screenshots";
            if (Settings.Automation.IsSaveScreenshotsInDateFolders)
            {
                savePath += @"\" + DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (fileName == null) fileName = DateTime.Now.ToString("u").Replace(":", "-");
            savePath += @"\" + fileName + ".png";
            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }
            bitmap.Save(savePath, ImageFormat.Png);
            if (Settings.Automation.IsAutoSaveStrokesAtScreenshot)
            {
                SaveInkCanvasStrokes(false, false);
            }
        }

        private Bitmap GetScreenshotBitmap()
        {
            Rectangle rc = System.Windows.Forms.SystemInformation.VirtualScreen;
            var bitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
            {
                memoryGrahics.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }
    }
}