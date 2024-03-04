using System.Windows;

namespace Ru.Krdnet.StickyNotes
{
    public static class Screen
    {
        public static void CheckBounds(Window wdw)
        {
            System.Windows.Forms.Screen curscr = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(wdw).Handle);
            double factor = PresentationSource.FromVisual(wdw).CompositionTarget.TransformToDevice.M11;

            if (wdw.Left * factor < curscr.WorkingArea.Left) wdw.Left = curscr.WorkingArea.Left / factor;
            if (wdw.Top * factor < curscr.WorkingArea.Top) wdw.Top = curscr.WorkingArea.Top / factor;
            if (wdw.Left > curscr.WorkingArea.X / factor + curscr.WorkingArea.Width / factor - wdw.Width) wdw.Left = (curscr.WorkingArea.X / factor + curscr.WorkingArea.Width / factor - wdw.Width);
            if (wdw.Top > curscr.WorkingArea.Y / factor + curscr.WorkingArea.Height / factor - wdw.Height) wdw.Top = (curscr.WorkingArea.Y / factor + curscr.WorkingArea.Height / factor - wdw.Height);
        }
    }
}
