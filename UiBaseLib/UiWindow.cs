using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UiBaseLib
{
    public abstract class UiWindow
    {
        /// <summary>
        /// The screen-coordinates of the left-most edge of this window.
        /// </summary>
        public virtual double Left { get; set; }

        /// <summary>
        /// The screen-coordinates of the top-most edge of this window.
        /// </summary>
        public virtual double Top { get; set; }

        /// <summary>
        /// The width, in terms of screen-coordinates, of this window.
        /// </summary>
        public virtual double Width { get; set; }

        /// <summary>
        /// The height, in terms of screen-coordinates, of this window.
        /// </summary>
        public virtual double Height { get; set; }

        /// <summary>
        /// The UiWindow that is the parent of this one.
        /// </summary>
        public virtual UiWindow Parent { get; }

        public bool IsWithin(UiWindow window, DisplayScreen screen)
        {
            bool answer = false;

            if (window.Left > screen.Left && window.Left + window.Width < screen.Right)
            {
                if (window.Top > screen.Top && window.Top + window.Height < screen.Bottom)
                {
                    answer = true;
                }
            }
            return answer;
        }

        public bool IsWithin( DisplayScreen screen)
        {
            bool answer = false;

            if (this.Left > screen.Left && this.Left + this.Width < screen.Right)
            {
                if (this.Top > screen.Top && this.Top + this.Height < screen.Bottom)
                {
                    answer = true;
                }
            }
            return answer;
        }

    }
}
