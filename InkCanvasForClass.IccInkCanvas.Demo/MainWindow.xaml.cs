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

namespace InkCanvasForClass.IccInkCanvas.Demo {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            RegisterEventsForBoardSettings();
            UpdateBoardSettingsTextBlock();
        }

        private void UpdateBoardSettingsTextBlock() {
            BoardSettingsTextBlock.Text =
                $@"Width:{IccBoard.BoardSettings.NibWidth}  Height:{IccBoard.BoardSettings.NibHeight}  Color:{IccBoard.BoardSettings.NibColor.ToString()}";
        }

        private void RegisterEventsForBoardSettings() {
            IccBoard.BoardSettings.NibWidthChanged += (s, e) => UpdateBoardSettingsTextBlock();
            IccBoard.BoardSettings.NibHeightChanged += (s, e) => UpdateBoardSettingsTextBlock();
            IccBoard.BoardSettings.NibColorChanged += (s, e) => UpdateBoardSettingsTextBlock();
        }

        private void EditingModeChangeToNone_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.EditingMode = EditingMode.NoneWithHitTest;
        }
        private void EditingModeChangeToWriting_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.EditingMode = EditingMode.Writing;
        }
        private void BoardSettingsNibSizeChangeTo2_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.BoardSettings.NibWidth = 2;
            IccBoard.BoardSettings.NibHeight = 2;
        }
        private void BoardSettingsNibSizeChangeTo4_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.BoardSettings.NibWidth = 4;
            IccBoard.BoardSettings.NibHeight = 4;
        }
        private void BoardSettingsNibSizeChangeTo6_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.BoardSettings.NibWidth = 6;
            IccBoard.BoardSettings.NibHeight = 6;
        }
        private void BoardSettingsNibColorChangeToRed_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.BoardSettings.NibColor = Color.FromRgb(220, 38, 38);
        }
        private void BoardSettingsNibColorChangeToBlue_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.BoardSettings.NibColor = Color.FromRgb(37, 99, 235);
        }
        private void BoardSettingsNibColorChangeToGreen_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.BoardSettings.NibColor = Color.FromRgb(34, 197, 94);
        }
        private void BoardSettingsNibColorChangeToBlack_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.BoardSettings.NibColor = Colors.Black;
        }
        private void BoardSettingsStrokeNibStyleChangeToSolid_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.BoardSettings.StrokeNibStyle = StrokeNibStyle.Solid;
        }
        private void BoardSettingsStrokeNibStyleChangeToBeautiful_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.BoardSettings.StrokeNibStyle = StrokeNibStyle.Beautiful;
        }
    }
}
