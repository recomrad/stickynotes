using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Ru.Krdnet.StickyNotes
{
    public class ShowWindowCommand : ICommand
    {
        public void Execute(object? parameter)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() => ((MainWindow)Application.Current.MainWindow)?.BringToForeground()));
            }
            catch (Exception ex)
            {
                if (!Directory.Exists(App.SavePath)) Directory.CreateDirectory(App.SavePath);

                File.AppendAllText(App.DebugFile, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss COMMAND EXEPTION\n"));
                File.AppendAllText(App.DebugFile, ex.ToString());
                File.AppendAllText(App.DebugFile, DateTime.Now.ToString("------------------------------------------\n"));
            }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}