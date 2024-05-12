using Ink_Canvas.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Ink_Canvas {
    public partial class MainWindow : Window {
        bool isFloatingBarFolded = false, isFloatingBarChangingHideMode = false;

        private async void FoldFloatingBar_MouseUp(object sender, MouseButtonEventArgs e) {
            FloatingBarIcons_MouseUp_New(sender);
            if (sender == null) {
                foldFloatingBarByUser = false;
            } else {
                foldFloatingBarByUser = true;
            }
            unfoldFloatingBarByUser = false;

            if (isFloatingBarChangingHideMode) return;

            await Dispatcher.InvokeAsync(() => {
                isFloatingBarChangingHideMode = true;
                isFloatingBarFolded = true;
                if (currentMode != 0) ImageBlackboard_MouseUp(null, null);
                if (StackPanelCanvasControls.Visibility == Visibility.Visible) {
                    if (foldFloatingBarByUser && inkCanvas.Strokes.Count > 2) {
                        ShowNotification("正在清空墨迹并收纳至侧边栏，可进入批注模式后通过【撤销】功能来恢复原先墨迹。");
                    }
                }
                lastBorderMouseDownObject = sender;
                CursorWithDelIcon_Click(sender, null);
                SidePannelMarginAnimation(-10);
            });

            await Task.Delay(50);

            await Dispatcher.InvokeAsync(() => {
                BottomViewboxPPTSidesControl.Visibility = Visibility.Collapsed;
                LeftSidePanelForPPTNavigation.Visibility = Visibility.Collapsed;
                RightSidePanelForPPTNavigation.Visibility = Visibility.Collapsed;
                ViewboxFloatingBarMarginAnimation(-60);
                HideSubPanels("cursor");
                SidePannelMarginAnimation(-10);
            });
            isFloatingBarChangingHideMode = false;
        }

        private void LeftUnFoldButtonDisplayQuickPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Settings.Appearance.IsShowQuickPanel==true)
            {
                LeftUnFoldButtonQuickPanel.Visibility = Visibility.Visible;
                RightUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            } else
            {
                UnFoldFloatingBar_MouseUp(sender, e);
            }
        }
        private void RightUnFoldButtonDisplayQuickPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Settings.Appearance.IsShowQuickPanel == true)
            {
                RightUnFoldButtonQuickPanel.Visibility = Visibility.Visible;
                LeftUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            } else
            {
                UnFoldFloatingBar_MouseUp(sender, e);
            }
        }

        private void HideQuickPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LeftUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            RightUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
        }

        private async void UnFoldFloatingBar_MouseUp(object sender, MouseButtonEventArgs e) {
            LeftUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            RightUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            if (sender == null || StackPanelPPTControls.Visibility == Visibility.Visible) {
                unfoldFloatingBarByUser = false;
            } else {
                unfoldFloatingBarByUser = true;
            }
            foldFloatingBarByUser = false;

            if (isFloatingBarChangingHideMode) return;

            await Dispatcher.InvokeAsync(() => {
                isFloatingBarChangingHideMode = true;
                isFloatingBarFolded = false;
            });

            await Task.Delay(0);

            await Dispatcher.InvokeAsync(() => {
                if (StackPanelPPTControls.Visibility == Visibility.Visible) {
                    if (Settings.PowerPointSettings.IsShowBottomPPTNavigationPanel) {
                        AnimationsHelper.ShowWithSlideFromBottomAndFade(BottomViewboxPPTSidesControl);
                    }
                    if (Settings.PowerPointSettings.IsShowSidePPTNavigationPanel) {
                        AnimationsHelper.ShowWithScaleFromLeft(LeftSidePanelForPPTNavigation);
                        AnimationsHelper.ShowWithScaleFromRight(RightSidePanelForPPTNavigation);
                    }
                }
                if (BtnPPTSlideShowEnd.Visibility == Visibility.Visible) {
                    ViewboxFloatingBarMarginAnimation(60);
                } else {
                    ViewboxFloatingBarMarginAnimation(100);
                }
                SidePannelMarginAnimation(-50,true);
            });

            isFloatingBarChangingHideMode = false;
        }

        private async void SidePannelMarginAnimation(int MarginFromEdge, bool isNoAnimation = false) // Possible value: -50, -10
        {
            await Dispatcher.InvokeAsync(() => {
                if (MarginFromEdge == -10) LeftSidePanel.Visibility = Visibility.Visible;

                ThicknessAnimation LeftSidePanelmarginAnimation = new ThicknessAnimation {
                    Duration = TimeSpan.FromSeconds(isNoAnimation? 0 : 0.175),
                    From = LeftSidePanel.Margin,
                    To = new Thickness(MarginFromEdge, 0, 0, -150)
                };
                ThicknessAnimation RightSidePanelmarginAnimation = new ThicknessAnimation {
                    Duration = TimeSpan.FromSeconds(isNoAnimation ? 0 : 0.175),
                    From = RightSidePanel.Margin,
                    To = new Thickness(0, 0, MarginFromEdge, -150)
                };

                LeftSidePanel.BeginAnimation(FrameworkElement.MarginProperty, LeftSidePanelmarginAnimation);
                RightSidePanel.BeginAnimation(FrameworkElement.MarginProperty, RightSidePanelmarginAnimation);
            });

            await Task.Delay(600);

            await Dispatcher.InvokeAsync(() => {
                LeftSidePanel.Margin = new Thickness(MarginFromEdge, 0, 0, -150);
                RightSidePanel.Margin = new Thickness(0, 0, MarginFromEdge, -150);

                if (MarginFromEdge == -50) LeftSidePanel.Visibility = Visibility.Collapsed;
            });
            isFloatingBarChangingHideMode = false;
        }
    }
}