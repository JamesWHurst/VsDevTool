using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using Hurst.LogNut.Util;
using UiBaseLib;


namespace Hurst.BaseLibWpf
{
    [Serializable]
    public class WpfUserSettings : UserSettings
    {
        #region IsToSaveWindowSize
        /// <summary>
        /// Get or set whether to persist the dimensions of the window,
        /// which is distinct from the location.
        /// The default is false.
        /// </summary>
        public bool IsToSaveWindowSize
        {
            get { return _isToSaveSize; }
            set
            {
                //Debug.WriteLine($"IsToSaveWindowSize set from {_isToSaveSize} to {value}.");
                _isToSaveSize = value;
            }
        }
        #endregion

        #region PositionOfMainWindow
        /// <summary>
        /// Get or set the <see cref="WindowPosition"/> that denotes the location and size of the main window.
        /// Upon get, if this has not already been set then it creates a new <see cref="WindowPosition"/> object.
        /// </summary>
        public WindowPosition PositionOfMainWindow
        {
            get
            {
                if (_positionOfMainWindow == null)
                {
                    _positionOfMainWindow = new WindowPosition(_isToSaveLocation, _isToSaveSize);
                }
                return _positionOfMainWindow;
            }
            set { _positionOfMainWindow = value; }
        }
        /// <summary>
        /// This is the <see cref="WindowPosition"/> that denotes the location the main window.
        /// </summary>
        protected WindowPosition _positionOfMainWindow;
        #endregion

        #region LoadPosition
        /// <summary>
        /// Set the location and/or size of the given WPF-window from the values
        /// contained within this <see cref="WindowPosition"/>.
        /// </summary>
        /// <param name="windowPosition">the saved position to load the window's new position from</param>
        /// <param name="window">the WPF-window to set the position of</param>
        public void LoadPosition( WindowPosition windowPosition, Window window )
        {
            if (windowPosition == null)
            {
                throw new ArgumentNullException("windowPosition");
            }
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            //CBL
#if !NETFX_CORE
            if (windowPosition.WindowState == WindowState.Normal)
            {
                // Deal with multiple monitors.
                if (windowPosition.IsSavingSize && windowPosition.SavedSize.Width > 0 && windowPosition.SavedSize.Height > 0)
                {
                    window.Width = windowPosition.SavedSize.Width;
                    window.Height = windowPosition.SavedSize.Height;
                }
                if (windowPosition.IsSavingLocation && (Math.Abs(windowPosition.SavedLocation.X) > Double.Epsilon || Math.Abs(windowPosition.SavedLocation.Y) > Double.Epsilon))
                {
                    window.Left = windowPosition.SavedLocation.X;
                    window.Top = windowPosition.SavedLocation.Y;

                    // Apply a correction if the previous settings had it located on a monitor that no longer is available.
                    //
                    double virtualScreenTop = System.Windows.SystemParameters.VirtualScreenTop;
                    double virtualScreenWidth = System.Windows.SystemParameters.VirtualScreenWidth;
                    double virtualScreenHeight = System.Windows.SystemParameters.VirtualScreenHeight;
                    double virtualScreenLeft = System.Windows.SystemParameters.VirtualScreenLeft;
                    double virtualScreenRight = virtualScreenLeft + virtualScreenWidth;
                    double virtualScreenBottom = virtualScreenTop + virtualScreenHeight;
                    double myWidth = window.Width;
                    double myBottom = window.Top + window.Height;

                    // If the 2nd monitor was to the right, and is now not..
                    if (window.Left > (virtualScreenRight - myWidth))
                    {
                        window.Left = virtualScreenRight - myWidth;
                    }
                    // or if it was to the left..
                    else if (window.Left < virtualScreenLeft)
                    {
                        window.Left = virtualScreenLeft;
                    }
                    // or if there was a vertical change..
                    if (myBottom > virtualScreenBottom)
                    {
                        window.Top = virtualScreenBottom - window.Height;
                    }
                    else if (window.Top < virtualScreenTop)
                    {
                        window.Top = virtualScreenTop;
                    }

                    // I had one case wherein my 2nd monitor, to my right, was of lesser resolution, location.y = 1075, but the monitor height was 1080.
                    // CBL  I could not see the dam thing.
                    if (Screen.AllScreens.Length > 1)
                    {
                        // We have more than one monitor.
                        // Note: This requires that we pull in assembly System.Drawing.
                        Screen screen1 = Screen.AllScreens[0];
                        Screen screen2 = Screen.AllScreens[1];
                        System.Drawing.Point point = new System.Drawing.Point((int)windowPosition.SavedLocation.X, (int)windowPosition.SavedLocation.Y);
                        //var p = Screen.FromPoint( _location );
                        Screen screenTarget = Screen.FromPoint(point);
                        if (screenTarget.Equals(screen2))
                        {
                            int y = (int)windowPosition.SavedLocation.Y;
                            //Console.WriteLine( "y = {0} and window.Height = {1} and screen2.Bounds.Bottom = {2}", y, window.Height, screen2.Bounds.Bottom );
                            if (y + window.Height > screen2.Bounds.Bottom)
                            {
                                double offset = SystemParameters.WindowCaptionHeight;
                                window.Top = screen2.Bounds.Bottom - window.Height - offset;
                            }
                        }
                    }
                }
            }
            else if (windowPosition.IsSavingLocation && (Math.Abs(windowPosition.RestorationLocation.X) > Double.Epsilon || Math.Abs(windowPosition.RestorationLocation.Y) > Double.Epsilon))
            {
                window.Left = windowPosition.RestorationLocation.X;
                window.Top = windowPosition.RestorationLocation.Y;
                if (windowPosition.IsSavingSize)
                {
                    window.Width = windowPosition.RestorationSize.Width;
                    window.Height = windowPosition.RestorationSize.Height;
                }
            }
            window.WindowState = windowPosition.WindowState;
#endif
        }
        #endregion LoadPosition

