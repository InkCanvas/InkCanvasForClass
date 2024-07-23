using iNKORE.UI.WPF.Modern.Controls;
using Newtonsoft.Json.Linq;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ColorPicker;
using iNKORE.UI.WPF.Helpers;
using static Ink_Canvas.Popups.ColorPalette;
using System.Drawing;
using Ink_Canvas.Helpers;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Image = System.Windows.Controls.Image;

namespace Ink_Canvas.Popups {
    public partial class ColorPalette : UserControl {
        public enum ColorPaletteColor {
            ColorBlack,
            ColorWhite,
            ColorRed,
            ColorOrange,
            ColorYellow,
            ColorLime,
            ColorGreen,
            ColorTeal,
            ColorCyan,
            ColorBlue,
            ColorIndigo,
            ColorPurple,
            ColorFuchsia,
            ColorPink,
            ColorRose,
            ColorCustom
        };

        private Color[] _darkColors = new Color[] {
            Color.FromRgb(9, 9, 11),
            Color.FromRgb(250, 250, 250),
            Color.FromRgb(220, 38, 38),
            Color.FromRgb(234, 88, 12), 
            Color.FromRgb(250, 204, 21), 
            Color.FromRgb(101, 163, 13),
            Color.FromRgb(22, 163, 74),
            Color.FromRgb(13, 148, 136),
            Color.FromRgb(8, 145, 178),
            Color.FromRgb(37, 99, 235),
            Color.FromRgb(79, 70, 229),
            Color.FromRgb(124, 58, 237),
            Color.FromRgb(192, 38, 211),
            Color.FromRgb(219, 39, 119),
            Color.FromRgb(225, 29, 72), 
        };

        private Color[] _lightColors = new Color[] {
            Color.FromRgb(9, 9, 11),
            Color.FromRgb(250, 250, 250),
            Color.FromRgb(239, 68, 68),
            Color.FromRgb(249, 115, 22),
            Color.FromRgb(253, 224, 71),
            Color.FromRgb(163, 230, 53),
            Color.FromRgb(74, 222, 128),
            Color.FromRgb(94, 234, 212),
            Color.FromRgb(34, 211, 238),
            Color.FromRgb(59, 130, 246),
            Color.FromRgb(129, 140, 248),
            Color.FromRgb(168, 85, 247),
            Color.FromRgb(217, 70, 239),
            Color.FromRgb(236, 72, 153),
            Color.FromRgb(244, 63, 94),
        };

        public string[] ColorPaletteColorStrings = new[] {
            "black", "white", "red", "orange", "yellow", "lime", "green", "teal", "cyan", "blue", "indigo", "purple",
            "fuchsia", "pink", "rose"
        };

        public Border[] ColorPaletteColorButtonBorders;
        public Border[] PenModeTabButtonBorders;
        public SimpleStackPanel[] PenModeTabButtonIndicators;
        public GeometryDrawing[] PenModeTabButtonIcons;
        public TextBlock[] PenModeTabButtonTexts;

        private bool _usingDarkColors = true;

        public bool UsingDarkColors {
            get => _usingDarkColors;
            set {
                var pre = _usingDarkColors;
                _usingDarkColors = value;
                ColorModeChanged?.Invoke(this, new ColorModeChangedEventArgs()
                {
                    IsPreviousUsedDarkColor = pre,
                    IsNowUsingDarkColor = value,
                    TriggerMode = TriggerMode.TriggeredByCode,
                });
            }
        }

        private ColorPaletteColor _colorSelected = ColorPaletteColor.ColorRed;
        public ColorPaletteColor SelectedColor {
            get => _colorSelected;
            set {
                var pre = _colorSelected;
                _colorSelected = value;
                ColorSelectionChanged?.Invoke(this, new ColorSelectionChangedEventArgs {
                    PreviousColor = pre,
                    NowColor = value,
                    TriggerMode = TriggerMode.TriggeredByCode,
                });
            }
        }

        private bool _inkRecognition = true;
        public bool InkRecognition {
            get => _inkRecognition;
            set {
                var pre = _inkRecognition;
                _inkRecognition = value;
                InkRecognitionChanged?.Invoke(this, new InkRecognitionChangedEventArgs
                {
                    PreviousStatus = pre,
                    NowStatus = value,
                    TriggerMode = TriggerMode.TriggeredByCode,
                });
            }
        }

