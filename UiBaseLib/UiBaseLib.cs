using System;
using System.Windows;


namespace UiBaseLib
{
    /// <summary>
    /// This indicates WHERE to position a Window, relative to another Window (presumably it's parent or owner Window).
    /// The default value is ToRightOfParent.
    /// </summary>
    public enum AlignmentType
    {
        /// <summary>
        /// Position it to the right of the parent-element
        /// </summary>
        ToRightOfParent,
        /// <summary>
        /// Position it to the left of the parent-element
        /// </summary>
        ToLeftOfParent,
        /// <summary>
        /// Position it below the parent-element
        /// </summary>
        UnderParent,
        /// <summary>
        /// Position it above the parent-element
        /// </summary>
        AboveParent
    }

    /// <summary>
    /// This enum-type denotes the four possible positions to which the Windows Task-Bar can be placed.
    /// </summary>
    public enum TaskBarLocation
    {
        /// <summary>
        /// This value signifies that it is positioned along the top edge of the display-screen.
        /// </summary>
        Top,
        /// <summary>
        /// This value signifies that it is positioned along the bottom edge of the display-screen.
        /// </summary>
        Bottom,
        /// <summary>
        /// This value signifies that it is positioned along the left edge of the display-screen.
        /// </summary>
        Left,
        /// <summary>
        /// This value signifies that it is positioned along the right edge of the display-screen.
        /// </summary>
        Right
    }

    /// <summary>
    /// This library serves to provide some platform-neutral methods for position the UX windows,
    /// for which implementations are provided for WPF and Windows Forms.
    /// </summary>
    public class UiBaseLib
    {
        #region AlignToRight
        /// <summary>
        /// This is a helper method for AlignToParent; it aligns the given Window to the right of the parent-Window, if possible.
        /// </summary>
        /// <param name="dialog">the Window to align</param>
        /// <param name="isMultipleScreensHorizontally">denotes whether there are more than one display-screen to consider</param>
        /// <param name="primaryScreenWidth">the width of the primary display-screen</param>
        /// <param name="virtualScreenRight">the screen-coordinates the right-edge of the "virtual" screen</param>
        /// <param name="virtualScreenBottom">the screen-coordinates the bottom-edge of the "virtual" screen</param>
        /// <param name="secondDisplayScreen">this is the secondary display-screen, or null if there is none</param>
        /// <param name="taskBarLocation">the position of the Windows Task-Bar</param>
        /// <param name="taskBarHeight">the height (in pixels) of the Windows Task-Bar that is currently displayed on the screen</param>
        /// <returns>true if successful, false if there wasn't sufficient space on the display</returns>
        public static bool AlignToRight( UiWindow dialog,
                                         bool isMultipleScreensHorizontally,
                                         int primaryScreenWidth,
                                         int virtualScreenRight,
                                         int virtualScreenBottom,
                                         DisplayScreen secondDisplayScreen,
                                         TaskBarLocation taskBarLocation,
                                         double taskBarHeight )
        {
            UiWindow parent = dialog.Parent;
            double parentRight = parent.Left + parent.Width;
            const int separation = 2;
            int screenRight = virtualScreenRight;

            // This following, which brings in the need for Windows.Forms,
            // is for the case where a 2nd monitor is not as wide as the pri monitor.
            if (!isMultipleScreensHorizontally)
            {
                if (secondDisplayScreen != null)
                {
                    int width2 = secondDisplayScreen.Width;
                    int top2 = secondDisplayScreen.Top;
                    int bottom2 = secondDisplayScreen.Bottom;

                    if (IsInRange(parent.Top, top2, bottom2 ))
                    {
                        // Evidently, this Window is on Screen 2, so use it's area.
                        screenRight = width2;
                    }
                }
            }

            // I'll position myself along the right side if there's sufficient space.
            if (parentRight + dialog.Width < screenRight)
            {
                // Try to avoid falling over top of the break between two screens.
                if (isMultipleScreensHorizontally)
                {
                    // See if the parent window is entirely within the leftmost screen.
                    if (parentRight < primaryScreenWidth)
                    {
                        //CBL Just a temp hack, since my Window was set to 416 but showing on-screen at 400.
// Really, this should simply set dialogWidth = dialog.Width
                        double dialogWidth = dialog.Width - 17;

                        // but the dialog's width would make it go beyond the primary (left-most) screen..
                        if (parentRight + dialogWidth > primaryScreenWidth)
                        {
                            dialog.Left = primaryScreenWidth;
                            int bottom;
                            if (taskBarLocation == TaskBarLocation.Bottom)
                            {
                                bottom = secondDisplayScreen.Bottom - (int)taskBarHeight;
                            }
                            else
                            {
                                bottom = secondDisplayScreen.Bottom;
                            }
                            //CBL Here, virtualScreenBottom refers to the left display, but we are placing it on the right display !
                            dialog.Top = Math.Min( parent.Top, bottom - dialog.Height );
                            //dialog.Top = Math.Min( parent.Top, virtualScreenBottom - dialog.Height );

                            //CBL Temp code:
                            if (dialog.Left + dialog.Width > primaryScreenWidth)
                            {
                                if (secondDisplayScreen != null)
                                {
                                    if (dialog.Top < secondDisplayScreen.Top)
                                    {
                                        // Evidently, this Window is on Screen 2, so ensure we are within it's area.
                                        dialog.Top = secondDisplayScreen.Top;
                                    }
                                }
                            }

                            return true;
                        }
                    }
                }

                // Do not add the separation if we are already abutting the right edge.
                //CBL Need to get the ACTUAL size somehow!
                //dialog.Left = parentRight ;
                dialog.Left = parentRight + separation;

                // Adjust my vertical position so that my feet don't jut off the bottom edge.
                dialog.Top = Math.Min( parent.Top, virtualScreenBottom - dialog.Height );


                //CBL Temp code:
                if (dialog.Left + dialog.Width > primaryScreenWidth)
                {
                    if (secondDisplayScreen != null)
                    {
                        int top2 = secondDisplayScreen.Top;

                        if (dialog.Top < top2)
                        {
                            // Evidently, this Window is on Screen 2, so ensure we are within it's area.
                            dialog.Top = top2;
                        }
                    }
                }

                return true;
            }
            else
            {
                //CBL Temp code:
                if (dialog.Left + dialog.Width > primaryScreenWidth)
                {
                    if (secondDisplayScreen != null)
                    {
                        int top2 = secondDisplayScreen.Top;

                        if (dialog.Top < top2)
                        {
                            // Evidently, this Window is on Screen 2, so ensure we are within it's area.
                            dialog.Top = top2;
                        }
                    }
                }

                return false;
            }
        }
        #endregion AlignToRight

        #region IsInRange
        /// <summary>
        /// Return true if this value (x) is within the range denoted by the two parameters X1,x2 regardless of their order or whether they're negative.
        /// </summary>
        /// <param name="x">this numeric value to compare against the range limits</param>
        /// <param name="x1">one range limit (may be greater or less than the other limit)</param>
        /// <param name="x2">the other range limit</param>
        /// <returns></returns>
        private static bool IsInRange(double x, double x1, double x2)
        {
            double lowerLimit = Math.Min(x1, x2);
            double upperLimit = Math.Max(x1, x2);
            return (x >= lowerLimit && x <= upperLimit);
        }
        #endregion
    }
}
