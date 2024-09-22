using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using InkCanvasForClass.IccInkCanvas.Settings;

namespace InkCanvasForClass.IccInkCanvas {
    /// <summary>
    /// IccBoard.xaml 的交互逻辑
    /// </summary>
    public partial class IccBoard : UserControl {

        public BoardSettings BoardSettings { get; private set; } = new BoardSettings();

        #region Properties

        private bool _isEditingModePropertyAccessdByCodeBehind = false;
        public EditingMode EditingMode {
            get => (EditingMode)GetValue(EditingModeProperty);
            set {
                if (value == EditingMode.ShapeDrawing) throw new Exception("EditingMode.ShapeDrawing 不能被用户手动设定");
                _isEditingModePropertyAccessdByCodeBehind = true;
                SetValue(EditingModeProperty, value);
                _isEditingModePropertyAccessdByCodeBehind = false;
                UpdateEditingMode();
            }
        }
        public static readonly System.Windows.DependencyProperty EditingModeProperty =
            System.Windows.DependencyProperty.Register(
                nameof(EditingMode),
                typeof(EditingMode),
                typeof(IccBoard),
                new FrameworkPropertyMetadata(EditingMode.Writing,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    propertyChangedCallback: OnEditingModePropertyChanged));
        private static void OnEditingModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var iccboard = d as IccBoard;
            if (iccboard != null && iccboard._isEditingModePropertyAccessdByCodeBehind) return;
            iccboard?.UpdateEditingMode();
        }

        #endregion

        #region EditingMode

        private void UpdateEditingMode() {
            if (EditingMode == EditingMode.None || EditingMode == EditingMode.NoneWithHitTest) 
                InkCanvas.EditingMode = InkCanvasEditingMode.None;
            IsHitTestVisible = EditingMode != EditingMode.None;
            if (EditingMode == EditingMode.Writing) InkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        #endregion

        #region Events

        private static readonly System.Windows.RoutedEvent EditingModeChangedEvent = EventManager.RegisterRoutedEvent(
            name: "EditingModeChanged",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(System.Windows.RoutedEventHandler),
            ownerType: typeof(IccBoard));
        private static readonly System.Windows.RoutedEvent ActiveEditingModeChangedEvent = EventManager.RegisterRoutedEvent(
            name: "ActiveEditingModeChanged",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(System.Windows.RoutedEventHandler),
            ownerType: typeof(IccBoard));

        public event System.Windows.RoutedEventHandler EditingModeChanged {
            add => AddHandler(EditingModeChangedEvent, value);
            remove => RemoveHandler(EditingModeChangedEvent, value);
        }
        public event System.Windows.RoutedEventHandler ActiveEditingModeChanged {
            add => AddHandler(ActiveEditingModeChangedEvent, value);
            remove => RemoveHandler(ActiveEditingModeChangedEvent, value);
        }

        private void RaiseEditingModeChangedEvent() {
            RoutedEventArgs routedEventArgs = new RoutedEventArgs(routedEvent: EditingModeChangedEvent);
            RaiseEvent(routedEventArgs);
        }
        private void RaiseActiveEditingModeChangedEvent() {
            RoutedEventArgs routedEventArgs = new RoutedEventArgs(routedEvent: ActiveEditingModeChangedEvent);
            RaiseEvent(routedEventArgs);
        }

        #endregion

        #region BoardSettings

        private void RegisterEventsForBoardSettings() {
            BoardSettings.NibWidthChanged += (s,e)=>InkCanvas.DefaultDrawingAttributes.Width = BoardSettings.NibWidth;
            BoardSettings.NibHeightChanged += (s,e)=>InkCanvas.DefaultDrawingAttributes.Height = BoardSettings.NibHeight;
            BoardSettings.NibColorChanged += (s,e)=>InkCanvas.DefaultDrawingAttributes.Color = BoardSettings.NibColor;
        }

        #endregion

        public IccBoard() {
            InitializeComponent();
        }

        private void IccInkCanvas_Loaded(object sender, RoutedEventArgs e) {
            var ic = (IccInkCanvas)sender;

            // 启动时自动修改 InkCanvas 的大小
            var screenW = SystemParameters.PrimaryScreenWidth;
            var screenH = SystemParameters.PrimaryScreenHeight;

            var fullWidth = screenW * 257;
            var fullHeight = screenH * 417;

            var left = 0 - screenW * 128;
            var top = 0 - screenH * 208;

            ic.Width = fullWidth;
            ic.Height = fullHeight;
            Canvas.SetLeft(ic, left);
            Canvas.SetTop(ic, top);

            ic.DefaultDrawingAttributes.Width = BoardSettings.NibWidth;
            ic.DefaultDrawingAttributes.Height = BoardSettings.NibHeight;
            ic.DefaultDrawingAttributes.Color = BoardSettings.NibColor;

            ic.BoardSettings = BoardSettings;

            // BoardSettings 事件注册
            RegisterEventsForBoardSettings();
        }

    }
}
