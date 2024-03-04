using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Ru.Krdnet.StickyNotes
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MaxHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - 200;
            MaxWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - 200;

            GetMyNote();
            InitializeComponent();
            NoteTextBox.Text = "";
        }

        bool isTextChanged { get; set; } = false;

        bool isSaving { get; set; } = false;

        bool isLoading { get; set; } = false;

        bool AllowClose { get; set; } = false;

        StickyNote Note { get; set; } = new();

        internal async void GetMyNote()
        {
            if (isLoading) return;

            isLoading = true;

            try
            {
                if (File.Exists(App.SaveFile))
                {

                    Note = JsonSerializer.Deserialize<StickyNote>(await File.ReadAllTextAsync(App.SaveFile)) ?? new();

                    NoteTextBox.Text = Note.Text;
                    Width = Note.Width.GetValueOrDefault(Width);
                    Height = Note.Height.GetValueOrDefault(Height);
                    Left = Note.intX.GetValueOrDefault(Left);
                    Top = Note.intY.GetValueOrDefault(Top);

                }
            }
            catch { Note = new(); }
            finally { isLoading = false; }
        }

        private async void SaveMyNote()
        {
            if (isLoading) return;
            if (isSaving) return;

            isSaving = true;

            try
            {
                if (!Directory.Exists(App.SavePath)) Directory.CreateDirectory(App.SavePath);

                Note.Width = Width;
                Note.Height = Height;

                Note.intX = Left;
                Note.intY = Top;

                if (isTextChanged) Note.Text = NoteTextBox.Text;

                Note.LastSaved = DateTime.Now;

                Note.Topmost = Topmost;

                await File.WriteAllTextAsync(App.SaveFile, JsonSerializer.Serialize(Note));
            }
            catch (Exception ex) { ShowErrorWindow(ex); }
            finally { isSaving = false; isTextChanged = false; }
        }

        private void NoteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveMyNote();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            base.OnStateChanged(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Screen.CheckBounds(this);

            if (!Directory.Exists(App.SavePath)) Directory.CreateDirectory(App.SavePath);

            var debugFile = new FileInfo(App.DebugFile);
            if (debugFile.Exists && debugFile.Length > 1048576)
            {
                File.Move(App.DebugFile, Path.Combine(App.SavePath, "debug_" + DateTime.Now.ToString("ddMMyyyyHHmmssffff") + ".log"));

                var debugFiles = new DirectoryInfo(App.SavePath).GetFiles("debug_*.log");
                if (debugFiles.Length > 20)
                {
                    var removeFiles = debugFiles.OrderByDescending(x => x.LastWriteTime).Skip(20);
                    foreach (var file in removeFiles) { file.Delete(); }
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveMyNote();
            e.Cancel = !AllowClose;
            if (!AllowClose)
            {
                Hide();
            }
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            isTextChanged = true;
        }

        private void NoteTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.S) { e.Handled = true; isTextChanged = true; SaveMyNote(); }
            else if (e.IsDown && e.Key == Key.Enter) { SaveMyNote(); }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.CheckBounds(this);
            SaveMyNote();
        }

        private void CloseBtnClick(object sender, RoutedEventArgs e)
        {
            SaveMyNote();
            Hide();
        }

        private void BtnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button b) b.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(100))), HandoffBehavior.SnapshotAndReplace);
        }

        private void BtnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button b) b.BeginAnimation(OpacityProperty, new DoubleAnimation(0.1, new Duration(TimeSpan.FromMilliseconds(100))), HandoffBehavior.SnapshotAndReplace);
        }

        private void NotesTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeAll; try { DragMove(); } catch { }
            Mouse.OverrideCursor = null; Screen.CheckBounds(this);
            SaveMyNote();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            Btn_Close.BeginAnimation(OpacityProperty, new DoubleAnimation(0.1, new Duration(TimeSpan.FromMilliseconds(200))), HandoffBehavior.SnapshotAndReplace);
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            SaveMyNote();
            Btn_Close.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(200))), HandoffBehavior.SnapshotAndReplace);
        }

        public void BringToForeground()
        {
            Screen.CheckBounds(this);
            Show();
            Activate();
            Focus();
        }

        public void ShowErrorWindow(Exception e)
        {
            if (!Directory.Exists(App.SavePath)) Directory.CreateDirectory(App.SavePath);

            File.AppendAllText(App.DebugFile, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss INNER EXEPTION\n"));
            File.AppendAllText(App.DebugFile, e.ToString());
            File.AppendAllText(App.DebugFile, DateTime.Now.ToString("------------------------------------------\n"));

            MessageBox.Show(e.ToString(), "Error found!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void TopMostMenuItemClick(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            TopmostMenuItem.IsChecked = Topmost;
            Note.Topmost = Topmost;
            SaveMyNote();
        }

        private void AboutMenuItemClick(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new();
            aboutWindow.ShowDialog();
        }

        private void ExitMenuItemClick(object sender, RoutedEventArgs e)
        {
            AllowClose = true;
            StickyNotifyIcon.Dispose();
            Close();
        }
    }
}