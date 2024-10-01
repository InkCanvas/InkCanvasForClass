using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkCanvasForClass.IccInkCanvas.Settings {
    public enum BoardPageAppendMode {
        /// <summary>
        /// 添加到所有页面的最后去
        /// </summary>
        AppendToListEnd,
        /// <summary>
        /// 添加为当前页面的下一页
        /// </summary>
        AppendAfterItem,
        /// <summary>
        /// 添加为当前页面的上一页
        /// </summary>
        AppendBeforeItem,
        /// <summary>
        /// 添加到所有页面的最前面去
        /// </summary>
        AppendToListStart
    }

    public enum PageSwitchMode {
        SwitchToPreviousPage,
        SwitchToNextPage,
    }
}
