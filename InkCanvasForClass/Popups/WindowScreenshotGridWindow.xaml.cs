using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
using System.Windows.Shapes;

namespace Ink_Canvas.Popups
{
    /// <summary>
    /// WindowScreenshotGridWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WindowScreenshotGridWindow : Window
    {

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private class Win {
            public string Title { get; set; }
            public BitmapImage Bitmap { get; set; }
        }

        private ObservableCollection<Win> _windows = new ObservableCollection<Win>();

        public WindowScreenshotGridWindow(MainWindow.WindowInformation[] wins) {
            InitializeComponent();
            _windows.Clear();
            WindowsItemControl.ItemsSource = _windows;
            foreach (var windowInformation in wins) {
                _windows.Add(new Win() {
                    Title = windowInformation.Title,
                    Bitmap = BitmapToImageSource(windowInformation.WindowBitmap)
                });
                Trace.WriteLine(windowInformation.Title);
            }
        }
    }
}
