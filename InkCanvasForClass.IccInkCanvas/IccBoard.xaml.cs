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
using System.Windows.Threading;
using InkCanvasForClass.IccInkCanvas.Settings;
using InkCanvasForClass.IccInkCanvas.Utils.Threading;

namespace InkCanvasForClass.IccInkCanvas {

    public class IccInnerInkCanvasInfo {
        public Guid GUID { get; set; }
        public Dispatcher Dispatcher { get; set; }
        public DispatcherContainer Container { get; set; }
    }

    /// <summary>
    /// IccBoard.xaml 的交互逻辑
    /// </summary>
    public partial class IccBoard : UserControl {

        public BoardSettings BoardSettings { get; private set; } = new BoardSettings();

        private List<IccInnerInkCanvasInfo> DispatcherInkCanvasList { get; set; } =
            new List<IccInnerInkCanvasInfo>();

        private List<IccBoardPage> BoardPages { get; set; } = new List<IccBoardPage>();

        private int CurrentPageIndex;

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
                WrapperInkCanvas.EditingMode = InkCanvasEditingMode.None;
            IsHitTestVisible = EditingMode != EditingMode.None;
            if (EditingMode == EditingMode.Writing) WrapperInkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            EraserCanvas.Visibility = EditingMode == EditingMode.GeometryErasing ? Visibility.Visible : Visibility.Collapsed;
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
        private static readonly System.Windows.RoutedEvent CurrentPageChangedEvent = EventManager.RegisterRoutedEvent(
            name: "CurrentPageChanged",
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
        public event System.Windows.RoutedEventHandler CurrentPageChanged {
            add => AddHandler(CurrentPageChangedEvent, value);
            remove => RemoveHandler(CurrentPageChangedEvent, value);
        }

        private bool isIgnoreRaiseCurrentPageChangedEvent = false;

        private void RaiseEditingModeChangedEvent() {
            RoutedEventArgs routedEventArgs = new RoutedEventArgs(routedEvent: EditingModeChangedEvent);
            RaiseEvent(routedEventArgs);
        }
        private void RaiseActiveEditingModeChangedEvent() {
            RoutedEventArgs routedEventArgs = new RoutedEventArgs(routedEvent: ActiveEditingModeChangedEvent);
            RaiseEvent(routedEventArgs);
        }
        private void RaiseCurrentPageChangedEvent() {
            if (isIgnoreRaiseCurrentPageChangedEvent) return;
            RoutedEventArgs routedEventArgs = new RoutedEventArgs(routedEvent: CurrentPageChangedEvent);
            RaiseEvent(routedEventArgs);
        }

        #endregion

        #region BoardSettings

        private void RegisterEventsForBoardSettings() {
            BoardSettings.NibWidthChanged += (s,e)=>WrapperInkCanvas.DefaultDrawingAttributes.Width = BoardSettings.NibWidth;
            BoardSettings.NibHeightChanged += (s,e)=>WrapperInkCanvas.DefaultDrawingAttributes.Height = BoardSettings.NibHeight;
            BoardSettings.NibColorChanged += (s,e)=>WrapperInkCanvas.DefaultDrawingAttributes.Color = BoardSettings.NibColor;
        }

        #endregion

        #region InkCanvas Manager

        private async Task<IccInnerInkCanvasInfo> AddIccInkCanvas() {
            var screenW = SystemParameters.PrimaryScreenWidth;
            var screenH = SystemParameters.PrimaryScreenHeight;

            var fullWidth = screenW * 257;
            var fullHeight = screenH * 417;

            var left = 0 - screenW * 128;
            var top = 0 - screenH * 208;

            var di = new IccDispatcherInkCanvasInfo();
            await di.InitDispatcher();
            var control = await di.Dispatcher.InvokeAsync(() => new InkCanvas() {
                Background = new SolidColorBrush(Colors.White),
                Width = fullWidth,
                Height = fullHeight,
            });
            var dc = new DispatcherContainer() {
                Width = fullWidth,
                Height = fullHeight,
            };
            await dc.SetChildAsync(control);
            InkCanvasHostCanvas.Children.Add(dc);
            Canvas.SetTop(dc, top);
            Canvas.SetLeft(dc, left);
            var info = new IccInnerInkCanvasInfo() {
                GUID = di.GUID,
                Dispatcher = di.Dispatcher,
                Container = dc
            };
            DispatcherInkCanvasList.Add(info);
            return info;
        }

        private void RemoveIccInkCanvas(IccInnerInkCanvasInfo item) {
            item.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Render);
            InkCanvasHostCanvas.Children.Remove(item.Container);
            DispatcherInkCanvasList.Remove(item);
        }