        private void InkRecognitionMoreButton_MouseDown(object sender, MouseButtonEventArgs e) {
            var ircm = this.FindResource("InkRecognitionContextMenu") as ContextMenu;
            var transform = InkRecognitionMoreButton.TransformToVisual(this);
            var pt = transform.Transform(new Point(0, 0));
            ircm.VerticalOffset = pt.Y-4;
            ircm.HorizontalOffset = pt.X-4;
            ircm.IsOpen = !ircm.IsOpen;
        }

        private void UpdateInkRecognitionContextMenyDisplayStatus() {
            var ircm = (ContextMenu)this.FindResource("InkRecognitionContextMenu");
            var enableRecog = ircm.Items[0] as MenuItem;
            enableRecog.IsChecked = _inkRecognition;
            var recogTri = ircm.Items[2] as MenuItem;
            var recogQua = ircm.Items[3] as MenuItem;
            var recogEll = ircm.Items[4] as MenuItem;
            var recogPlg = ircm.Items[5] as MenuItem;
            var recogSub = new MenuItem[] {
                recogTri, recogQua, recogEll, recogPlg,
            };
            foreach (var mi in recogSub) mi.IsEnabled = _inkRecognition;
        }

        private void InkRecognitionContextMenuItem_Checked(object sender, RoutedEventArgs e) {
            var mi = (MenuItem)sender;
            if (mi.Name == "EnableRecog") {
                var pre = _inkRecognition;
                Trace.WriteLine(mi.IsChecked);
                _inkRecognition = mi.IsChecked;

                UpdateInkRecognitionContextMenyDisplayStatus();
                InkRecognitionToggleSwitchImage.Source =
                    this.FindResource(_inkRecognition ? "SwitchOnImage" : "SwitchOffImage") as DrawingImage;

                InkRecognitionChanged?.Invoke(this, new InkRecognitionChangedEventArgs
                {
                    PreviousStatus = pre,
                    NowStatus = _inkRecognition,
                    TriggerMode = TriggerMode.TriggeredByUser,
                });
            } else {
                UpdateInkRecognitionContextMenyDisplayStatus();
                InkRecognitionChanged?.Invoke(this, new InkRecognitionChangedEventArgs
                {
                    PreviousStatus = _inkRecognition,
                    NowStatus = _inkRecognition,
                    TriggerMode = TriggerMode.TriggeredByUser,
                });
            }
        }

        private void InkRecognitionToggleSwitchButton_Clicked(object sender, RoutedEventArgs e) {
            var pre = _inkRecognition;
            _inkRecognition = !_inkRecognition;
            InkRecognitionToggleSwitchImage.Source =
                this.FindResource(_inkRecognition ? "SwitchOnImage" : "SwitchOffImage") as DrawingImage;
            UpdateInkRecognitionContextMenyDisplayStatus();
            InkRecognitionChanged?.Invoke(this, new InkRecognitionChangedEventArgs
            {
                PreviousStatus = pre,
                NowStatus = _inkRecognition,
                TriggerMode = TriggerMode.TriggeredByUser,
            });
        }

        private void InkRecognitionContextMenu_Closed(object sender, RoutedEventArgs e) {
            InkRecognitionMoreButton.Background = new SolidColorBrush(Colors.Transparent);
            UpdateInkRecognitionContextMenyDisplayStatus();
        }

        private void InkRecognitionContextMenu_Opened(object sender, RoutedEventArgs e) {
            InkRecognitionMoreButton.Background = new SolidColorBrush(Color.FromArgb(34,39, 39, 42));
            UpdateInkRecognitionContextMenyDisplayStatus();
        }

        public enum PenMode {
            PenMode,
            HighlighterMode,
            LaserPenMode
        }

        private PenMode _penModeSelected = PenMode.PenMode;
        public PenMode PenModeSelected
        {
            get => _penModeSelected;
            set
            {
                var pre = _penModeSelected;
                _penModeSelected = value;
                PenModeChanged?.Invoke(this, new PenModeChangedEventArgs()
                {
                    PreviousMode = pre,
                    NowMode = value,
                    TriggerMode = TriggerMode.TriggeredByCode,
                });
            }
        }

