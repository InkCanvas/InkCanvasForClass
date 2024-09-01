using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ink_Canvas
{

    public enum FloatingBarItemType {
        Button,
        StateButton,
        Separator
    }

    public class FloatingBarItem {
        public string Name { get; set; }
        public ImageSource IconSource { get; set; }
        public bool Selected { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public string IconSourceResourceKey { get; set; }
        public FloatingBarItemType Type { get; set; }
        public MainWindow.ICCToolsEnum ToolType { get; set; }
        public SolidColorBrush _backgroundBrush {
            get {
                if (Selected) return new SolidColorBrush(Color.FromRgb(37, 99, 235));
                return new SolidColorBrush(Colors.Transparent);
            }
        }
        public Visibility _itemVisibility {
            get {
                if (!IsVisible) return Visibility.Collapsed;
                return Type != FloatingBarItemType.Separator ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Visibility _separatorVisibility {
            get {
                if (!IsVisible) return Visibility.Collapsed;
                return Type != FloatingBarItemType.Separator ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }

    /// <summary>
    /// FloatingToolBarV2.xaml 的交互逻辑
    /// </summary>
    public partial class FloatingToolBarV2 : Window {

        public FloatingToolBarV2() {
            InitializeComponent();

            ToolBarItemsControl.ItemsSource = ToolbarItems;
            //var clonedDrawing = (FindResource("CursorIcon") as DrawingImage).Clone();
            //((clonedDrawing.Drawing as DrawingGroup).Children[0] as
            //    GeometryDrawing).Brush = new SolidColorBrush(Colors.Black);
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "Cursor",
                IconSource = FindResource("CursorIcon") as DrawingImage,
                IconSourceResourceKey = "CursorIcon",
                IsVisible = true,
                Selected = false,
                ToolType = MainWindow.ICCToolsEnum.CursorMode,
                Type = FloatingBarItemType.StateButton,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "Pen",
                IconSource = FindResource("PenIcon") as DrawingImage,
                IconSourceResourceKey = "PenIcon",
                IsVisible = true,
                Selected = false,
                ToolType = MainWindow.ICCToolsEnum.PenMode,
                Type = FloatingBarItemType.StateButton,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "Clear",
                IconSource = FindResource("TrashBinIcon") as DrawingImage,
                IconSourceResourceKey = "TrashBinIcon",
                IsVisible = true,
                Selected = false,
                Type = FloatingBarItemType.Button,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "SeparatorA",
                IsVisible = true,
                Type = FloatingBarItemType.Separator,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "Eraser",
                IconSource = FindResource("EraserIcon") as DrawingImage,
                IconSourceResourceKey = "EraserIcon",
                IsVisible = true,
                Selected = false,
                ToolType = MainWindow.ICCToolsEnum.EraseByGeometryMode,
                Type = FloatingBarItemType.StateButton,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "ShapeDrawing",
                IconSource = FindResource("ShapesIcon") as DrawingImage,
                IconSourceResourceKey = "ShapesIcon",
                IsVisible = true,
                Selected = false,
                Type = FloatingBarItemType.Button,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "Select",
                IconSource = FindResource("SelectIcon") as DrawingImage,
                IconSourceResourceKey = "SelectIcon",
                IsVisible = true,
                Selected = false,
                ToolType = MainWindow.ICCToolsEnum.LassoMode,
                Type = FloatingBarItemType.StateButton,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "SeparatorB",
                IsVisible = true,
                Type = FloatingBarItemType.Separator,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "Undo",
                IconSource = FindResource("UndoIcon") as DrawingImage,
                IconSourceResourceKey = "UndoIcon",
                IsVisible = true,
                Selected = false,
                Type = FloatingBarItemType.Button,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "Redo",
                IconSource = FindResource("RedoIcon") as DrawingImage,
                IconSourceResourceKey = "RedoIcon",
                IsVisible = true,
                Selected = false,
                Type = FloatingBarItemType.Button,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "SeparatorC",
                IsVisible = true,
                Type = FloatingBarItemType.Separator,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "Gesture",
                IconSource = FindResource("GestureIcon") as DrawingImage,
                IconSourceResourceKey = "GestureIcon",
                IsVisible = true,
                Selected = false,
                Type = FloatingBarItemType.Button,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "Menu",
                IconSource = FindResource("MoreIcon") as DrawingImage,
                IconSourceResourceKey = "MoreIcon",
                IsVisible = true,
                Selected = false,
                Type = FloatingBarItemType.Button,
            });
            ToolbarItems.Add(new FloatingBarItem() {
                Name = "Fold",
                IconSource = FindResource("FoldIcon") as DrawingImage,
                IconSourceResourceKey = "FoldIcon",
                IsVisible = true,
                Selected = false,
                Type = FloatingBarItemType.Button,
            });

            ReMeasureToolBar();
            UpdateToolBarSelectedTool(MainWindow.ICCToolsEnum.CursorMode);
            UpdateToolBarVariant(1);

            double widthInDevicePixels = GetSystemMetrics(0); // SM_CXSCREEN
            double widthInDIP = SystemParameters.WorkArea.Right; // Device independent pixels.
            double scalingFactor = widthInDIP/widthInDevicePixels;
            ScalingFactor = scalingFactor;

        }

        private double ScalingFactor;

        public ObservableCollection<FloatingBarItem> ToolbarItems { get; set; } =
            new ObservableCollection<FloatingBarItem>();

        private Grid _mouseDownButton = null;
        private bool _isMouseDownHeadIcon = false;

        private double CaculateToolBarWindowWidth(Collection<FloatingBarItem> toolbarItems) {
            // ------------------------------------------------------//
            var containerMargin = 4D;       // 内容StackPanel的Margin
            var itemWidth = 42D;            // 按钮宽度
            var itemsSpacing = 2D;          // 按钮之间的间距
            var headIconSpacingRight = 4D;  // icc图标的右间距
            var separatorWidth = 1D;        // 分隔符宽度
            var separatorSpacing = 3D;      // 分隔符左右的间距
            // ------------------------------------------------------//

            var _width = containerMargin * 2 + itemWidth + headIconSpacingRight - itemsSpacing;
            foreach (var i in toolbarItems) {
                if (!i.IsVisible) continue;
                if (i.Type != FloatingBarItemType.Separator) _width += itemsSpacing + itemWidth; 
                    else _width += separatorSpacing + separatorWidth + separatorSpacing - itemsSpacing;
            }

            return _width;
        }

        /// <summary>
        /// 修改工具栏的变体
        /// </summary>
        /// <param name="variant">0为全长，1为鼠标模式下变体，2为迷你，3为仅小图标</param>
        private void UpdateToolBarVariant(int variant) {
            IEnumerable<FloatingBarItem> items;
            if (variant == 0) items = ToolbarItems.AsEnumerable();
            else if (variant == 1)
                items = ToolbarItems.Where((item, i) => (new string[] {
                    "Cursor", "Pen", "Clear", "SeparatorC", "Gesture", "Menu", "Fold"
                }).Contains(item.Name));
            else if (variant == 2)
                items = ToolbarItems.Where((item, i) => (new string[] {
                    "Cursor", "Pen", "Clear"
                }).Contains(item.Name));
            else throw new NotImplementedException();

            foreach (var fi in ToolbarItems) {
                fi.IsVisible = items.Contains(fi);
            }

            CollectionViewSource.GetDefaultView(ToolbarItems).Refresh();
            ReMeasureToolBar();
        }

        private void UpdateToolBarSelectedTool(MainWindow.ICCToolsEnum type) {
            var _i = ToolbarItems.Where((item, i) =>
                item.Type == FloatingBarItemType.StateButton &&
                item.ToolType == type).Single();
            var nowSelected = ToolbarItems.Where((item, i) =>
                item.Selected);
            if (nowSelected.Any() && nowSelected.Count()==1 && _i == nowSelected.Single()) return;

            foreach (var fi in ToolbarItems) {
                if (fi.Type == FloatingBarItemType.Separator) continue;
                fi.Selected = false;
                fi.IconSource = FindResource(fi.IconSourceResourceKey) as ImageSource;
            }
            
            _i.Selected = true;
            var clonedIcon = _i.IconSource.Clone() as DrawingImage;
            foreach (var d in (clonedIcon.Drawing as DrawingGroup).Children) 
                ((GeometryDrawing)d).Brush = new SolidColorBrush(Colors.White);
            _i.IconSource = clonedIcon;
            
            CollectionViewSource.GetDefaultView(ToolbarItems).Refresh();
        }

        private void ReMeasureToolBar() {
            var barWidth = CaculateToolBarWindowWidth(ToolbarItems);
            ToolBarV2Grid.Width = barWidth;
            Width = barWidth + 6 * 2; // 6是工具栏和窗口的Margin
            Height = 48 + 6 * 2; // 48是工具栏高度
        }

        private void OnToolSelectionChanged(FloatingBarItem sender) {
            if (sender.Selected && sender.ToolType != MainWindow.ICCToolsEnum.CursorMode)
                UpdateToolBarVariant(0);
            else UpdateToolBarVariant(1);
        }

        private void ToolbarButton_MouseDown(object sender, MouseButtonEventArgs e) {
            if (_mouseDownButton != null) return;
            var gd = sender as Grid;
            var itemData = gd.Tag as FloatingBarItem;
            _mouseDownButton = gd;
            if (!itemData.Selected) {
                var bgImg = gd.Children[1] as Image;
                bgImg.Opacity = 1;
            }
        }

        private void ToolbarButton_MouseUp(object sender, MouseButtonEventArgs e) {
            if (_mouseDownButton == null || _mouseDownButton != sender) return;
            ToolbarButton_MouseLeave(sender, null);

            var gd = sender as Grid;
            var itemData = gd.Tag as FloatingBarItem;
            if (itemData.Type == FloatingBarItemType.StateButton) {
                UpdateToolBarSelectedTool(itemData.ToolType);
                OnToolSelectionChanged(itemData);
            }
        }

        [DllImport("USER32.DLL", SetLastError = true)]
        public static extern int GetSystemMetrics(int nIndex);

        private void ToolbarButton_MouseLeave(object sender, MouseEventArgs e) {
            if (_mouseDownButton == null || _mouseDownButton != sender) return;
            var itemData = _mouseDownButton.Tag as FloatingBarItem;
            if (!itemData.Selected) {
                var bgImg = _mouseDownButton.Children[1] as Image;
                bgImg.Opacity = 0;
            }
            _mouseDownButton = null;
        }

        public Point prevPoint = new Point(0,0);
        public Point lastPoint = new Point(0, 0);
        public bool isInMovingMode = false;
        public double winLeft = 0;
        public double winTop = 0;

        private void HeadIconButton_MouseDown(object sender, MouseButtonEventArgs e) {
            if (_isMouseDownHeadIcon) return;
            _isMouseDownHeadIcon = true;
            var gd = sender as Grid;
            prevPoint = e.GetPosition(ToolBarV2Grid);
            winLeft = Left;
            winTop = Top;
            Trace.WriteLine(prevPoint);
            gd.CaptureMouse();
            gd.RenderTransform = new ScaleTransform(0.95, 0.95);
            isInMovingMode = false;
        }

        private void HeadIconButton_MouseMove(object sender, MouseEventArgs e) {
            if (_isMouseDownHeadIcon == false) return;
            var nowPt = e.GetPosition(ToolBarV2Grid);
            lastPoint = nowPt;
            var deltaX = nowPt.X - prevPoint.X;
            var deltaY = nowPt.Y - prevPoint.Y;
            if (Math.Abs(deltaY) > 16 || Math.Abs(deltaX) > 16) isInMovingMode = true;
            if (isInMovingMode) {
                ToolbarV2.RenderTransform = null;
                HeadIconImage.RenderTransform = null;
                var mp = System.Windows.Forms.Control.MousePosition;
                Top = mp.Y * ScalingFactor - prevPoint.Y - 6;
                Left = mp.X * ScalingFactor - prevPoint.X - 6;
            } else {
                ToolbarV2.RenderTransform = new TranslateTransform(deltaX / 3, deltaY / 3);
                HeadIconImage.RenderTransform = new TranslateTransform(deltaX / 8, deltaY / 8);
            }
        }

        private void HeadIconButton_MouseUp(object sender, MouseButtonEventArgs e) {
            if (_isMouseDownHeadIcon == false) return;
            _isMouseDownHeadIcon = false;
            var gd = sender as Grid;
            gd.ReleaseMouseCapture();
            gd.RenderTransform = null;
            ToolbarV2.RenderTransform = null;
            HeadIconImage.RenderTransform = null;
            isInMovingMode = false;
        }
    }
}
