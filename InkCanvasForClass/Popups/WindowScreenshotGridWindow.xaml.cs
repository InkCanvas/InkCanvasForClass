using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using iNKORE.UI.WPF.Modern.Controls;
using Vanara.PInvoke;

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

        private static ImageSource IconToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        private class Win {
            public string Title { get; set; }
            public BitmapImage Bitmap { get; set; }
            public ImageSource Icon { get; set; }
            public HWND Handle { get; set; }
            public Bitmap OriginBitmap { get; set; }
        }

        private MainWindow mainWindow;

        private ObservableCollection<Win> _windows = new ObservableCollection<Win>();

        public WindowScreenshotGridWindow(MainWindow.WindowInformation[] wins, MainWindow mainWindow) {
            InitializeComponent();
            WindowsItemControl.ItemsSource = _windows;
            foreach (var windowInformation in wins) {
                _windows.Add(new Win() {
                    Title = windowInformation.Title,
                    Bitmap = BitmapToImageSource(windowInformation.WindowBitmap),
                    Handle = windowInformation.hwnd,
                    OriginBitmap = windowInformation.WindowBitmap,
                    Icon = IconToImageSource(windowInformation.AppIcon)
                });
                Trace.WriteLine(windowInformation.Title);
            }

            this.mainWindow = mainWindow;
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            _windows.Clear();
        }

        private void WindowItem_MouseUp(object sender, MouseButtonEventArgs e) {
            var item = ((SimpleStackPanel)sender).Tag as Win;
            string savePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            item.OriginBitmap.Save(savePath + @"\" + DateTime.Now.ToString("u").Replace(':', '-') + ".bmp", ImageFormat.Bmp);
        }
    }
}