        public enum TriggerMode {
            TriggeredByUser,
            TriggeredByCode
        }

        public enum PressureSimulation {
            PointSimulate,
            VelocitySimulate,
            None
        }

        private PressureSimulation _simulatePressure = PressureSimulation.PointSimulate;
        public PressureSimulation SimulatePressure
        {
            get => _simulatePressure;
            set
            {
                var pre = _simulatePressure;
                _simulatePressure = value;
                PressureSimulationChanged?.Invoke(this, new PressureSimulationChangedEventArgs()
                {
                    PreviousMode = pre,
                    NowMode = value,
                    TriggerMode = TriggerMode.TriggeredByCode,
                });
            }
        }

        private void UpdateSimulatePressureContextMenuDisplayStatus()
        {
            var spcm = (ContextMenu)this.FindResource("SimulatePressureContextMenu");
            var pointSP = spcm.Items[0] as MenuItem;
            var velocitySP = spcm.Items[1] as MenuItem;
            var noneSP = spcm.Items[2] as MenuItem;
            var pressSub = new MenuItem[] {
                pointSP, velocitySP, noneSP
            };
            isSimulatePressureCheckedByUser = false;
            foreach (var mi in pressSub) {
                if (mi.Name=="PointSP") mi.IsChecked = _simulatePressure == PressureSimulation.PointSimulate;
                else if (mi.Name == "VelocitySP") mi.IsChecked = _simulatePressure == PressureSimulation.VelocitySimulate;
                else if (mi.Name == "NoneSP") mi.IsChecked = _simulatePressure == PressureSimulation.None;
            }
            isSimulatePressureCheckedByUser = true;
        }

        private void SimulatePressureContextMenu_Closed(object sender, RoutedEventArgs e) {
            SimulatePressureMoreButton.Background = new SolidColorBrush(Colors.Transparent);
            UpdateSimulatePressureContextMenuDisplayStatus();
        }

        private void SimulatePressureContextMenu_Opened(object sender, RoutedEventArgs e) {
            SimulatePressureMoreButton.Background = new SolidColorBrush(Color.FromArgb(34, 39, 39, 42));
            UpdateSimulatePressureContextMenuDisplayStatus();
        }

        private bool isSimulatePressureCheckedByUser = true;

        private void SimulatePressureContextMenuItem_Checked(object sender, RoutedEventArgs e) {
            if (!isSimulatePressureCheckedByUser) return;
            var mi = (MenuItem)sender;
            var pre = _simulatePressure;
            Trace.WriteLine(mi.Name);
            _simulatePressure = mi.Name == "PointSP" ? PressureSimulation.PointSimulate : mi.Name == "VelocitySP" ? PressureSimulation.VelocitySimulate : PressureSimulation.None;
            SimulatePressureToggleSwitchImage.Source =
                this.FindResource(_simulatePressure != PressureSimulation.None ? "SwitchOnImage" : "SwitchOffImage") as DrawingImage;
            UpdateSimulatePressureContextMenuDisplayStatus();
            PressureSimulationChanged?.Invoke(this, new PressureSimulationChangedEventArgs()
            {
                PreviousMode = pre,
                NowMode = _simulatePressure,
                TriggerMode = TriggerMode.TriggeredByUser,
            });
        }

        private void SimulatePressureToggleSwitchButton_Clicked(object sender, RoutedEventArgs e) {
            var pre = _simulatePressure;
            _simulatePressure = _simulatePressure == PressureSimulation.None ? PressureSimulation.PointSimulate : PressureSimulation.None;
            SimulatePressureToggleSwitchImage.Source =
                this.FindResource(_simulatePressure != PressureSimulation.None ? "SwitchOnImage" : "SwitchOffImage") as DrawingImage;
            UpdateSimulatePressureContextMenuDisplayStatus();
            PressureSimulationChanged?.Invoke(this, new PressureSimulationChangedEventArgs()
            {
                PreviousMode = pre,
                NowMode = _simulatePressure,
                TriggerMode = TriggerMode.TriggeredByUser,
            });
        }

