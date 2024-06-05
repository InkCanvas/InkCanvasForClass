using Ink_Canvas.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace Ink_Canvas {
    public partial class MainWindow : Window {
        private enum CommitReason {
            UserInput,
            CodeInput,
            ShapeDrawing,
            ShapeRecognition,
            ClearingCanvas,
            Manipulation
        }

        private CommitReason _currentCommitType = CommitReason.UserInput;
        private bool IsEraseByPoint => inkCanvas.EditingMode == InkCanvasEditingMode.EraseByPoint;
        private StrokeCollection ReplacedStroke;
        private StrokeCollection AddedStroke;
        private StrokeCollection CuboidStrokeCollection;
        private TimeMachine timeMachine = new TimeMachine();

        private void ApplyHistoryToCanvas(TimeMachineHistory item) {
            _currentCommitType = CommitReason.CodeInput;
            if (item.CommitType == TimeMachineHistoryType.UserInput) {
                if (!item.StrokeHasBeenCleared) {
                    foreach (var strokes in item.CurrentStroke)
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                }
                else {
                    foreach (var strokes in item.CurrentStroke)
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                }
            }
            else if (item.CommitType == TimeMachineHistoryType.ShapeRecognition) {
                if (item.StrokeHasBeenCleared) {
                    foreach (var strokes in item.CurrentStroke)
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);

                    foreach (var strokes in item.ReplacedStroke)
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);
                }
                else {
                    foreach (var strokes in item.CurrentStroke)
                        if (!inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Add(strokes);

                    foreach (var strokes in item.ReplacedStroke)
                        if (inkCanvas.Strokes.Contains(strokes))
                            inkCanvas.Strokes.Remove(strokes);
                }
            }
            else if (item.CommitType == TimeMachineHistoryType.Manipulation) {
                item.ManipulationHistory.Invert();
                foreach (var strokes in item.CurrentStroke) strokes.Transform(item.ManipulationHistory, false);
            }
            else if (item.CommitType == TimeMachineHistoryType.Clear) {
                if (!item.StrokeHasBeenCleared) {
                    if (item.CurrentStroke != null)
                        foreach (var currentStroke in item.CurrentStroke)
                            if (!inkCanvas.Strokes.Contains(currentStroke))
                                inkCanvas.Strokes.Add(currentStroke);

                    if (item.ReplacedStroke != null)
                        foreach (var replacedStroke in item.ReplacedStroke)
                            if (inkCanvas.Strokes.Contains(replacedStroke))
                                inkCanvas.Strokes.Remove(replacedStroke);
                }
                else {
                    if (item.ReplacedStroke != null)
                        foreach (var replacedStroke in item.ReplacedStroke)
                            if (!inkCanvas.Strokes.Contains(replacedStroke))
                                inkCanvas.Strokes.Add(replacedStroke);

                    if (item.CurrentStroke != null)
                        foreach (var currentStroke in item.CurrentStroke)
                            if (inkCanvas.Strokes.Contains(currentStroke))
                                inkCanvas.Strokes.Remove(currentStroke);
                }
            }

            _currentCommitType = CommitReason.UserInput;
        }

        private void TimeMachine_OnUndoStateChanged(bool status) {
            var result = status ? Visibility.Visible : Visibility.Collapsed;
            BtnUndo.Visibility = result;
            BtnUndo.IsEnabled = status;
        }

        private void TimeMachine_OnRedoStateChanged(bool status) {
            var result = status ? Visibility.Visible : Visibility.Collapsed;
            BtnRedo.Visibility = result;
            BtnRedo.IsEnabled = status;
        }

        private void StrokesOnStrokesChanged(object sender, StrokeCollectionChangedEventArgs e) {
            if (!isHidingSubPanelsWhenInking) {
                isHidingSubPanelsWhenInking = true;
                HideSubPanels(); // 书写时自动隐藏二级菜单
            }

            if (_currentCommitType == CommitReason.CodeInput || _currentCommitType == CommitReason.ShapeDrawing) return;

            if ((e.Added.Count != 0 || e.Removed.Count != 0) && IsEraseByPoint) {
                if (AddedStroke == null) AddedStroke = new StrokeCollection();
                if (ReplacedStroke == null) ReplacedStroke = new StrokeCollection();
                AddedStroke.Add(e.Added);
                ReplacedStroke.Add(e.Removed);
                return;
            }

            if (e.Added.Count != 0) {
                if (_currentCommitType == CommitReason.ShapeRecognition) {
                    timeMachine.CommitStrokeShapeHistory(ReplacedStroke, e.Added);
                    ReplacedStroke = null;
                    return;
                }
                else {
                    timeMachine.CommitStrokeUserInputHistory(e.Added);
                    return;
                }
            }

            if (e.Removed.Count != 0) {
                if (_currentCommitType == CommitReason.ShapeRecognition) {
                    ReplacedStroke = e.Removed;
                    return;
                }
                else if (!IsEraseByPoint || _currentCommitType == CommitReason.ClearingCanvas) {
                    timeMachine.CommitStrokeEraseHistory(e.Removed);
                    return;
                }
            }
        }
    }
}