        #region SavePositionOf
        /// <summary>
        /// Set the property-values of PositionOfMainWindow from the on-screen location and size of the givne WPF-window.
        /// </summary>
        /// <param name="window">the WPF-window to save the on-screen extent of</param>
        public virtual void SavePositionOf( Window window )
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            PositionOfMainWindow.HasChanged = window.WindowState != PositionOfMainWindow.WindowState;
            PositionOfMainWindow.WindowState = window.WindowState;
            if (PositionOfMainWindow.WindowState != WindowState.Normal)
            {
                PositionOfMainWindow.RestorationLocation = new Point(window.RestoreBounds.X, window.RestoreBounds.Y);
                PositionOfMainWindow.RestorationSize = new Size(window.RestoreBounds.Width, window.RestoreBounds.Height);
                PositionOfMainWindow.HasChanged = true;
            }
            else
            {
                if (PositionOfMainWindow.IsSavingLocation)
                {
                    if (PositionOfMainWindow.IsLocationValue)
                    {
                        bool hasLocationChanged = (Math.Abs(window.Left - PositionOfMainWindow.SavedLocation.X) > Double.Epsilon)
                                               || (Math.Abs(window.Top - PositionOfMainWindow.SavedLocation.Y) > Double.Epsilon);
                        if (hasLocationChanged)
                        {
                            PositionOfMainWindow.SavedLocation = new Point(window.Left, window.Top);
                            PositionOfMainWindow.HasChanged = true;
                        }
                    }
                    else
                    {
                        if (!Double.IsNaN(window.Left) && !Double.IsNaN(window.Top))
                        {
                            PositionOfMainWindow.SavedLocation = new Point(window.Left, window.Top);
                            PositionOfMainWindow.HasChanged = true;
                        }
                    }
                }
                if (PositionOfMainWindow.IsSavingSize)
                {
                    if (PositionOfMainWindow.IsSizeValue)
                    {
                        bool hasSizeChanged = (Math.Abs(window.Width - PositionOfMainWindow.SavedSize.Width) > Double.Epsilon)
                                           || (Math.Abs(window.Height - PositionOfMainWindow.SavedSize.Height) > Double.Epsilon);
                        if (hasSizeChanged)
                        {
                            PositionOfMainWindow.SavedSize = new Size(window.Width, window.Height);
                            PositionOfMainWindow.HasChanged = true;
                        }
                    }
                    else
                    {
                        if (!Double.IsNaN(window.Width) && !Double.IsNaN(window.Height))
                        {
                            PositionOfMainWindow.SavedSize = new Size(window.Width, window.Height);
                            PositionOfMainWindow.HasChanged = true;
                        }
                    }
                }
            }
        }
        #endregion SavePositionOf

        //#region SaveWindowPositionIfChanged
        ///// <summary>
        ///// If the position of the main-window has changed since the last save, save it.
        ///// </summary>
        ///// <param name="userSettings">this is your class that derives from UserSettings that contains the saved position</param>
        ///// <param name="mainWindow">the WPF-window to save the position of</param>
        //public virtual void SaveWindowPositionIfChanged(Window mainWindow)
        //{
        //    if (HasPositionBeenSet)
        //    {
        //        SavePosition(mainWindow);
        //    }
        //}
        //#endregion

        #region SetWindowToSavedPosition
        /// <summary>
        /// Position the given WPF-window on the display-screen
        /// according to the state-values that were previously saved for it.
        /// This only happens once -- thereafter it does nothing.
        /// </summary>
        /// <param name="userSettings">this is your class that derives from UserSettings that contains the saved position</param>
        /// <param name="window">the WPF-window to set the position of</param>
        public void SetWindowToSavedPosition( Window window )
        {
            if (!this.PositionOfMainWindow.HasPositionBeenSet)
            {
                LoadPosition(this.PositionOfMainWindow, window);
                this.PositionOfMainWindow.HasPositionBeenSet = true;
            }
        }
        #endregion

        #region fields

        /// <summary>
        /// This flag denotes whether to persist the location of the window on the display-screen,
        /// which is distinct from the it's dimensions.
        /// The default is true.
        /// </summary>
        public bool _isToSaveLocation = true;

        /// <summary>
        /// This flag denotes whether to persist the dimensions of the window,
        /// which is distinct from the location.
        /// The default is false.
        /// </summary>
        public bool _isToSaveSize;

        #endregion fields
    }
}