        private void SimulatePressureMoreButton_MouseDown(object sender, MouseButtonEventArgs e) {
            var spcm = this.FindResource("SimulatePressureContextMenu") as ContextMenu;
            var transform = SimulatePressureMoreButton.TransformToVisual(this);
            var pt = transform.Transform(new Point(0, 0));
            spcm.VerticalOffset = pt.Y - 4;
            spcm.HorizontalOffset = pt.X - 4;
            spcm.IsOpen = !spcm.IsOpen;
        }

        private Border lastColorBtnDown;

        public void UpdateColorButtonsDisplayStatus() {
            foreach (var bd in ColorPaletteColorButtonBorders) {
                bd.Child = null;
            }

            UpdateCustomColorButtonDisplayStatus();
            if (_colorSelected == ColorPaletteColor.ColorCustom) return;
            var index = (int)_colorSelected;
            var bdSel = ColorPaletteColorButtonBorders[index];
            Image checkedImage = new Image();
            checkedImage.Width = 24;
            checkedImage.Height = 24;
            var checkLight = this.FindResource("CheckedLightIcon");
            var checkDark = this.FindResource("CheckedDarkIcon");
            if (ColorUtilities.GetReverseForegroundColor(ColorUtilities.GetGrayLevel((_usingDarkColors?_darkColors:_lightColors)[(int)_colorSelected])) == Colors.Black) checkedImage.Source = checkDark as DrawingImage;
            else checkedImage.Source = checkLight as DrawingImage;
            bdSel.Child = checkedImage;
        }

        public void UpdatePenModeButtonsDisplayStatus() {
            foreach (var bd in PenModeTabButtonBorders) {
                bd.Background = new SolidColorBrush(Colors.Transparent);
            }
            foreach (var indicator in PenModeTabButtonIndicators) {
                indicator.Visibility = Visibility.Hidden;
            }
            foreach (var gd in PenModeTabButtonIcons) {
                gd.Brush = new SolidColorBrush(Color.FromRgb(63, 63, 70));
            }
            foreach (var text in PenModeTabButtonTexts) {
                text.Foreground = new SolidColorBrush(Color.FromRgb(63, 63, 70));
                text.FontWeight = FontWeights.Normal;
            }

            PenModeTabButtonBorders[(int)_penModeSelected].Background = new SolidColorBrush(Color.FromArgb(34, 59, 130, 246));
            PenModeTabButtonIndicators[(int)_penModeSelected].Visibility = Visibility.Visible;
            PenModeTabButtonIcons[(int)_penModeSelected].Brush = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            PenModeTabButtonTexts[(int)_penModeSelected].Foreground = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            PenModeTabButtonTexts[(int)_penModeSelected].FontWeight = FontWeights.Bold;
        }

        private void PenTabButton_MouseDown(object sender, MouseButtonEventArgs e) {
            var pre = _penModeSelected;
            _penModeSelected = (PenMode)Array.IndexOf(PenModeTabButtonBorders, (Border)sender);
            UpdatePenModeButtonsDisplayStatus();

            PenModeChanged?.Invoke(this, new PenModeChangedEventArgs()
            {
                PreviousMode = pre,
                NowMode = _penModeSelected,
                TriggerMode = TriggerMode.TriggeredByUser,
            });
        }

        private void ColorButton_MouseDown(object sender, MouseButtonEventArgs e) {
            lastColorBtnDown = sender as Border;
            ColorBtnStoryboardScaleSmaller(sender);
        }

        private void ColorButton_MouseUp(object sender, MouseButtonEventArgs e) {
            if (lastColorBtnDown != sender) return;
            lastColorBtnDown = null;
            var border = (Border)sender;
            var index = Array.IndexOf(ColorPaletteColorStrings, border.Name.ToLower());

            var pre = _colorSelected;
            _colorSelected = (ColorPaletteColor)index;
            UpdateColorButtonsDisplayStatus();
            ColorBtnStoryboardScaleLarger(sender);

            ColorSelectionChanged?.Invoke(this, new ColorSelectionChangedEventArgs {
                PreviousColor = pre,
                NowColor = (ColorPaletteColor)index,
                TriggerMode = TriggerMode.TriggeredByUser,
            });
        }

