using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace Ru.Krdnet.StickyNotes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constants and Fields

        /// <summary>The event mutex name.</summary>
#if DEBUG
        private const string UniqueEventName = "{0C7C7FDB-D1B1-45C4-96AD-CC0D1E85F6EE}";
        private const string UniqueMutexName = "{75777037-E20D-4080-BA39-D84191CB1C2B}";
#else
   private const string UniqueEventName = "{A11057A0-A6C0-4F3B-B9EC-001A0A66B00B}";
   private const string UniqueMutexName = "{6BF7EED0-ECA2-487A-ADAC-B3A8D127DE8D}";
#endif

        /// <summary>The unique mutex name.</summary>

        /// <summary>The event wait handle.</summary>
        private EventWaitHandle _eventWaitHandle;

        /// <summary>The mutex.</summary>
        private Mutex _mutex;

        #endregion

        public static readonly string SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recomrad");
        public static readonly string DebugFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recomrad", "debug.log");
        public static readonly string SaveFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recomrad", "StickyNote.save");

        public static string backupText { get; set; } = string.Empty;

        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            this._mutex = new Mutex(true, UniqueMutexName, out bool isOwned);
            this._eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            // So, R# would not give a warning that this variable is not used.
            GC.KeepAlive(this._mutex);

            if (isOwned)
            {
                // Spawn a thread which will be waiting for our event
                var thread = new Thread(
                    () =>
                    {
                        while (this._eventWaitHandle.WaitOne())
                        {
                            try
                            {
                                Current.Dispatcher.BeginInvoke((Action)(() => ((MainWindow)Current.MainWindow)?.BringToForeground()));
                            }
                            catch (Exception ex) { ShowErrorWindow(ex); }
                        }
                    })
                {

                    // It is important mark it as background otherwise it will prevent app from exiting.
                    IsBackground = true
                };

                thread.Start();
                return;
            }

            // Notify other instance so it could bring itself to foreground.
            try { this._eventWaitHandle.Set(); } catch (Exception ex) { ShowErrorWindow(ex); }

            // Terminate this instance.
            this.Shutdown();
        }

        private void ShowErrorWindow(Exception e)
        {
            try
            {
                Current.Dispatcher.BeginInvoke((Action)(() => ((MainWindow)Current.MainWindow)?.ShowErrorWindow(e)));
            }
            catch (Exception ex)
            {
                if (!Directory.Exists(App.SavePath)) Directory.CreateDirectory(App.SavePath);

                File.AppendAllText(App.DebugFile, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss INNER EXEPTION\n"));
                File.AppendAllText(App.DebugFile, e.ToString());
                File.AppendAllText(App.DebugFile, DateTime.Now.ToString("------------------------------------------\n"));

                File.AppendAllText(App.DebugFile, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss OUTER EXEPTION\n"));
                File.AppendAllText(App.DebugFile, ex.ToString());
                File.AppendAllText(App.DebugFile, DateTime.Now.ToString("------------------------------------------\n"));
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            File.WriteAllText(Path.Combine(SavePath, "unhandled.backup"), backupText);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unhandled.backup"), backupText);
            Exception e = (Exception)args.ExceptionObject;
            ShowErrorWindow(e);
        }
    }
}
