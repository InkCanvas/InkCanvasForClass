using Ink_Canvas.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

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

        private async void LeftUnFoldButtonDisplayQuickPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Settings.Appearance.IsShowQuickPanel==true)
            {
                HideRightQuickPanel();
                LeftUnFoldButtonQuickPanel.Visibility = Visibility.Visible;
                await Dispatcher.InvokeAsync(() =>
                {
                    ThicknessAnimation marginAnimation = new ThicknessAnimation
                    {
                        Duration = TimeSpan.FromSeconds(0.1),
                        From = new Thickness(-50, 0, 0, -150),
                        To = new Thickness(-1, 0, 0, -150)
                    };
                    LeftUnFoldButtonQuickPanel.BeginAnimation(FrameworkElement.MarginProperty, marginAnimation);
                });
                await Task.Delay(100);

                await Dispatcher.InvokeAsync(() => {
                    LeftUnFoldButtonQuickPanel.Margin = new Thickness(-1, 0, 0, -150);
                });
            } else
            {
                UnFoldFloatingBar_MouseUp(sender, e);
            }
        }
        private async void RightUnFoldButtonDisplayQuickPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Settings.Appearance.IsShowQuickPanel == true)
            {
                HideLeftQuickPanel();
                RightUnFoldButtonQuickPanel.Visibility = Visibility.Visible;
                await Dispatcher.InvokeAsync(() =>
                {
                    ThicknessAnimation marginAnimation = new ThicknessAnimation
                    {
                        Duration = TimeSpan.FromSeconds(0.1),
                        From = new Thickness(0, 0, -50, -150),
                        To = new Thickness(0, 0, -1, -150)
                    };
                    RightUnFoldButtonQuickPanel.BeginAnimation(FrameworkElement.MarginProperty, marginAnimation);
                });
                await Task.Delay(100);

                await Dispatcher.InvokeAsync(() => {
                    RightUnFoldButtonQuickPanel.Margin = new Thickness(0, 0, -1, -150);
                });
            } else
            {
                UnFoldFloatingBar_MouseUp(sender, e);
            }
        }

        private async void HideLeftQuickPanel()
        {
            if (LeftUnFoldButtonQuickPanel.Visibility == Visibility.Visible)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    ThicknessAnimation marginAnimation = new ThicknessAnimation
                    {
                        Duration = TimeSpan.FromSeconds(0.1),
                        From = new Thickness(-1, 0, 0, -150),
                        To = new Thickness(-50, 0, 0, -150)
                    };
                    LeftUnFoldButtonQuickPanel.BeginAnimation(FrameworkElement.MarginProperty, marginAnimation);
                });
                await Task.Delay(100);

                await Dispatcher.InvokeAsync(() => {
                    LeftUnFoldButtonQuickPanel.Margin = new Thickness(0, 0, -50, -150);
                    LeftUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
                });
            }
        }

        private async void HideRightQuickPanel()
        {
            if (RightUnFoldButtonQuickPanel.Visibility == Visibility.Visible)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    ThicknessAnimation marginAnimation = new ThicknessAnimation
                    {
                        Duration = TimeSpan.FromSeconds(0.1),
                        From = new Thickness(0, 0, -1, -150),
                        To = new Thickness(0, 0, -50, -150)
                    };
                    RightUnFoldButtonQuickPanel.BeginAnimation(FrameworkElement.MarginProperty, marginAnimation);
                });
                await Task.Delay(100);

                await Dispatcher.InvokeAsync(() => {
                    RightUnFoldButtonQuickPanel.Margin = new Thickness(0, 0, -50, -150);
                    RightUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void HideQuickPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HideLeftQuickPanel();
            HideRightQuickPanel();
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
                    ViewboxFloatingBarMarginAnimation(100, true);
                }
                SidePannelMarginAnimation(-50, !unfoldFloatingBarByUser);
            });

            isFloatingBarChangingHideMode = false;
        }

        private async void SidePannelMarginAnimation(int MarginFromEdge, bool isNoAnimation = false) // Possible value: -50, -10
        {
            await Dispatcher.InvokeAsync(() => {
                if (MarginFromEdge == -10) LeftSidePanel.Visibility = Visibility.Visible;

                ThicknessAnimation LeftSidePanelmarginAnimation = new ThicknessAnimation {
                    Duration = isNoAnimation==true? TimeSpan.FromSeconds(0) : TimeSpan.FromSeconds(0.175),
                    From = LeftSidePanel.Margin,
                    To = new Thickness(MarginFromEdge, 0, 0, -150)
                };
                ThicknessAnimation RightSidePanelmarginAnimation = new ThicknessAnimation {
                    Duration = isNoAnimation == true ? TimeSpan.FromSeconds(0) : TimeSpan.FromSeconds(0.175),
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