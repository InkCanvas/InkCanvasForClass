using Ink_Canvas.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Ink_Canvas {
    public partial class MainWindow : Window {
        private StrokeCollection[] strokeCollections = new StrokeCollection[101];
        private bool[] whiteboadLastModeIsRedo = new bool[101];
        private StrokeCollection lastTouchDownStrokeCollection = new StrokeCollection();

        private int CurrentWhiteboardIndex = 1;
        private int WhiteboardTotalCount = 1;
        private TimeMachineHistory[][] TimeMachineHistories = new TimeMachineHistory[101][]; //最多99页，0用来存储非白板时的墨迹以便还原

        private void SaveStrokes(bool isBackupMain = false) {
            if (isBackupMain) {
                var timeMachineHistory = timeMachine.ExportTimeMachineHistory();
                TimeMachineHistories[0] = timeMachineHistory;
                timeMachine.ClearStrokeHistory();
            }
            else {
                var timeMachineHistory = timeMachine.ExportTimeMachineHistory();
                TimeMachineHistories[CurrentWhiteboardIndex] = timeMachineHistory;
                timeMachine.ClearStrokeHistory();
            }
        }

        private void ClearStrokes(bool isErasedByCode) {
            _currentCommitType = CommitReason.ClearingCanvas;
            if (isErasedByCode) _currentCommitType = CommitReason.CodeInput;
            inkCanvas.Strokes.Clear();
            _currentCommitType = CommitReason.UserInput;
        }

        private void RestoreStrokes(bool isBackupMain = false) {
            try {
                if (TimeMachineHistories[CurrentWhiteboardIndex] == null) return; //防止白板打开后不居中
                if (isBackupMain) {
                    timeMachine.ImportTimeMachineHistory(TimeMachineHistories[0]);
                    foreach (var item in TimeMachineHistories[0]) ApplyHistoryToCanvas(item);
                }
                else {
                    timeMachine.ImportTimeMachineHistory(TimeMachineHistories[CurrentWhiteboardIndex]);
                    foreach (var item in TimeMachineHistories[CurrentWhiteboardIndex]) ApplyHistoryToCanvas(item);
                }
            }
            catch {
                // ignored
            }
        }

        private void BtnWhiteBoardPageIndex_Click(object sender, EventArgs e) {
            if (BoardBorderLeftPageListView.Visibility == Visibility.Visible) {
                AnimationsHelper.HideWithSlideAndFade(BoardBorderLeftPageListView);
            } else {
                RefreshBlackBoardLeftSidePageListView();

                try
                {
                    var sb = new Storyboard();

                    // 渐变动画
                    var fadeInAnimation = new DoubleAnimation
                    {
                        From = 0.5,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.15)
                    };
                    fadeInAnimation.EasingFunction = new CubicEase();

                    Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));

                    // 滑动动画
                    var slideAnimation = new DoubleAnimation
                    {
                        From = BoardBorderLeftPageListView.RenderTransform.Value.OffsetY + 10, // 滑动距离
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.15)
                    };
                    Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

                    slideAnimation.EasingFunction = new CubicEase();

                    sb.Children.Add(fadeInAnimation);
                    sb.Children.Add(slideAnimation);

                    sb.Completed += (_,__) => {
                        BlackBoardLeftSidePageListView.ScrollIntoView(BlackBoardLeftSidePageListView.SelectedItem);
                    };

                    BoardBorderLeftPageListView.Visibility = Visibility.Visible;
                    BoardBorderLeftPageListView.RenderTransform = new TranslateTransform();

                    sb.Begin((FrameworkElement)BoardBorderLeftPageListView);
                }
                catch { }
            }
            
        }

        private void BtnWhiteBoardSwitchPrevious_Click(object sender, EventArgs e) {
            if (CurrentWhiteboardIndex <= 1) return;

            SaveStrokes();

            ClearStrokes(true);
            CurrentWhiteboardIndex--;

            RestoreStrokes();

            UpdateIndexInfoDisplay();
        }

        private void BtnWhiteBoardSwitchNext_Click(object sender, EventArgs e) {
            if (Settings.Automation.IsAutoSaveStrokesAtClear &&
                inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber) SaveScreenShot(true);
            if (CurrentWhiteboardIndex >= WhiteboardTotalCount) {
                BtnWhiteBoardAdd_Click(sender, e);
                return;
            }

            SaveStrokes();

            ClearStrokes(true);
            CurrentWhiteboardIndex++;

            RestoreStrokes();

            UpdateIndexInfoDisplay();
        }

        private void BtnWhiteBoardAdd_Click(object sender, EventArgs e) {
            if (WhiteboardTotalCount >= 99) return;
            if (Settings.Automation.IsAutoSaveStrokesAtClear &&
                inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber) SaveScreenShot(true);
            SaveStrokes();
            ClearStrokes(true);

            WhiteboardTotalCount++;
            CurrentWhiteboardIndex++;

            if (CurrentWhiteboardIndex != WhiteboardTotalCount)
                for (var i = WhiteboardTotalCount; i > CurrentWhiteboardIndex; i--)
                    TimeMachineHistories[i] = TimeMachineHistories[i - 1];

            UpdateIndexInfoDisplay();

            if (WhiteboardTotalCount >= 99) BtnWhiteBoardAdd.IsEnabled = false;
        }

        private void BtnWhiteBoardDelete_Click(object sender, RoutedEventArgs e) {
            ClearStrokes(true);

            if (CurrentWhiteboardIndex != WhiteboardTotalCount)
                for (var i = CurrentWhiteboardIndex; i <= WhiteboardTotalCount; i++)
                    TimeMachineHistories[i] = TimeMachineHistories[i + 1];
            else
                CurrentWhiteboardIndex--;

            WhiteboardTotalCount--;

            RestoreStrokes();

            UpdateIndexInfoDisplay();

            if (WhiteboardTotalCount < 99) BtnWhiteBoardAdd.IsEnabled = true;
        }

        private void UpdateIndexInfoDisplay() {
            TextBlockWhiteBoardIndexInfo.Text =
                $"{CurrentWhiteboardIndex}/{WhiteboardTotalCount}";

            if (CurrentWhiteboardIndex == WhiteboardTotalCount) {
                var newImageSource = new BitmapImage();
                newImageSource.BeginInit();
                newImageSource.UriSource = new Uri("/Resources/Icons-Fluent/ic_fluent_add_circle_24_regular.png",
                    UriKind.RelativeOrAbsolute);
                newImageSource.EndInit();
                //BoardLeftPannelNextPage.Source = newImageSource;
                //BoardRightPannelNextPage.Source = newImageSource;
                //BoardRightPannelNextPageTextBlock.Text = "加页";
                //BoardLeftPannelNextPageTextBlock.Text = "加页";
            }
            else {
                var newImageSource = new BitmapImage();
                newImageSource.BeginInit();
                newImageSource.UriSource =
                    new Uri("/Resources/Icons-Fluent/ic_fluent_arrow_circle_right_24_regular.png",
                        UriKind.RelativeOrAbsolute);
                newImageSource.EndInit();
                //BoardLeftPannelNextPage.Source = newImageSource;
                //BoardRightPannelNextPage.Source = newImageSource;
                //BoardRightPannelNextPageTextBlock.Text = "下一页";
                //BoardLeftPannelNextPageTextBlock.Text = "下一页";
            }

            BtnWhiteBoardSwitchPrevious.IsEnabled = CurrentWhiteboardIndex != 1;

            BtnWhiteBoardSwitchNext.IsEnabled = CurrentWhiteboardIndex != WhiteboardTotalCount;

            BtnWhiteBoardDelete.IsEnabled = WhiteboardTotalCount != 1;
        }
    }
}