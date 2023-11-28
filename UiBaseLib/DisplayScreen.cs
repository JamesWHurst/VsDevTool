using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UiBaseLib
{
    public class DisplayScreen
    {
        public DisplayScreen(System.Windows.Forms.Screen screen)
        {
            _screen = screen;
        }

        public int Bottom
        {
            get { return _screen.Bounds.Bottom; }
        }

        public int Left
        {
            get { return _screen.Bounds.Left; }
        }

        public int Right
        {
            get { return _screen.Bounds.Right; }
        }

        public int Top
        {
            get { return _screen.Bounds.Top; }
        }

        public int Width
        {
            get { return _screen.Bounds.Width; }
        }

        private System.Windows.Forms.Screen _screen;
    }
}
