using System;
using System.Collections.Generic;
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
    public partial class SelectionPopup : UserControl {
        public SelectionPopup() {
            InitializeComponent();
        }

        private bool isCloseButtonDown = false;
        public event EventHandler<RoutedEventArgs> SelectionPopupShouldCloseEvent;
        public event EventHandler<RoutedEventArgs> SelectAllEvent;
        public event EventHandler<RoutedEventArgs> UnSelectEvent;
        public event EventHandler<RoutedEventArgs> ReverseSelectEvent;

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
            SelectionPopupShouldCloseEvent?.Invoke(this,new RoutedEventArgs());
        }

        private void SelectAllButtonClicked(object sender, RoutedEventArgs e) {
            SelectAllEvent?.Invoke(this,new RoutedEventArgs());
        }

        private void UnSelectButtonClicked(object sender, RoutedEventArgs e) {
            UnSelectEvent?.Invoke(this,new RoutedEventArgs());
        }

        private void ReverseSelectButtonClicked(object sender, RoutedEventArgs e) {
            ReverseSelectEvent?.Invoke(this,new RoutedEventArgs());
        }
    }
}
