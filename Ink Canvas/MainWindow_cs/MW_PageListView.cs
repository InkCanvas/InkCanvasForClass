using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace Ink_Canvas
{
    public partial class MainWindow : Window
    {
        private class PageListViewItem
        {
            public int Index { get; set; }
            public StrokeCollection Strokes { get; set; }
        }

        ObservableCollection<PageListViewItem> blackBoardLeftSidePageListViewObservableCollection = new ObservableCollection<PageListViewItem>();

        /// <summary>
        /// <para>刷新白板的缩略图页面列表。</para>
        /// </summary>
        private void RefreshBlackBoardLeftSidePageListView()
        {
            if (blackBoardLeftSidePageListViewObservableCollection.Count == WhiteboardTotalCount) {
                foreach (int index in Enumerable.Range(1, WhiteboardTotalCount))
                {
                    var pitem = new PageListViewItem()
                    {
                        Index = index,
                        Strokes = ApplyHistoriesToNewStrokeCollection(TimeMachineHistories[index]),
                    };
                    blackBoardLeftSidePageListViewObservableCollection[index-1] = pitem;
                }
            } else {
                blackBoardLeftSidePageListViewObservableCollection.Clear();
                foreach (int index in Enumerable.Range(1, WhiteboardTotalCount))
                {
                    var pitem = new PageListViewItem()
                    {
                        Index = index,
                        Strokes = ApplyHistoriesToNewStrokeCollection(TimeMachineHistories[index]),
                    };
                    blackBoardLeftSidePageListViewObservableCollection.Add(pitem);
                }
            }

            var _pitem = new PageListViewItem()
            {
                Index = CurrentWhiteboardIndex,
                Strokes = inkCanvas.Strokes,
            };
            blackBoardLeftSidePageListViewObservableCollection[CurrentWhiteboardIndex - 1] = _pitem;

            BlackBoardLeftSidePageListView.SelectedIndex = CurrentWhiteboardIndex -1;
        }

        private void BlackBoardLeftSidePageListView_OnMouseUp(object sender, MouseButtonEventArgs e) {
            var item = BlackBoardLeftSidePageListView.SelectedItem;
            var index = BlackBoardLeftSidePageListView.SelectedIndex;
            if (item != null)
            {
                SaveStrokes();
                ClearStrokes(true);
                CurrentWhiteboardIndex= index+1;
                RestoreStrokes();
                UpdateIndexInfoDisplay();
                BlackBoardLeftSidePageListView.SelectedIndex = index;
            }
        }
    }
}
