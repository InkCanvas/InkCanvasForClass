using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ink_Canvas.Popups {
    
    public partial class ShapeDrawingPopup : UserControl {
        public ShapeDrawingPopup() {
            InitializeComponent();
            ShapeDrawingItemsControl.ItemsSource = _items;
            _items.Add(new ShapeDrawingItem() {
                Name = "直线",
                Image = FindResource("LineIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "虚线",
                Image = FindResource("DashedLineIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "点虚线",
                Image = FindResource("DottedLineIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "箭头",
                Image = FindResource("ArrowLineIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "双箭头",
                Image = FindResource("ArrowLineTwoSideIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "矩形",
                Image = FindResource("RectIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "椭圆",
                Image = FindResource("CircleIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "饼图形",
                Image = FindResource("PieIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "三角形",
                Image = FindResource("TriangleIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "直角三角",
                Image = FindResource("RightTriangleIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "菱形",
                Image = FindResource("DiamondIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "平行四边形",
                Image = FindResource("ParallelogramIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "四线三格",
                Image = FindResource("FourLineIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "五线谱",
                Image = FindResource("FiveLineIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "平面坐标轴",
                Image = FindResource("DefaultAxisIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "平面坐标轴2",
                Image = FindResource("Axis2Icon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "平面坐标轴3",
                Image = FindResource("Axis3Icon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "平面坐标轴4",
                Image = FindResource("Axis4Icon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "三维坐标轴",
                Image = FindResource("ThreeDimensionAxisIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "双曲线",
                Image = FindResource("HyperbolaIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "带焦点的双曲线",
                Image = FindResource("HyperbolaWithFocalPointIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "抛物线1",
                Image = FindResource("ParabolaIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "抛物线2",
                Image = FindResource("Parabola2Icon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "带焦点的抛物线2",
                Image = FindResource("Parabola2WithFocalPointIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "圆柱体",
                Image = FindResource("CylinderIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "立方体",
                Image = FindResource("CubeIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "圆锥体",
                Image = FindResource("ConeIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "带圆心的圆形",
                Image = FindResource("CircleWithCenterPointIcon") as DrawingImage,
            });
            _items.Add(new ShapeDrawingItem() {
                Name = "带中心点的矩形",
                Image = FindResource("RectWithCenterPointIcon") as DrawingImage,
            });
        }

        private class ShapeDrawingItem {
            public string Name { get; set; }
            public DrawingImage Image { get; set; }
        }
        private ObservableCollection<ShapeDrawingItem> _items = new ObservableCollection<ShapeDrawingItem>();
        private bool isCloseButtonDown = false;
        public event EventHandler<RoutedEventArgs> ShapeDrawingPopupShouldCloseEvent;
        private void CloseButtonBorder_MouseDown(object sender, MouseButtonEventArgs e) {
            isCloseButtonDown = true;
            CloseButtonBorder.Background = new SolidColorBrush(Color.FromArgb(34, 220, 38, 38));
        }
        private void CloseButtonBorder_MouseLeave(object sender, MouseEventArgs e) {
            isCloseButtonDown = false;
            CloseButtonBorder.Background = new SolidColorBrush(Colors.Transparent);
        }
        private void CloseButtonBorder_MouseUp(object sender, MouseButtonEventArgs e) {
            if (!isCloseButtonDown) return;

            CloseButtonBorder_MouseLeave(null, null);
            ShapeDrawingPopupShouldCloseEvent?.Invoke(this,new RoutedEventArgs());
        }
        private Border shapeDrawingButtonDownBorder = null;
        private void ShapeDrawingButtonBorder_MouseDown(object sender, MouseButtonEventArgs e) {
            if (shapeDrawingButtonDownBorder != null) return;
            shapeDrawingButtonDownBorder = (Border)sender;
            shapeDrawingButtonDownBorder.Background = new SolidColorBrush(Color.FromRgb(228, 228, 231));
        }
        private void ShapeDrawingButtonBorder_MouseLeave(object sender, MouseEventArgs e) {
            if (shapeDrawingButtonDownBorder == null || shapeDrawingButtonDownBorder != sender) return;
            shapeDrawingButtonDownBorder.Background = new SolidColorBrush(Colors.Transparent);
            shapeDrawingButtonDownBorder = null;
        }
        private void ShapeDrawingButtonBorder_MouseUp(object sender, MouseButtonEventArgs e) {
            if (shapeDrawingButtonDownBorder == null || shapeDrawingButtonDownBorder != sender) return;
            ShapeDrawingButtonBorder_MouseLeave(sender, null);
            
        }
    }
}
