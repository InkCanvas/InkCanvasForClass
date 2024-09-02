using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ink_Canvas.Popups;
using iNKORE.UI.WPF.Helpers;
using iNKORE.UI.WPF.Modern.Controls;
using OSVersionExtension;

namespace Ink_Canvas.Windows {
    public partial class SettingsWindow : Window {

        public SettingsWindow() {
            InitializeComponent();

            // 初始化侧边栏项目
            SidebarItemsControl.ItemsSource = SidebarItems;
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "启动时行为",
                Name = "StartupItem",
                IconSource = FindResource("StartupIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "画板和墨迹",
                Name = "CanvasAndInkItem",
                IconSource = FindResource("CanvasAndInkIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "手势操作",
                Name = "GesturesItem",
                IconSource = FindResource("GesturesIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Separator
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "个性化和外观",
                Name = "AppearanceItem",
                IconSource = FindResource("AppearanceIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "墨迹转形状",
                Name = "InkRecognitionItem",
                IconSource = FindResource("InkRecognitionIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "几何与形状绘制",
                Name = "ShapeDrawingItem",
                IconSource = FindResource("ShapeDrawingIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "自动化行为",
                Name = "AutomationItem",
                IconSource = FindResource("AutomationIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Separator
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "PowerPoint 支持",
                Name = "PowerPointItem",
                IconSource = FindResource("PowerPointIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "插件和脚本",
                Name = "ExtensionsItem",
                IconSource = FindResource("ExtensionsIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Separator
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "存储空间",
                Name = "StorageItem",
                IconSource = FindResource("StorageIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "截图和屏幕捕捉",
                Name = "SnapshotItem",
                IconSource = FindResource("SnapshotIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "点名器设置",
                Name = "LuckyRandomItem",
                IconSource = FindResource("LuckyRandomIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "高级选项",
                Name = "AdvancedItem",
                IconSource = FindResource("AdvancedIcon") as DrawingImage,
                Selected = false,
            });
            SidebarItems.Add(new SidebarItem() {
                Type = SidebarItemType.Item,
                Title = "关于 InkCanvasForClass",
                Name = "AboutItem",
                IconSource = FindResource("AboutIcon") as DrawingImage,
                Selected = false,
            });
            _selectedSidebarItemName = "AboutItem";
            UpdateSidebarItemsSelection();

            SettingsPanes = new Grid[] {
                AboutPane,
                ExtensionsPane,
                CanvasAndInkPane,
                GesturesPane,
                StartupPane,
                AppearancePane,
                InkRecognitionPane,
                AutomationPane,
                PowerPointPane
            };

            SettingsPaneScrollViewers = new ScrollViewer[] {
                AboutScrollViewerEx,
                CanvasAndInkScrollViewerEx,
                GesturesScrollViewerEx,
                StartupScrollViewerEx,
                AppearanceScrollViewerEx,
                InkRecognitionScrollViewerEx,
                AutomationScrollViewerEx,
                PowerPointScrollViewerEx
            };

            // 关于页面图片横幅
            if (File.Exists(App.RootPath + "icc-about-illustrations.png")) {
                try {
                    CopyrightBannerImage.Visibility = Visibility.Visible;
                    CopyrightBannerImage.Source =
                        new BitmapImage(new Uri($"file://{App.RootPath + "icc-about-illustrations.png"}"));
                }
                catch { }
            } else {
                CopyrightBannerImage.Visibility = Visibility.Collapsed;
            }

            // 关于页面构建时间
            var buildTime = FileBuildTimeHelper.GetBuildDateTime(System.Reflection.Assembly.GetExecutingAssembly());
            if (buildTime != null) {
                var bt = ((DateTimeOffset)buildTime).LocalDateTime;
                var m = bt.Month.ToString().PadLeft(2, '0');
                var d = bt.Day.ToString().PadLeft(2, '0');
                var h = bt.Hour.ToString().PadLeft(2, '0');
                var min = bt.Minute.ToString().PadLeft(2, '0');
                var s = bt.Second.ToString().PadLeft(2, '0');
                AboutBuildTime.Text =
                    $"build-{bt.Year}-{m}-{d}-{h}:{min}:{s}";
            }

            // 关于页面系统版本
            AboutSystemVersion.Text = $"{OSVersion.GetOperatingSystem()} {OSVersion.GetOSVersion().Version}";

            // 关于页面触摸设备
            var _t_touch = new Thread(() => {
                var touchcount = TouchTabletDetectHelper.GetTouchTabletDevices().Count;
                var support = TouchTabletDetectHelper.IsTouchEnabled();
                Dispatcher.BeginInvoke(() =>
                    AboutTouchTabletText.Text = $"{touchcount}个设备，{(support ? "支持触摸设备" : "无触摸支持")}");
            });
            _t_touch.Start();
        }

        public Grid[] SettingsPanes = new Grid[] { };
        public ScrollViewer[] SettingsPaneScrollViewers = new ScrollViewer[] { };

        public enum SidebarItemType {
            Item,
            Separator
        }

        public class SidebarItem {
            public SidebarItemType Type { get; set; }
            public string Title { get; set; }
            public string Name { get; set; }
            public ImageSource IconSource { get; set; }
            public bool Selected { get; set; }
            public Visibility _spVisibility {
                get => this.Type == SidebarItemType.Separator ? Visibility.Visible : Visibility.Collapsed;
            }
            public Visibility _siVisibility {
                get => this.Type == SidebarItemType.Item ? Visibility.Visible : Visibility.Collapsed;
            }

            public SolidColorBrush _siBackground {
                get => this.Selected
                    ? new SolidColorBrush(Color.FromRgb(217, 217, 217))
                    : new SolidColorBrush(Colors.Transparent);
            }
        }

        public string _selectedSidebarItemName = "";
        public ObservableCollection<SidebarItem> SidebarItems = new ObservableCollection<SidebarItem>();

        public void UpdateSidebarItemsSelection() {
            foreach (var si in SidebarItems) {
                si.Selected = si.Name == _selectedSidebarItemName;
                if (si.Selected) SettingsWindowTitle.Text = si.Title;
            }
            CollectionViewSource.GetDefaultView(SidebarItems).Refresh();

            AboutPane.Visibility = _selectedSidebarItemName == "AboutItem" ? Visibility.Visible : Visibility.Collapsed;
            ExtensionsPane.Visibility = _selectedSidebarItemName == "ExtensionsItem" ? Visibility.Visible : Visibility.Collapsed;
            CanvasAndInkPane.Visibility = _selectedSidebarItemName == "CanvasAndInkItem" ? Visibility.Visible : Visibility.Collapsed;
            GesturesPane.Visibility = _selectedSidebarItemName == "GesturesItem" ? Visibility.Visible : Visibility.Collapsed;
            StartupPane.Visibility = _selectedSidebarItemName == "StartupItem" ? Visibility.Visible : Visibility.Collapsed;
            AppearancePane.Visibility = _selectedSidebarItemName == "AppearanceItem" ? Visibility.Visible : Visibility.Collapsed;
            InkRecognitionPane.Visibility = _selectedSidebarItemName == "InkRecognitionItem" ? Visibility.Visible : Visibility.Collapsed;
            AutomationPane.Visibility = _selectedSidebarItemName == "AutomationItem" ? Visibility.Visible : Visibility.Collapsed;
            PowerPointPane.Visibility = _selectedSidebarItemName == "PowerPointItem" ? Visibility.Visible : Visibility.Collapsed;
            foreach (var sv in SettingsPaneScrollViewers) {
                sv.ScrollToTop();
            }
        }

        public static class TouchTabletDetectHelper {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern int GetSystemMetrics(int nIndex);

            public static bool IsTouchEnabled()
            {
                const int MAXTOUCHES_INDEX = 95;
                int maxTouches = GetSystemMetrics(MAXTOUCHES_INDEX);

                return maxTouches > 0;
            }

            public class USBDeviceInfo
            {
                public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
                {
                    this.DeviceID = deviceID;
                    this.PnpDeviceID = pnpDeviceID;
                    this.Description = description;
                }
                public string DeviceID { get; private set; }
                public string PnpDeviceID { get; private set; }
                public string Description { get; private set; }
            }

            public static List<USBDeviceInfo> GetTouchTabletDevices()
            {
                List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

                ManagementObjectCollection collection;
                using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity"))
                    collection = searcher.Get();      

                foreach (var device in collection) {
                    var name = new StringBuilder((string)device.GetPropertyValue("Name")).ToString();
                    if (!name.Contains("Pentablet")) continue;
                    devices.Add(new USBDeviceInfo(
                        (string)device.GetPropertyValue("DeviceID"),
                        (string)device.GetPropertyValue("PNPDeviceID"),
                        (string)device.GetPropertyValue("Description")
                    ));
                }

                collection.Dispose();
                return devices;
            }
        }

        public static class FileBuildTimeHelper {
            public struct _IMAGE_FILE_HEADER
            {
                public ushort Machine;
                public ushort NumberOfSections;
                public uint TimeDateStamp;
                public uint PointerToSymbolTable;
                public uint NumberOfSymbols;
                public ushort SizeOfOptionalHeader;
                public ushort Characteristics;
            };

            public static DateTimeOffset? GetBuildDateTime(Assembly assembly)
            {
                var path = assembly.Location;
                if (File.Exists(path))
                {
                    var buffer = new byte[Math.Max(Marshal.SizeOf(typeof(_IMAGE_FILE_HEADER)), 4)];
                    using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Position = 0x3C;
                        fileStream.Read(buffer, 0, 4);
                        fileStream.Position = BitConverter.ToUInt32(buffer, 0); // COFF header offset
                        fileStream.Read(buffer, 0, 4); // "PE\0\0"
                        fileStream.Read(buffer, 0, buffer.Length);
                    }
                    var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    try
                    {
                        var coffHeader = (_IMAGE_FILE_HEADER)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(_IMAGE_FILE_HEADER));
                        return DateTimeOffset.FromUnixTimeSeconds(coffHeader.TimeDateStamp);
                    }
                    finally
                    {
                        pinnedBuffer.Free();
                    }
                }
                else 
                {
                    return null;
                }
            }
        }

        private void ScrollViewerEx_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.VerticalOffset >= 10) {
                DropShadowEffectTopBar.Opacity = 0.25;
            } else {
                DropShadowEffectTopBar.Opacity = 0;
            }
        }

        private void ScrollBar_Scroll(object sender, RoutedEventArgs e) {
            var scrollbar = (ScrollBar)sender;
            var scrollviewer = scrollbar.FindAscendant<ScrollViewer>();
            if (scrollviewer != null) scrollviewer.ScrollToVerticalOffset(scrollbar.Track.Value);
        }

        private void ScrollBarTrack_MouseEnter(object sender, MouseEventArgs e) {
            var border = (Border)sender;
            if (border.Child is Track track) {
                track.Width = 16;
                track.Margin = new Thickness(0, 0, -2, 0);
                var scrollbar = track.FindAscendant<ScrollBar>();
                if (scrollbar != null) scrollbar.Width = 16;
                var grid = track.FindAscendant<Grid>();
                if (grid.FindDescendantByName("ScrollBarBorderTrackBackground") is Border backgroundBorder) {
                    backgroundBorder.Width = 8;
                    backgroundBorder.CornerRadius = new CornerRadius(4);
                    backgroundBorder.Opacity = 1;
                }
                var thumb = track.Thumb.Template.FindName("ScrollbarThumbEx", track.Thumb) ;
                if (thumb != null) {
                    var _thumb = thumb as Border;
                    _thumb.CornerRadius = new CornerRadius(4);
                    _thumb.Width = 8;
                    _thumb.Margin = new Thickness(-0.75, 0, 1, 0);
                    _thumb.Background = new SolidColorBrush(Color.FromRgb(138, 138, 138));
                }
            }
        }

        private void ScrollBarTrack_MouseLeave(object sender, MouseEventArgs e) {
            var border = (Border)sender;
            border.Background = new SolidColorBrush(Colors.Transparent);
            border.CornerRadius = new CornerRadius(0);
            if (border.Child is Track track) {
                track.Width = 6;
                track.Margin = new Thickness(0, 0, 0, 0);
                var scrollbar = track.FindAscendant<ScrollBar>();
                if (scrollbar != null) scrollbar.Width = 6;
                var grid = track.FindAscendant<Grid>();
                if (grid.FindDescendantByName("ScrollBarBorderTrackBackground") is Border backgroundBorder) {
                    backgroundBorder.Width = 3;
                    backgroundBorder.CornerRadius = new CornerRadius(1.5);
                    backgroundBorder.Opacity = 0;
                }
                var thumb = track.Thumb.Template.FindName("ScrollbarThumbEx", track.Thumb) ;
                if (thumb != null) {
                    var _thumb = thumb as Border;
                    _thumb.CornerRadius = new CornerRadius(1.5);
                    _thumb.Width = 3;
                    _thumb.Margin = new Thickness(0);
                    _thumb.Background = new SolidColorBrush(Color.FromRgb(195, 195, 195));
                }
            }
        }

        private void ScrollbarThumb_MouseDown(object sender, MouseButtonEventArgs e) {
            var thumb = (Thumb)sender;
            var border = thumb.Template.FindName("ScrollbarThumbEx",thumb);
            ((Border)border).Background = new SolidColorBrush(Color.FromRgb(95, 95, 95));
        }

        private void ScrollbarThumb_MouseUp(object sender, MouseButtonEventArgs e) {
            var thumb = (Thumb)sender;
            var border = thumb.Template.FindName("ScrollbarThumbEx",thumb);
            ((Border)border).Background = new SolidColorBrush(Color.FromRgb(138, 138, 138));
        }

        private Border _sidebarItemMouseDownBorder = null;

        private void SidebarItem_MouseDown(object sender, MouseButtonEventArgs e) {
            if (_sidebarItemMouseDownBorder != null || _sidebarItemMouseDownBorder == sender) return;
            _sidebarItemMouseDownBorder = (Border)sender;
            var bd = sender as Border;
            if (bd.FindDescendantByName("MouseFeedbackBorder") is Border feedbackBd) feedbackBd.Opacity = 0.12;
        }

        private void SidebarItem_MouseUp(object sender, MouseButtonEventArgs e) {
            if (_sidebarItemMouseDownBorder == null || _sidebarItemMouseDownBorder != sender) return;
            if (_sidebarItemMouseDownBorder.Tag is SidebarItem data) _selectedSidebarItemName = data.Name;
            SidebarItem_MouseLeave(sender, null);
            UpdateSidebarItemsSelection();
        }

        private void SidebarItem_MouseLeave(object sender, MouseEventArgs e) {
            if (_sidebarItemMouseDownBorder == null || _sidebarItemMouseDownBorder != sender) return;
            if (_sidebarItemMouseDownBorder.FindDescendantByName("MouseFeedbackBorder") is Border feedbackBd) feedbackBd.Opacity = 0;
            _sidebarItemMouseDownBorder = null;
        }
    }
}
