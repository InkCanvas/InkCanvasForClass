using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using Acornima.Ast;
using OSVersionExtension;
using Vanara.PInvoke;
using static Vanara.PInvoke.Gdi32;
using Color = System.Drawing.Color;
using OperatingSystem = OSVersionExtension.OperatingSystem;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Ink_Canvas {
    public partial class MainWindow : Window {
        private void SaveScreenshot(bool isHideNotification, string fileName = null) {
            var bitmap = GetScreenshotBitmap();
            string savePath = Settings.Automation.AutoSavedStrokesLocation + @"\Auto Saved - Screenshots";
            if (fileName == null) fileName = DateTime.Now.ToString("u").Replace(":", "-");
            if (Settings.Automation.IsSaveScreenshotsInDateFolders) {
                savePath += @"\" + DateTime.Now.ToString("yyyy-MM-dd");
            }

            savePath += @"\" + fileName + ".png";
            if (!Directory.Exists(Path.GetDirectoryName(savePath))) {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }

            bitmap.Save(savePath, ImageFormat.Png);
            if (Settings.Automation.IsAutoSaveStrokesAtScreenshot) {
                SaveInkCanvasStrokes(false, false);
            }

            if (!isHideNotification) {
                ShowNewToast("截图成功保存至 " + savePath, MW_Toast.ToastType.Success, 3000);
            }
        }

        #region MagnificationAPI 獲取屏幕截圖並過濾ICC窗口

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        public unsafe void SaveScreenshotToDesktopByMagnificationAPIEx(bool isExcludeMode, HWND[] hwndsList,
            Action<Bitmap> callbackAction) {
            if (OSVersion.GetOperatingSystem() < OperatingSystem.Windows8 &&
                OSVersion.GetOperatingSystem() > OperatingSystem.Windows10) return;
            if (!Magnification.MagInitialize()) return;

            // 創建宿主窗體
            var mainWinMag = new Form();
            mainWinMag.Show();
            var handle = new HWND(mainWinMag.Handle);
            User32.SetWindowPos(handle, HWND.HWND_NOTOPMOST, 0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, User32.SetWindowPosFlags.SWP_HIDEWINDOW); // SWP_HIDEWINDOW
            Trace.WriteLine(handle);
            User32.SetWindowLong(handle, User32.WindowLongFlags.GWL_STYLE,
                new IntPtr((int)User32.WindowStyles.WS_SIZEBOX | (int)User32.WindowStyles.WS_SYSMENU | (int)User32.WindowStyles.WS_CLIPCHILDREN | (int)User32.WindowStyles.WS_CAPTION | (int)User32.WindowStyles.WS_MAXIMIZEBOX));
            Trace.WriteLine(handle);
            var exptr = User32.GetWindowLong(handle, User32.WindowLongFlags.GWL_EXSTYLE);
            Trace.WriteLine(exptr);
            User32.SetWindowLong(handle, User32.WindowLongFlags.GWL_EXSTYLE,
                new IntPtr(exptr |
                           (int)User32.WindowStylesEx.WS_EX_LAYERED | (int)User32.WindowStylesEx.WS_EX_TOPMOST));
            Trace.WriteLine(handle);
            User32.SetLayeredWindowAttributes(handle,0, 255, User32.LayeredWindowAttributes.LWA_ALPHA);

            Trace.WriteLine(handle);

            // 創建放大鏡窗體（使用Win32方法）
            var hwndMag = User32.CreateWindow(Magnification.WC_MAGNIFIER, "ICCMagnifierWindow",
                User32.WindowStyles.WS_CHILD | User32.WindowStyles.WS_VISIBLE, 0, 0,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, handle, HMENU.NULL, HINSTANCE.NULL,
                IntPtr.Zero);

            Trace.WriteLine(hwndMag);

            // 過濾窗口
            var hwnds = new List<HWND> { hwndMag };
            hwnds.AddRange(hwndsList);
            if (!Magnification.MagSetWindowFilterList(hwndMag,
                    isExcludeMode
                        ? Magnification.MW_FILTERMODE.MW_FILTERMODE_EXCLUDE
                        : Magnification.MW_FILTERMODE.MW_FILTERMODE_INCLUDE,
                    isExcludeMode ? hwnds.Count : hwndsList.Length, hwnds.ToArray())) return;

            // 設置窗口Source
            if (!Magnification.MagSetWindowSource(hwndMag,
                    new RECT(0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                        System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height))) return;

            RECT rect;
            User32.GetWindowRect(hwndMag, out rect);
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            Graphics memoryGraphics = Graphics.FromImage(bmp);
            IntPtr hdc = memoryGraphics.GetHdc();
            Trace.WriteLine(User32.PrintWindow(hwndMag, hdc, User32.PW.PW_RENDERFULLCONTENT));

            string savePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            bmp.Save(savePath + @"\" + DateTime.Now.ToString("u").Replace(':', '-') + ".png", ImageFormat.Png);

            // 關閉宿主窗體
            //Magnification.MagUninitialize();
            //mainWinMag.Close();
        }

        public unsafe void SaveScreenshotToDesktopByMagnificationAPI(bool isExcludeMode, HWND[] hwndsList,
            Action<Bitmap> callbackAction) {
            if (OSVersion.GetOperatingSystem() < OperatingSystem.Windows8 &&
                OSVersion.GetOperatingSystem() > OperatingSystem.Windows10) return;
            if (!Magnification.MagInitialize()) return;

            // 創建宿主窗體
            var mainWinMag = new Window();
            mainWinMag.WindowState = WindowState.Maximized;
            mainWinMag.WindowStyle = WindowStyle.None;
            mainWinMag.ResizeMode = ResizeMode.NoResize;
            mainWinMag.Background = new SolidColorBrush(Colors.Transparent);
            mainWinMag.AllowsTransparency = true;
            mainWinMag.Show();
            var handle = new HWND(new WindowInteropHelper(mainWinMag).Handle);
            User32.SetWindowPos(handle, HWND.HWND_NOTOPMOST, 0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, User32.SetWindowPosFlags.SWP_HIDEWINDOW); // SWP_HIDEWINDOW
            SetWindowLongPtr(handle.DangerousGetHandle(), -20, new IntPtr((int)GetWindowLongPtr(handle.DangerousGetHandle(), -20) | 0x00080000));
            User32.SetLayeredWindowAttributes(handle,0, 255, User32.LayeredWindowAttributes.LWA_ALPHA);

            // 創建放大鏡窗體（使用Win32方法）
            var hwndMag = User32.CreateWindow(Magnification.WC_MAGNIFIER, "ICCMagnifierWindow",
                User32.WindowStyles.WS_CHILD | User32.WindowStyles.WS_VISIBLE, 0, 0,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, handle, HMENU.NULL, HINSTANCE.NULL,
                IntPtr.Zero);

            Trace.WriteLine(hwndMag);

            // 過濾窗口
            var hwnds = new List<HWND> { hwndMag };
            hwnds.AddRange(hwndsList);
            if (!Magnification.MagSetWindowFilterList(hwndMag,
                    isExcludeMode
                        ? Magnification.MW_FILTERMODE.MW_FILTERMODE_EXCLUDE
                        : Magnification.MW_FILTERMODE.MW_FILTERMODE_INCLUDE,
                    isExcludeMode ? hwnds.Count : hwndsList.Length, hwnds.ToArray())) return;

            // 保存數據
            if (!Magnification.MagSetImageScalingCallback(hwndMag,
                    (hwnd, srcdata, srcheader, destdata, destheader, unclipped, clipped, dirty) => {
                        Bitmap bm = new Bitmap((int)srcheader.width, (int)srcheader.height, (int)srcheader.width * 4,
                            PixelFormat.Format32bppRgb, srcdata);
                        callbackAction(bm);
                        return true;
                    })) return;

            // 設置窗口Source
            if (!Magnification.MagSetWindowSource(hwndMag,
                    new RECT(0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                        System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height))) return;

            // 關閉宿主窗體
            Magnification.MagUninitialize();
            mainWinMag.Close();
        }

        #endregion

        #region 窗口截图（復刻Powerpoint）

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex) {
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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName,
            string windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetShellWindow();

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out bool pvAttribute, int cbAttribute);

        public Icon GetAppIcon(IntPtr hwnd) {
            IntPtr iconHandle = SendMessage(hwnd, 0x7F, 2, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = SendMessage(hwnd, 0x7F, 0, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = SendMessage(hwnd, 0x7F, 1, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, -14);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, -34);
            if (iconHandle == IntPtr.Zero)
                return null;
            Icon icn = System.Drawing.Icon.FromHandle(iconHandle);
            return icn;
        }

        public class WindowInformation {
            public string Title { get; set; }
            public Bitmap WindowBitmap { get; set; }
            public Icon AppIcon { get; set; }
            public bool IsVisible { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public RECT Rect { get; set; }
            public WINDOWPLACEMENT Placement { get; set; }
            public HWND hwnd { get; set; }
        }

        public struct WINDOWPLACEMENT {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;

            public static WINDOWPLACEMENT Default {
                get {
                    WINDOWPLACEMENT result = new WINDOWPLACEMENT();
                    result.length = Marshal.SizeOf(result);
                    return result;
                }
            }
        }

        public delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);

        public WindowInformation[] GetAllWindows(HWND[] excludedHwnds) {
            var windows = new List<WindowInformation>();
            IntPtr hShellWnd = GetShellWindow();
            IntPtr hDefView = FindWindowEx(hShellWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
            IntPtr folderView = FindWindowEx(hDefView, IntPtr.Zero, "SysListView32", null);
            IntPtr taskBar = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
            var excluded = new List<HWND>() {
                new HWND(hShellWnd), new HWND(hDefView), new HWND(folderView), new HWND(taskBar)
            };
            var excludedWindowTitle = new string[] {
                "NVIDIA GeForce Overlay"
            };
            excluded.AddRange(excludedHwnds);
            if (!EnumDesktopWindows(IntPtr.Zero, new EnumDesktopWindowsDelegate((hwnd, param) => {
                        if (excluded.Contains(new HWND(hwnd))) return true;
                        var isvisible = IsWindowVisible(hwnd);
                        if (!isvisible) return true;
                        var windowLong = (int)GetWindowLongPtr(hwnd, -20);
                        if ((windowLong & 0x00000080L) != 0) return true;
                        DwmGetWindowAttribute(hwnd, 14, out bool isCloacked, Marshal.SizeOf(typeof(bool)));
                        if (isCloacked) return true;
                        var icon = GetAppIcon(hwnd);
                        var length = GetWindowTextLength(hwnd) + 1;
                        var title = new StringBuilder(length);
                        GetWindowText(hwnd, title, length);
                        if (title.ToString().Length == 0) return true;
                        // 這裡懶得做 Alt Tab窗口的校驗了，直接窗體標題黑名單
                        if (excludedWindowTitle.Contains(title.ToString())) return true;
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
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        return true;
                    }),
                    IntPtr.Zero)) return new WindowInformation[] { };
            return windows.ToArray();
        }

        public async Task<WindowInformation[]> GetAllWindowsAsync(HWND[] excludedHwnds)
        {
            var windows = await Task.Run(() => GetAllWindows(excludedHwnds));
            return windows;
        }

        #endregion

        private void SaveScreenShotToDesktop() {
            var bitmap = GetScreenshotBitmap();
            string savePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            bitmap.Save(savePath + @"\" + DateTime.Now.ToString("u").Replace(':', '-') + ".png", ImageFormat.Png);
            ShowNewToast("截图成功保存至【桌面" + @"\" + DateTime.Now.ToString("u").Replace(':', '-') + ".png】",
                MW_Toast.ToastType.Success, 3000);
            if (Settings.Automation.IsAutoSaveStrokesAtScreenshot) SaveInkCanvasStrokes(false, false);
        }

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private void SaveFullScreenScreenshotToDesktopAutoWays(HWND[] excludedHwnds) {
            if (OSVersion.GetOperatingSystem() < OperatingSystem.Windows8 &&
                OSVersion.GetOperatingSystem() > OperatingSystem.Windows10) {
                foreach (var excludedHwnd in excludedHwnds) ShowWindow(excludedHwnd.DangerousGetHandle(),0);
                SaveScreenShotToDesktop();
                foreach (var excludedHwnd in excludedHwnds) ShowWindow(excludedHwnd.DangerousGetHandle(),5);
            } else SaveScreenshotToDesktopByMagnificationAPI(true,excludedHwnds, bitmap => {
                    string savePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    bitmap.Save(savePath + @"\" + DateTime.Now.ToString("u").Replace(':', '-') + ".png", ImageFormat.Png);
                });
        }

        private void SavePPTScreenshot(string fileName) {
            var bitmap = GetScreenshotBitmap();
            string savePath = Settings.Automation.AutoSavedStrokesLocation + @"\Auto Saved - PPT Screenshots";
            if (Settings.Automation.IsSaveScreenshotsInDateFolders) {
                savePath += @"\" + DateTime.Now.ToString("yyyy-MM-dd");
            }

            if (fileName == null) fileName = DateTime.Now.ToString("u").Replace(":", "-");
            savePath += @"\" + fileName + ".png";
            if (!Directory.Exists(Path.GetDirectoryName(savePath))) {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }

            bitmap.Save(savePath, ImageFormat.Png);
            if (Settings.Automation.IsAutoSaveStrokesAtScreenshot) {
                SaveInkCanvasStrokes(false, false);
            }
        }

        private Bitmap GetScreenshotBitmap() {
            Rectangle rc = System.Windows.Forms.SystemInformation.VirtualScreen;
            var bitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            using (Graphics memoryGrahics = Graphics.FromImage(bitmap)) {
                memoryGrahics.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }
    }
}