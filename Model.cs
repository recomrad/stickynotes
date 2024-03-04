using System;

namespace Ru.Krdnet.StickyNotes
{
    public class StickyNote
    {
        public string Text { get; set; } = "";

        public double? intX { get; set; }

        public double? intY { get; set; }

        public double? Width { get; set; }

        public double? Height { get; set; }

        public bool Topmost { get; set; } = false;

        public DateTime LastSaved { get; set; } = DateTime.MinValue;
    }
}