        private void UpdateInnerInkCanvasVisibility(IccBoardPage item) {
            foreach (UIElement child in InkCanvasHostCanvas.Children) {
                child.Visibility = Visibility.Collapsed;
                if (child.Equals(item.Container)) child.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Multi-Pages

        /// <summary>
        /// 根据索引值获取页面
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IccBoardPage this[int index] => BoardPages[index];

        /// <summary>
        /// 根据InkCanvas的GUID获取页面
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        public IccBoardPage this[Guid GUID] => BoardPages.Find(page => page.GUID == GUID);

        /// <summary>
        /// 获取总页数
        /// </summary>
        public int PagesCount {
            get => BoardPages.Count;
        }

        /// <summary>
        /// 获取当前页面的索引值（如果需要展示页数的话，获取到的还需要加1）
        /// </summary>
        public int CurrentPage {
            get => CurrentPageIndex;
        }

        /// <summary>
        /// 获取到当前页面的 IccBoardPage 对象
        /// </summary>
        public IccBoardPage CurrentPageItem {
            get => BoardPages[CurrentPageIndex];
        }

        /// <summary>
        /// 添加新的白板页面
        /// </summary>
        /// <param name="boardPageAppendMode">选择页面插入的模式</param>
        /// <param name="jumpToPageWhenAdded">指定是否在插入页面后跳转到该页面</param>
        /// <returns></returns>
        public async Task<IccBoardPage> AddPage(BoardPageAppendMode? boardPageAppendMode = BoardPageAppendMode.AppendAfterItem, bool? jumpToPageWhenAdded = true) {
            var info = await AddIccInkCanvas();
            
            var page = new IccBoardPage() {
                Container = info.Container,
                GUID = info.GUID,
                Dispatcher = info.Dispatcher,
                InkCanvas = await info.Dispatcher.InvokeAsync(() => info.Container.Child) as InkCanvas
            };

            var appendMode = boardPageAppendMode ?? BoardPageAppendMode.AppendAfterItem;
            var insertP = 0;

            if (appendMode == BoardPageAppendMode.AppendAfterItem) {
                insertP = Math.Max(Math.Min(CurrentPageIndex + 1, BoardPages.Count),0);
            } else if (appendMode == BoardPageAppendMode.AppendBeforeItem) {
                insertP = Math.Max(Math.Min(CurrentPageIndex - 1, BoardPages.Count),0);
            } else if (appendMode == BoardPageAppendMode.AppendToListStart) {
                insertP = 0;
            } else if (appendMode == BoardPageAppendMode.AppendToListEnd) {
                insertP = BoardPages.Count;
            }
            BoardPages.Insert(insertP, page);

            isIgnoreRaiseCurrentPageChangedEvent = false;

            if (jumpToPageWhenAdded??true) {
                UpdateInnerInkCanvasVisibility(page);
                CurrentPageIndex = BoardPages.IndexOf(page);
                RaiseCurrentPageChangedEvent();
            }
            
            return page;
        }

        /// <summary>
        /// 切换到上一页
        /// </summary>
        /// <param name="step">跳转的页数，默认为1</param>
        public void GoToPreviousPage(int? step = 1) {
            var ci = CurrentPageIndex - step??1;
            if (ci < 0) ci = 0;

            var page = BoardPages[ci];
            UpdateInnerInkCanvasVisibility(page);

            if (!ci.Equals(CurrentPageIndex)) {
                CurrentPageIndex = ci;
                RaiseCurrentPageChangedEvent();
            }
        }

        /// <summary>
        /// 切换到下一页
        /// </summary>
        /// <param name="step">跳转的页数，默认为1</param>
        public void GoToNextPage(int? step = 1) {
            var ci = CurrentPageIndex + step??1;
            if (ci > BoardPages.Count - 1) ci = BoardPages.Count - 1;

            var page = BoardPages[ci];
            UpdateInnerInkCanvasVisibility(page);

            if (!ci.Equals(CurrentPageIndex)) {
                CurrentPageIndex = ci;
                RaiseCurrentPageChangedEvent();
            }
        }

        /// <summary>
        /// 跳转到指定索引的页面
        /// </summary>
        /// <param name="index">索引</param>
        public void GotoPage(int index) {
            UpdateInnerInkCanvasVisibility(BoardPages[index]);

            if (!index.Equals(CurrentPageIndex)) {
                CurrentPageIndex = index;
                RaiseCurrentPageChangedEvent();
            }
        }

        /// <summary>
        /// 跳转到指定页面
        /// </summary>
        /// <param name="page">页面</param>
        public void GotoPage(IccBoardPage page) {
            var index = BoardPages.IndexOf(page);
            UpdateInnerInkCanvasVisibility(page);

            if (!index.Equals(CurrentPageIndex)) {
                CurrentPageIndex = index;
                RaiseCurrentPageChangedEvent();
            }
        }

        /// <summary>
        /// 指示当前页面是否是最后一页，如果只有一页会返回true
        /// </summary>
        public bool IsCurrentLastPage => CurrentPageIndex == BoardPages.Count - 1;

        /// <summary>
        /// 指示当前页面是否是第一页，如果只有一页会返回true
        /// </summary>
        public bool IsCurrentFirstPage => CurrentPageIndex == 0;

        private async Task _RemovePage(IccBoardPage page, bool? isDisposeInstance = true, PageSwitchMode? pageSwitchMode = PageSwitchMode.SwitchToPreviousPage) {
            var isCurrent = CurrentPageItem == page;
            isIgnoreRaiseCurrentPageChangedEvent = true;
            if (isDisposeInstance??true) RemoveIccInkCanvas(DispatcherInkCanvasList.Find(info => info.GUID.Equals(page.GUID)));
            if (isCurrent) {
                var switchMode = pageSwitchMode ?? PageSwitchMode.SwitchToPreviousPage;
                if (IsCurrentFirstPage) GoToNextPage();
                else if (IsCurrentLastPage) GoToPreviousPage();
                else if (!IsCurrentFirstPage && !IsCurrentLastPage) {
                    if (switchMode == PageSwitchMode.SwitchToPreviousPage) GoToPreviousPage();
                    else if (switchMode == PageSwitchMode.SwitchToNextPage) GoToNextPage();
                }
            }
            BoardPages.Remove(page);
            isIgnoreRaiseCurrentPageChangedEvent = false;
            if (BoardPages.Count == 0) await AddPage(BoardPageAppendMode.AppendToListEnd);
            else RaiseCurrentPageChangedEvent();
        }

        /// <summary>
        /// 移除指定索引的页面，如果只有1页，执行该方法会删除该页面并新建一个页面。
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="isDisposeInstance">指定是否销毁该页面的Dispatcher，销毁后无法再使用该实例添加到Board！</param>
        /// <param name="pageSwitchMode">删除当前激活页面后切换到临近页面的行为，如果删除的页面不是当前页面则不会生效</param>
        public async Task RemovePageAt(int index, bool? isDisposeInstance = true, PageSwitchMode? pageSwitchMode = PageSwitchMode.SwitchToPreviousPage) {
            var page = BoardPages[index];
            await _RemovePage(page, isDisposeInstance, pageSwitchMode);
        }

        /// <summary>
        /// 移除当前页面，如果只有1页，执行该方法会删除当前页面并新建一个页面。
        /// </summary>
        /// <param name="isDisposeInstance">指定是否销毁该页面的Dispatcher，销毁后无法再使用该实例添加到Board！</param>
        /// <param name="pageSwitchMode">删除当前激活页面后切换到临近页面的行为，如果删除的页面不是当前页面则不会生效</param>
        public async Task RemovePage(bool? isDisposeInstance = true, PageSwitchMode? pageSwitchMode = PageSwitchMode.SwitchToPreviousPage) {
            var page = CurrentPageItem;
            await _RemovePage(page, isDisposeInstance, pageSwitchMode);
        }

        /// <summary>
        /// 移除指定的页面，如果只有1页，执行该方法会删除该页面并新建一个页面。
        /// </summary>
        /// <param name="page">页面</param>
        /// <param name="isDisposeInstance">指定是否销毁该页面的Dispatcher，销毁后无法再使用该实例添加到Board！</param>
        /// <param name="pageSwitchMode">删除当前激活页面后切换到临近页面的行为，如果删除的页面不是当前页面则不会生效</param>
        public async Task RemovePage(IccBoardPage page, bool? isDisposeInstance = true, PageSwitchMode? pageSwitchMode = PageSwitchMode.SwitchToPreviousPage) {
            await _RemovePage(page, isDisposeInstance, pageSwitchMode);
        }

        #endregion

        #region Dynamic Renderer

        private async void WrapperInkCanvas_Loaded(object sender, RoutedEventArgs e) {
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

            ic.StrokeCollected += IccWrapperInkCanvas_StrokeCollected;

            ic.BoardSettings = BoardSettings;

            // BoardSettings 事件注册
            RegisterEventsForBoardSettings();

            if (BoardPages.Count == 0) {
                await AddPage();
            }
        }

        private void IccWrapperInkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e) {
            var ic = (IccInkCanvas)sender;

            ic.Strokes.Remove(e.Stroke);
            BoardPages[CurrentPageIndex].Dispatcher.Invoke(() => {
                var _c = (InkCanvas)BoardPages[CurrentPageIndex].Container.Child;
                _c.Strokes.Add(e.Stroke);
            });
        }

        #endregion

        #region Eraser Overlay

        private bool isEraserOverlayPointerDown = false;

        private void EraserOverlayCanvas_Loaded(object sender, RoutedEventArgs e) {
            var bd = (Canvas)sender;
            bd.StylusDown += ((o, args) => {
                e.Handled = true;
                if (args.StylusDevice.TabletDevice.Type == TabletDeviceType.Stylus) ((Canvas)o).CaptureStylus();
                EraserOverlay_PointerDown(sender);
            });
            bd.StylusUp += ((o, args) => {
                e.Handled = true;
                if (args.StylusDevice.TabletDevice.Type == TabletDeviceType.Stylus) ((Canvas)o).ReleaseStylusCapture();
                EraserOverlay_PointerUp(sender);
            });
            bd.StylusMove += ((o, args) => {
                e.Handled = true;
                EraserOverlay_PointerMove(sender, args.GetPosition(WrapperInkCanvas));
            });
            bd.MouseDown += ((o, args) => {
                ((Canvas)o).CaptureMouse();
                EraserOverlay_PointerDown(sender);
            });
            bd.MouseUp += ((o, args) => {
                ((Canvas)o).ReleaseMouseCapture();
                EraserOverlay_PointerUp(sender);
            });
            bd.MouseMove += ((o, args) => {
                EraserOverlay_PointerMove(sender, args.GetPosition(WrapperInkCanvas));
            });
        }

        private void EraserOverlay_PointerDown(object sender) {
            
        }

        private void EraserOverlay_PointerUp(object sender) {

        }

        private void EraserOverlay_PointerMove(object sender, Point pt) {
            
        }

        #endregion

        public IccBoard() {
            InitializeComponent();
        }

    }
}
