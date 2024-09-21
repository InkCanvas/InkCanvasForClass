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
        }

        private void EditingModeChangeToNone_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.EditingMode = EditingMode.None;
        }
        private void EditingModeChangeToWriting_ButtonClick(object sender, RoutedEventArgs e) {
            IccBoard.EditingMode = EditingMode.Writing;
        }
    }
}