        private void UpdateCustomColorPickerDisplayStatus() {
            if (_customColor == null) {
                CustomColorHexTextBox.Text = "请在上方选择一个颜色";
                CustomColorHexBorder.Background = new SolidColorBrush(Colors.Transparent);
            } else {
                CustomColorHexTextBox.Text = "#" + CustomColorPicker.SelectedColor.R.ToString("X2") + CustomColorPicker.SelectedColor.G.ToString("X2") + CustomColorPicker.SelectedColor.B.ToString("X2");
                CustomColorHexBorder.Background = new SolidColorBrush(CustomColorPicker.SelectedColor);
            }
        }

        private void UpdateCustomColorButtonDisplayStatus() {
            if (_customColor == null) {
                CustomColorButtonColorBorder.Visibility = Visibility.Collapsed;
                CustomColorButtonIcon.Visibility = Visibility.Visible;
            } else {
                CustomColorButtonColorBorder.Visibility = Visibility.Visible;
                CustomColorButtonColorBorder.Background = new SolidColorBrush((Color)_customColor);
                CustomColorButtonIcon.Visibility = Visibility.Collapsed;
                if (_colorSelected == ColorPaletteColor.ColorCustom)
                    CustomColorButtonColorBorderCheckIcon.Visibility = Visibility.Visible;
                else CustomColorButtonColorBorderCheckIcon.Visibility = Visibility.Collapsed;
                CustomColorButtonColorBorderCheckIcon.Source =
                    this.FindResource(ColorUtilities.GetReverseForegroundColor(ColorUtilities.GetGrayLevel((Color)_customColor)) == Colors.White
                        ? "CheckedLightIcon"
                        : "CheckedDarkIcon") as DrawingImage;
            }
        }

        private void CustomColorPicker_ColorChanged(object sender, RoutedEventArgs e) {
            var cp = sender as SquarePicker;
            _customColor = cp.SelectedColor;
            if (_colorSelected != ColorPaletteColor.ColorCustom) _colorSelected = ColorPaletteColor.ColorCustom;
            UpdateCustomColorPickerDisplayStatus();
            UpdateCustomColorButtonDisplayStatus();
            UpdateColorButtonsDisplayStatus();
        }

