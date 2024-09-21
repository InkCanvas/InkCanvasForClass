using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace InkCanvasForClass.IccInkCanvas.Settings {
    public class BoardSettings {
        public BoardSettings() {}

        /// <summary>
        /// 笔尖长度
        /// </summary>
        public double NibWidth { get; set; } = 4.00;

        /// <summary>
        /// 笔尖高度
        /// </summary>
        public double NibHeight { get; set; } = 4.00;

        /// <summary>
        /// 笔尖大小，适合笔尖类型为普通笔时使用
        /// </summary>
        public double NibSize {
            get => (NibWidth + NibHeight) / 2;
            set => NibWidth = NibHeight = value;
        }

        public NibType NibType { get; set; } = NibType.Default;

        /// <summary>
        /// 笔尖颜色
        /// </summary>
        public Color NibColor { get; set; } = Colors.Black;

        /// <summary>
        /// 笔锋样式类型，默认有笔锋
        /// </summary>
        public StrokeNibStyle StrokeNibStyle { get; set; } = StrokeNibStyle.Beautiful;
    }
}
