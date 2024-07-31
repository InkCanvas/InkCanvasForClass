using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
		[DllImport("user32.dll")]
		static extern bool DestroyWindow(IntPtr hWnd);

		Bitmap RemoveImageTransparancy(Bitmap src) {
			Bitmap target = new Bitmap(src.Size.Width,src.Size.Height);
			Graphics g = Graphics.FromImage(target);
			g.DrawRectangle(new System.Drawing.Pen(new SolidBrush(System.Drawing.Color.White)), 0, 0, target.Width, target.Height);
			g.DrawImage(src, 0, 0);
			return target;
		}

		public enum WindowStyles : uint
	{
		/// <summary>The window has a thin-line border.</summary>
		WS_BORDER = 0x800000,

		/// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
		WS_CAPTION = 0xc00000,

		/// <summary>
		/// The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.
		/// </summary>
		WS_CHILD = 0x40000000,

		/// <summary>
		/// Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating
		/// the parent window.
		/// </summary>
		WS_CLIPCHILDREN = 0x2000000,

		/// <summary>
		/// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the
		/// WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated. If
		/// WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child
		/// window, to draw within the client area of a neighboring child window.
		/// </summary>
		WS_CLIPSIBLINGS = 0x4000000,

		/// <summary>
		/// The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has
		/// been created, use the EnableWindow function.
		/// </summary>
		WS_DISABLED = 0x8000000,

		/// <summary>
		/// The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.
		/// </summary>
		WS_DLGFRAME = 0x400000,

		/// <summary>
		/// The window is the first control of a group of controls. The group consists of this first control and all controls defined
		/// after it, up to the next control with the WS_GROUP style. The first control in each group usually has the WS_TABSTOP style so
		/// that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group
		/// to the next control in the group by using the direction keys. You can turn this style on and off to change dialog box
		/// navigation. To change this style after a window has been created, use the SetWindowLong function.
		/// </summary>
		WS_GROUP = 0x20000,

		/// <summary>The window has a horizontal scroll bar.</summary>
		WS_HSCROLL = 0x100000,

		/// <summary>The window is initially maximized.</summary>
		WS_MAXIMIZE = 0x1000000,

		/// <summary>
		/// The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.
		/// </summary>
		WS_MAXIMIZEBOX = 0x10000,

		/// <summary>The window is initially minimized.</summary>
		WS_MINIMIZE = 0x20000000,

		/// <summary>
		/// The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.
		/// </summary>
		WS_MINIMIZEBOX = 0x20000,

		/// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
		WS_OVERLAPPED = 0x0,

		/// <summary>The window is an overlapped window.</summary>
		WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

		/// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
		WS_POPUP = 0x80000000u,

		/// <summary>
		/// The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.
		/// </summary>
		WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,

		/// <summary>The window has a sizing border.</summary>
		WS_THICKFRAME = 0x40000,

		/// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
		WS_SYSMENU = 0x80000,

		/// <summary>
		/// The window is a control that can receive the keyboard focus when the user presses the TAB key. Pressing the TAB key changes
		/// the keyboard focus to the next control with the WS_TABSTOP style. You can turn this style on and off to change dialog box
		/// navigation. To change this style after a window has been created, use the SetWindowLong function. For user-created windows
		/// and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
		/// </summary>
		WS_TABSTOP = 0x10000,

		/// <summary>
		/// The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.
		/// </summary>
		WS_VISIBLE = 0x10000000,

		/// <summary>The window has a vertical scroll bar.</summary>
		WS_VSCROLL = 0x200000,

		/// <summary>
		/// The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_OVERLAPPED style.
		/// </summary>
		WS_TILED = WS_OVERLAPPED,

		/// <summary>The window is initially minimized. Same as the WS_MINIMIZE style.</summary>
		WS_ICONIC = WS_MINIMIZE,

		/// <summary>The window has a sizing border. Same as the WS_THICKFRAME style.</summary>
		WS_SIZEBOX = WS_THICKFRAME,

		/// <summary>The window is an overlapped window. Same as the WS_OVERLAPPEDWINDOW style.</summary>
		WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

		/// <summary>Same as the WS_CHILD style.</summary>
		WS_CHILDWINDOW = WS_CHILD,
	}

		public enum WindowStylesEx : uint
	{
		/// <summary>Specifies a window that accepts drag-drop files.</summary>
		WS_EX_ACCEPTFILES = 0x00000010,

		/// <summary>Forces a top-level window onto the taskbar when the window is visible.</summary>
		WS_EX_APPWINDOW = 0x00040000,

		/// <summary>Specifies a window that has a border with a sunken edge.</summary>
		WS_EX_CLIENTEDGE = 0x00000200,

		/// <summary>
		/// Specifies a window that paints all descendants in bottom-to-top painting order using double-buffering. This cannot be used if
		/// the window has a class style of either CS_OWNDC or CS_CLASSDC. This style is not supported in Windows 2000.
		/// </summary>
		/// <remarks>
		/// With WS_EX_COMPOSITED set, all descendants of a window get bottom-to-top painting order using double-buffering. Bottom-to-top
		/// painting order allows a descendent window to have translucency (alpha) and transparency (color-key) effects, but only if the
		/// descendent window also has the WS_EX_TRANSPARENT bit set. Double-buffering allows the window and its descendents to be
		/// painted without flicker.
		/// </remarks>
		WS_EX_COMPOSITED = 0x02000000,

		/// <summary>
		/// Specifies a window that includes a question mark in the title bar. When the user clicks the question mark, the cursor changes
		/// to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message. The child
		/// window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP
		/// command. The Help application displays a pop-up window that typically contains help for the child window. WS_EX_CONTEXTHELP
		/// cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
		/// </summary>
		WS_EX_CONTEXTHELP = 0x00000400,

		/// <summary>
		/// Specifies a window which contains child windows that should take part in dialog box navigation. If this style is specified,
		/// the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key,
		/// an arrow key, or a keyboard mnemonic.
		/// </summary>
		WS_EX_CONTROLPARENT = 0x00010000,

		/// <summary>Specifies a window that has a double border.</summary>
		WS_EX_DLGMODALFRAME = 0x00000001,

		/// <summary>
		/// Specifies a window that is a layered window. This cannot be used for child windows or if the window has a class style of
		/// either CS_OWNDC or CS_CLASSDC.
		/// </summary>
		WS_EX_LAYERED = 0x00080000,

		/// <summary>
		/// Specifies a window with the horizontal origin on the right edge. Increasing horizontal values advance to the left. The shell
		/// language must support reading-order alignment for this to take effect.
		/// </summary>
		WS_EX_LAYOUTRTL = 0x00400000,

		/// <summary>Specifies a window that has generic left-aligned properties. This is the default.</summary>
		WS_EX_LEFT = 0x00000000,

		/// <summary>
		/// Specifies a window with the vertical scroll bar (if present) to the left of the client area. The shell language must support
		/// reading-order alignment for this to take effect.
		/// </summary>
		WS_EX_LEFTSCROLLBAR = 0x00004000,

		/// <summary>Specifies a window that displays text using left-to-right reading-order properties. This is the default.</summary>
		WS_EX_LTRREADING = 0x00000000,

		/// <summary>Specifies a multiple-document interface (MDI) child window.</summary>
		WS_EX_MDICHILD = 0x00000040,

		/// <summary>
		/// Specifies a top-level window created with this style does not become the foreground window when the user clicks it. The
		/// system does not bring this window to the foreground when the user minimizes or closes the foreground window. The window does
		/// not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style. To
		/// activate the window, use the SetActiveWindow or SetForegroundWindow function.
		/// </summary>
		WS_EX_NOACTIVATE = 0x08000000,

		/// <summary>Specifies a window which does not pass its window layout to its child windows.</summary>
		WS_EX_NOINHERITLAYOUT = 0x00100000,

		/// <summary>
		/// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it
		/// is created or destroyed.
		/// </summary>
		WS_EX_NOPARENTNOTIFY = 0x00000004,

		/// <summary>
		/// The window does not render to a redirection surface. This is for windows that do not have visible content or that use
		/// mechanisms other than surfaces to provide their visual.
		/// </summary>
		WS_EX_NOREDIRECTIONBITMAP = 0x00200000,

		/// <summary>Specifies an overlapped window.</summary>
		WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,

		/// <summary>Specifies a palette window, which is a modeless dialog box that presents an array of commands.</summary>
		WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

		/// <summary>
		/// Specifies a window that has generic "right-aligned" properties. This depends on the window class. The shell language must
		/// support reading-order alignment for this to take effect. Using the WS_EX_RIGHT style has the same effect as using the
		/// SS_RIGHT (static), ES_RIGHT (edit), and BS_RIGHT/BS_RIGHTBUTTON (button) control styles.
		/// </summary>
		WS_EX_RIGHT = 0x00001000,

		/// <summary>Specifies a window with the vertical scroll bar (if present) to the right of the client area. This is the default.</summary>
		WS_EX_RIGHTSCROLLBAR = 0x00000000,

		/// <summary>
		/// Specifies a window that displays text using right-to-left reading-order properties. The shell language must support
		/// reading-order alignment for this to take effect.
		/// </summary>
		WS_EX_RTLREADING = 0x00002000,

		/// <summary>
		/// Specifies a window with a three-dimensional border style intended to be used for items that do not accept user input.
		/// </summary>
		WS_EX_STATICEDGE = 0x00020000,

		/// <summary>
		/// Specifies a window that is intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a
		/// normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the
		/// dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title
		/// bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
		/// </summary>
		WS_EX_TOOLWINDOW = 0x00000080,

		/// <summary>
		/// Specifies a window that should be placed above all non-topmost windows and should stay above them, even when the window is
		/// deactivated. To add or remove this style, use the SetWindowPos function.
		/// </summary>
		WS_EX_TOPMOST = 0x00000008,

		/// <summary>
		/// Specifies a window that should not be painted until siblings beneath the window (that were created by the same thread) have
		/// been painted. The window appears transparent because the bits of underlying sibling windows have already been painted. To
		/// achieve transparency without these restrictions, use the SetWindowRgn function.
		/// </summary>
		WS_EX_TRANSPARENT = 0x00000020,

		/// <summary>Specifies a window that has a border with a raised edge.</summary>
		WS_EX_WINDOWEDGE = 0x00000100,
	}

		public void SaveScreenshotToDesktopByMagnificationAPI(bool isExcludeMode, HWND[] hwndsList, Action<Bitmap> callbackAction) {

			if (OSVersion.GetOperatingSystem() < OperatingSystem.Windows8 && OSVersion.GetOperatingSystem() > OperatingSystem.Windows10) return;
			if (!Magnification.MagInitialize()) return;

			// 創建宿主窗體
			//var mainWinMag = new Window();
			//mainWinMag.WindowState = WindowState.Maximized;
			//mainWinMag.WindowStyle = WindowStyle.None;
			//mainWinMag.ResizeMode = ResizeMode.NoResize;
			//mainWinMag.Background = new SolidColorBrush(Colors.Transparent);
			//mainWinMag.AllowsTransparency = true;
			//mainWinMag.Show();
			
			var mainWin = CreateWindowEx((uint)WindowStylesEx.WS_EX_TOPMOST | (uint)WindowStylesEx.WS_EX_LAYERED, "ICCMagnificationHostWinClass",
				"ICCMagnificationHostWin",
				(uint)WindowStyles.WS_SIZEBOX | (uint)WindowStyles.WS_SYSMENU | (uint)WindowStyles.WS_CLIPCHILDREN | (uint)WindowStyles.WS_CAPTION | (uint)WindowStyles.WS_MAXIMIZEBOX, 0, 0,
				System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
				System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
				IntPtr.Zero);

			Trace.WriteLine(mainWin);

			//SetWindowPos(handle, new IntPtr(1), 0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
			//    System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, 0x0080); // SWP_HIDEWINDOW
			//SetWindowLongPtr(handle, -20, new IntPtr((int)GetWindowLongPtr(handle, -20) | 0x00080000));
			//SetLayeredWindowAttributes(handle,0, 255, (byte)0x2);  
			SetLayeredWindowAttributes(mainWin,0, 255, (byte)0x2);  

			// 創建放大鏡窗體（使用Win32方法）
			var hwndMag = CreateWindowEx(0,"Magnifier", "ICCMagnifierWindow", (uint)WindowStyles.WS_CHILD | (uint)WindowStyles.WS_VISIBLE, 0, 0,
				System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
				System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, mainWin, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			Trace.WriteLine(hwndMag);

			// 過濾窗口
			var hwnds = new List<HWND> { new HWND(hwndMag) };
			hwnds.AddRange(hwndsList);
			if (!Magnification.MagSetWindowFilterList(new HWND(hwndMag),
					isExcludeMode ? Magnification.MW_FILTERMODE.MW_FILTERMODE_EXCLUDE
					 : Magnification.MW_FILTERMODE.MW_FILTERMODE_INCLUDE, isExcludeMode ? hwnds.Count : hwndsList.Length, hwnds.ToArray())) return;

			// 保存數據
			// 不使用Callback，因為該方法已經被廢棄了
			//if (!Magnification.MagSetImageScalingCallback(new HWND(hwndMag),
			//        (hwnd, srcdata, srcheader, destdata, destheader, unclipped, clipped, dirty) => {
			//            Bitmap bm = new Bitmap((int)srcheader.width, (int)srcheader.height, (int)srcheader.width * 4,
			//                PixelFormat.Format32bppRgb, srcdata);
			//            callbackAction(bm);
			//            return true;
			//        })) return;

			// 設置窗口Source
			if (!Magnification.MagSetWindowSource(new HWND(hwndMag),
					new RECT(0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
						System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height))) return;

			
			RECT rect;
			GetWindowRect(hwndMag, out rect);
			Bitmap bmp = new Bitmap(rect.Width, rect.Height);
			Graphics memoryGraphics = Graphics.FromImage(bmp);
			IntPtr hdc = memoryGraphics.GetHdc();
			Trace.WriteLine("213423234234234234");
			Trace.WriteLine(PrintWindow(hwndMag, hdc, 2));
			Trace.WriteLine("2323232323321111111111111111111111");
			

			// 關閉宿主窗體
			if (!Magnification.MagUninitialize()) return;
			DestroyWindow(mainWin);
		}

		#endregion

		#region 窗口截图（復刻Powerpoint）

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
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
		[DllImport("user32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);
		[DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern IntPtr GetShellWindow();
		[DllImport("dwmapi.dll")]
		static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out bool pvAttribute, int cbAttribute);

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
					result.length = Marshal.SizeOf( result );
					return result;
				}
			}
		}

		public delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);

		public WindowInformation[] GetAllWindows(HWND[] excludedHwnds) {
			var windows = new List<WindowInformation>();
			IntPtr hShellWnd  = GetShellWindow();
			IntPtr hDefView   = FindWindowEx(hShellWnd,   IntPtr.Zero, "SHELLDLL_DefView", null);
			IntPtr folderView = FindWindowEx(hDefView,    IntPtr.Zero, "SysListView32",    null);
			IntPtr taskBar    = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd",    null);
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