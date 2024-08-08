using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ink_Canvas {
    public class IccInkCanvas : InkCanvas {
        public IccInkCanvas() {
            // 通过反射移除InkCanvas自带的默认 Delete按键事件
            var commandBindingsField =
                typeof(CommandManager).GetField("_classCommandBindings", BindingFlags.NonPublic | BindingFlags.Static);
            var bnds = commandBindingsField.GetValue(null) as HybridDictionary;
            var inkCanvasBindings = bnds[typeof(InkCanvas)] as CommandBindingCollection;
            var enumerator = inkCanvasBindings.GetEnumerator();
            while (enumerator.MoveNext()) {
                var item = (CommandBinding)enumerator.Current;
                if (item.Command == ApplicationCommands.Delete) {
                    var executedField =
                        typeof(CommandBinding).GetField("Executed", BindingFlags.NonPublic | BindingFlags.Instance);
                    var canExecuteField =
                        typeof(CommandBinding).GetField("CanExecute", BindingFlags.NonPublic | BindingFlags.Instance);
                    executedField.SetValue(item, new ExecutedRoutedEventHandler((sender, args) => { }));
                    canExecuteField.SetValue(item, new CanExecuteRoutedEventHandler((sender, args) => { }));
                }
            }

            // 为IccInkCanvas注册自定义的 Delete按键Command并Invoke OnDeleteCommandFired。
            CommandManager.RegisterClassCommandBinding(typeof(IccInkCanvas), new CommandBinding(ApplicationCommands.Delete,
                (sender, args) => {
                    DeleteKeyCommandFired?.Invoke(this, new RoutedEventArgs());
                }, (sender, args) => {
                    args.CanExecute = GetSelectedStrokes().Count != 0;
                }));
        }

        public event EventHandler<RoutedEventArgs> DeleteKeyCommandFired;
    }
}