        private void ColorBtnStoryBoardScaleAnimation(object sender, double from, double to) {
            var border = sender as Border;

            var sb = new Storyboard();
            var scaleAnimationX = new DoubleAnimation() {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(100))
            };
            var scaleAnimationY = new DoubleAnimation() {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(100))
            };
            scaleAnimationY.EasingFunction = new CubicEase();
            scaleAnimationX.EasingFunction = new CubicEase();
            Storyboard.SetTargetProperty(scaleAnimationX,
                new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(scaleAnimationY,
                new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            sb.Children.Add(scaleAnimationX);
            sb.Children.Add(scaleAnimationY);

            sb.Begin(border);
        }

        private void ColorBtnStoryboardScaleSmaller(object sender) {
            ColorBtnStoryBoardScaleAnimation(sender, 1D, 0.9);
        }

        private void ColorBtnStoryboardScaleLarger(object sender) {
            ColorBtnStoryBoardScaleAnimation(sender, 0.9, 1D);
        }

        private void ColorButton_MouseLeave(object sender, MouseEventArgs e) {
            if (lastColorBtnDown != null) {
                var la = lastColorBtnDown as Border;
                var st = la.RenderTransform as ScaleTransform;
                if (st.ScaleX != 1 && st.ScaleY != 1) ColorBtnStoryboardScaleLarger(lastColorBtnDown);
            }

            lastColorBtnDown = null;
        }

        public class ColorSelectionChangedEventArgs : EventArgs {
            public ColorPaletteColor PreviousColor { get; set; }
            public ColorPaletteColor NowColor { get; set; }
            public TriggerMode TriggerMode { get; set; }
            public Color CustomColor { get; set; }
        }

        public class ColorModeChangedEventArgs : EventArgs
        {
            public bool IsPreviousUsedDarkColor { get; set; }
            public bool IsNowUsingDarkColor { get; set; }
            public TriggerMode TriggerMode { get; set; }
        }

        private void UpdateColorPaletteColorsAndColorModeChangeButton() {
            foreach (var bd in ColorPaletteColorButtonBorders) {
                bd.Background =
                    new SolidColorBrush((_usingDarkColors ? _darkColors : _lightColors)[
                        Array.IndexOf(ColorPaletteColorStrings, bd.Name.ToLower())]);
            }

            var tb = ((SimpleStackPanel)ColorModeChangeButton.Content).Children.OfType<TextBlock>().Single();
            tb.Text = _usingDarkColors ? "亮色" : "暗色";
        }

        private void ColorModeChangeButton_Clicked(object sender, RoutedEventArgs e) {
            var pre = _usingDarkColors;
            _usingDarkColors = !_usingDarkColors;
            UpdateColorPaletteColorsAndColorModeChangeButton();
            UpdateColorButtonsDisplayStatus();
            ColorModeChanged?.Invoke(this, new ColorModeChangedEventArgs()
            {
                IsPreviousUsedDarkColor = pre,
                IsNowUsingDarkColor = _usingDarkColors,
                TriggerMode = TriggerMode.TriggeredByUser,
            });
        }

        public class PenModeChangedEventArgs : EventArgs
        {
            public PenMode PreviousMode { get; set; }
            public PenMode NowMode { get; set; }
            public TriggerMode TriggerMode { get; set; }
        }

        public class PressureSimulationChangedEventArgs : EventArgs
        {
            public PressureSimulation PreviousMode { get; set; }
            public PressureSimulation NowMode { get; set; }
            public TriggerMode TriggerMode { get; set; }
        }

        public class InkRecognitionOptions {
            public bool isEnableInkRecognition;
            public bool isRecognizeEllipse;
            public bool isRecognizeTriangle;
            public bool isRecognizeQuadrilateral;
            public bool isRecognizePolygon;
        }

        private Color? _customColor = null;

        private void CustomColorButton_Clicked(object sender, RoutedEventArgs e) {
            if (_customColor == null) {
                CustomColorPanel.Visibility = Visibility.Visible;
            } else {
                if (_colorSelected == ColorPaletteColor.ColorCustom) CustomColorPanel.Visibility = Visibility.Visible;
                else {
                    _colorSelected = ColorPaletteColor.ColorCustom;
                    UpdateColorButtonsDisplayStatus();
                    UpdateCustomColorButtonDisplayStatus();
                    UpdateCustomColorPickerDisplayStatus();
                }
            }
        }

        private void BackToPaletteButton_Clicked(object sender, RoutedEventArgs e) {
            CustomColorPanel.Visibility = Visibility.Collapsed;
        }

        public class InkRecognitionChangedEventArgs : EventArgs {
            public bool PreviousStatus { get; set; }
            public bool NowStatus { get; set; }
            public TriggerMode TriggerMode { get; set; }
            public InkRecognitionOptions Options { get; set; }
        }

        public event EventHandler<ColorSelectionChangedEventArgs> ColorSelectionChanged;
        public event EventHandler<PenModeChangedEventArgs> PenModeChanged;
        public event EventHandler<InkRecognitionChangedEventArgs> InkRecognitionChanged;
        public event EventHandler<PressureSimulationChangedEventArgs> PressureSimulationChanged;
        public event EventHandler<ColorModeChangedEventArgs> ColorModeChanged;

        public ColorPalette() {
            InitializeComponent();
            ColorPaletteColorButtonBorders = new Border[] {
                Black, White, Red, Orange, Yellow, Lime, Green, Teal, Cyan, Blue, Indigo, Purple, Fuchsia, Pink, Rose
            };
            PenModeTabButtonBorders = new Border[] {
                PenTabButton, HighlighterTabButton, LaserPenTabButton
            };
            PenModeTabButtonIndicators = new SimpleStackPanel[] {
                PenTabButtonIndicator, HighlighterTabButtonIndicator, LaserPenTabButtonIndicator
            };
            PenModeTabButtonIcons = new GeometryDrawing[] {
                PenTabButtonIcon, HighlighterTabButtonIcon, LaserPenTabButtonIcon
            };
            PenModeTabButtonTexts = new TextBlock[] {
                PenTabButtonText, HighlighterTabButtonText, LaserPenTabButtonText
            };
            foreach (var bd in ColorPaletteColorButtonBorders) {
                bd.RenderTransformOrigin = new Point(0.5, 0.5);
                bd.RenderTransform = new ScaleTransform(1, 1);
            }

            UpdatePenModeButtonsDisplayStatus();
            UpdateColorButtonsDisplayStatus();
            UpdateColorPaletteColorsAndColorModeChangeButton();
        }
    }
}