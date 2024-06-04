using Ink_Canvas.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace Ink_Canvas
{

    public class TimeViewModel : INotifyPropertyChanged
    {
        private string _nowTime;
        private string _nowDate;

        public string nowTime
        {
            get { return _nowTime; }
            set
            {
                if (_nowTime != value)
                {
                    _nowTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public string nowDate
        {
            get { return _nowDate; }
            set
            {
                if (_nowDate != value)
                {
                    _nowDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainWindow : Window
    {
        Timer timerCheckPPT = new Timer();
        Timer timerKillProcess = new Timer();
        Timer timerCheckAutoFold = new Timer();
        string AvailableLatestVersion = null;
        Timer timerCheckAutoUpdateWithSilence = new Timer();
        bool isHidingSubPanelsWhenInking = false; // 避免书写时触发二次关闭二级菜单导致动画不连续

        Timer timerDisplayTime = new Timer();
        Timer timerDisplayDate = new Timer();

        private TimeViewModel nowTimeVM = new TimeViewModel();

        private void InitTimers()
        {
            timerCheckPPT.Elapsed += TimerCheckPPT_Elapsed;
            timerCheckPPT.Interval = 500;
            timerKillProcess.Elapsed += TimerKillProcess_Elapsed;
            timerKillProcess.Interval = 5000;
            timerCheckAutoFold.Elapsed += timerCheckAutoFold_Elapsed;
            timerCheckAutoFold.Interval = 1500;
            timerCheckAutoUpdateWithSilence.Elapsed += timerCheckAutoUpdateWithSilence_Elapsed;
            timerCheckAutoUpdateWithSilence.Interval = 1000 * 60 * 10;
            WaterMarkTime.DataContext = nowTimeVM;
            WaterMarkDate.DataContext = nowTimeVM;
            timerDisplayTime.Elapsed += TimerDisplayTime_Elapsed;
            timerDisplayTime.Interval = 1000;
            timerDisplayTime.Start();
            timerDisplayDate.Elapsed += TimerDisplayDate_Elapsed;
            timerDisplayDate.Interval = 1000 * 60 * 60 * 1;
            timerDisplayDate.Start();
            nowTimeVM.nowDate = DateTime.Now.ToShortDateString().ToString();
            nowTimeVM.nowTime = DateTime.Now.ToShortTimeString().ToString();
        }

        private void TimerDisplayTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            nowTimeVM.nowTime = DateTime.Now.ToShortTimeString().ToString();
        }

        private void TimerDisplayDate_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            nowTimeVM.nowDate = DateTime.Now.ToShortDateString().ToString();
        }

        private void TimerKillProcess_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // 希沃相关： easinote swenserver RemoteProcess EasiNote.MediaHttpService smartnote.cloud EasiUpdate smartnote EasiUpdate3 EasiUpdate3Protect SeewoP2P CefSharp.BrowserSubprocess SeewoUploadService
                string arg = "/F";
                if (Settings.Automation.IsAutoKillPptService)
                {
                    Process[] processes = Process.GetProcessesByName("PPTService");
                    if (processes.Length > 0)
                    {
                        arg += " /IM PPTService.exe";
                    }
                    processes = Process.GetProcessesByName("SeewoIwbAssistant");
                    if (processes.Length > 0)
                    {
                        arg += " /IM SeewoIwbAssistant.exe" + " /IM Sia.Guard.exe";
                    }
                }
                if (Settings.Automation.IsAutoKillEasiNote)
                {
                    Process[] processes = Process.GetProcessesByName("EasiNote");
                    if (processes.Length > 0)
                    {
                        arg += " /IM EasiNote.exe";
                    }
                }
                if (Settings.Automation.IsAutoKillHiteAnnotation)
                {
                    Process[] processes = Process.GetProcessesByName("HiteAnnotation");
                    if (processes.Length > 0)
                    {
                        arg += " /IM HiteAnnotation.exe";
                    }
                }
                if (arg != "/F")
                {
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo("taskkill", arg);
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.Start();

                    if (arg.Contains("EasiNote"))
                    {
                        BtnSwitch_Click(BtnSwitch, null);
                        MessageBox.Show("“希沃白板 5”已自动关闭");
                    }
                    if (arg.Contains("HiteAnnotation"))
                    {
                        BtnSwitch_Click(BtnSwitch, null);
                        MessageBox.Show("“鸿合屏幕书写”已自动关闭");
                    }
                }
            }
            catch { }
        }


        bool foldFloatingBarByUser = false, // 保持收纳操作不受自动收纳的控制
            unfoldFloatingBarByUser = false; // 允许用户在希沃软件内进行展开操作

        private void timerCheckAutoFold_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (isFloatingBarChangingHideMode) return;
            try
            {
                string windowProcessName = ForegroundWindowInfo.ProcessName();
                string windowTitle = ForegroundWindowInfo.WindowTitle();
                //LogHelper.WriteLogToFile("windowTitle | " + windowTitle + " | windowProcessName | " + windowProcessName);

                if (Settings.Automation.IsAutoFoldInEasiNote && windowProcessName == "EasiNote" // 希沃白板
                    && (!(windowTitle.Length == 0 && ForegroundWindowInfo.WindowRect().Height < 500) || !Settings.Automation.IsAutoFoldInEasiNoteIgnoreDesktopAnno)
                    || Settings.Automation.IsAutoFoldInEasiCamera && windowProcessName == "EasiCamera" // 希沃视频展台
                    || Settings.Automation.IsAutoFoldInEasiNote3C && windowProcessName == "EasiNote" // 希沃轻白板3C
                    || Settings.Automation.IsAutoFoldInEasiNote5C && windowProcessName == "EasiNote5C" // 希沃轻白板5C
                    || Settings.Automation.IsAutoFoldInSeewoPincoTeacher && (windowProcessName == "BoardService" || windowProcessName == "seewoPincoTeacher") // 希沃品课
                    || Settings.Automation.IsAutoFoldInHiteCamera && windowProcessName == "HiteCamera" // 鸿合视频展台
                    || Settings.Automation.IsAutoFoldInHiteTouchPro && windowProcessName == "HiteTouchPro" // 鸿合白板
                    || Settings.Automation.IsAutoFoldInWxBoardMain && windowProcessName == "WxBoardMain" // 文香白板
                    || Settings.Automation.IsAutoFoldInMSWhiteboard && (windowProcessName == "MicrosoftWhiteboard" || windowProcessName == "msedgewebview2") // 微软白板
                    || Settings.Automation.IsAutoFoldInOldZyBoard && // 中原旧白板
                    (WinTabWindowsChecker.IsWindowExisted("WhiteBoard - DrawingWindow")
                    || WinTabWindowsChecker.IsWindowExisted("InstantAnnotationWindow")))
                {
                    if (!unfoldFloatingBarByUser && !isFloatingBarFolded)
                    {
                        FoldFloatingBar_MouseUp(null, null);
                    }
                }
                else if (WinTabWindowsChecker.IsWindowExisted("幻灯片放映", false))
                { // 处于幻灯片放映状态
                    if (!Settings.Automation.IsAutoFoldInPPTSlideShow && isFloatingBarFolded && !foldFloatingBarByUser)
                    {
                        UnFoldFloatingBar_MouseUp(new Object(), null);
                    }
                }
                else
                {
                    if (isFloatingBarFolded && !foldFloatingBarByUser)
                    {
                        UnFoldFloatingBar_MouseUp(new Object(), null);
                    }
                    unfoldFloatingBarByUser = false;
                }
            }
            catch { }
        }

        private void timerCheckAutoUpdateWithSilence_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                try
                {
                    if ((!Topmost) || (inkCanvas.Strokes.Count > 0)) return;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLogToFile(ex.ToString(), LogHelper.LogType.Error);
                }
            });
            try
            {
                if (AutoUpdateWithSilenceTimeComboBox.CheckIsInSilencePeriod(Settings.Startup.AutoUpdateWithSilenceStartTime, Settings.Startup.AutoUpdateWithSilenceEndTime))
                {
                    AutoUpdateHelper.InstallNewVersionApp(AvailableLatestVersion, true);
                    timerCheckAutoUpdateWithSilence.Stop();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile(ex.ToString(), LogHelper.LogType.Error);
            }
        }
    }
}
