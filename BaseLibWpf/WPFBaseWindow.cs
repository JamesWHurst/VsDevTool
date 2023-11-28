using Hurst.LogNut.Util;
using System.Windows;
using System.Windows.Forms;
using UiBaseLib;


namespace Hurst.BaseLibWpf
{
    public class WPFBaseWindow : UiWindow
    {
        public WPFBaseWindow( Window wpfWindow )
        {
            _window = wpfWindow;
        }

        public override double Top
        {
            get { return _window.Top; }
            set { _window.Top = value; }
        }

        public override double Left
        {
            get { return _window.Left; }
            set { _window.Left = value; }
        }

        public override double Height
        {
            get { return _window.Height; }
            set { _window.Height = value; }
        }

        public override double Width
        {
            get { return _window.Width; }
            set { _window.Width = value; }
        }

        /// <summary>
        /// The UiWindow that is the parent of this one.
        /// </summary>
        public override UiWindow Parent
        {
            get
            {
                if (_parentWindow == null)
                {
                    _parentWindow = new WPFBaseWindow( _window.Owner );
                }
                return _parentWindow;
            }
        }

        private UiWindow _parentWindow;
        private readonly Window _window;
    }


    //public class WPFDisplayScreen : IDisplayScreen
    //{
    //    public WPFDisplayScreen( Screen screen )
    //    {
    //        _screen = screen;
    //    }

    //    public int Bottom
    //    {
    //        get { return _screen.Bounds.Bottom; }
    //    }

    //    public int Left
    //    {
    //        get { return _screen.Bounds.Left; }
    //    }

    //    public int Right
    //    {
    //        get { return _screen.Bounds.Right; }
    //    }

    //    public int Top
    //    {
    //        get { return _screen.Bounds.Top; }
    //    }

    //    public int Width
    //    {
    //        get { return _screen.Bounds.Width; }
    //    }

    //    private System.Windows.Forms.Screen _screen;
    //}

}
