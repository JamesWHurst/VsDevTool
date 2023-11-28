#if PRE_4
#define PRE_5
#endif
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Hurst.LogNut.Util;
using UiBaseLib;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using Control = System.Windows.Controls.Control;
using TextBox = System.Windows.Controls.TextBox;


namespace Hurst.BaseLibWpf
{
    #region class DesignerProperties
    /// <summary>
    /// Provides a custom implementation of DesignerProperties.GetIsInDesignMode
    /// to work around an issue.
    /// </summary>
    public static class DesignerProperties
    {
        // The method here is inspired by David Ansons blog article at http://blogs.msdn.com/b/delay/archive/2009/02/26/designerproperties-getisindesignmode-forrealz-how-to-reliably-detect-silverlight-design-mode-in-blend-and-visual-studio.aspx
        // dated 2009/2/26.
        // The issue was that DesignerProperties.GetIsInDesignMode doesn't always return the correct value under Visual Studio.

        /// <summary>
        /// Returns whether the control is in design mode (running under Blend
        /// or Visual Studio).
        /// </summary>
        /// <param name="dependencyObject">The element from which the property value is
        /// read.</param>
        /// <returns>True if in design mode.</returns>
        [SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "element", Justification =
            "Matching declaration of System.ComponentModel.DesignerProperties.GetIsInDesignMode (which has a bug and is not reliable)." )]
#if !PRE_4
        public static bool GetIsInDesignMode( DependencyObject dependencyObject = null )
#else
        public static bool GetIsInDesignMode(DependencyObject dependencyObject)
#endif
        {
#if SILVERLIGHT
            return ViewModel.IsInDesignModeStatic;
#else
            if (!_isInDesignMode.HasValue)
            {
                if (dependencyObject == null)
                {
                    // We can assume we're in 'design-mode', if Application.Current returns null,
                    // or if the type of the current application is simply Application, as opposed to our subclass of Application.
                    if (Application.Current == null || Application.Current.GetType() == typeof( Application ))
                    {
                        //CBL  This merits some work to see how best to do this.
                        //if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                        //{
                        //    Debug.WriteLine("Yeah, but System.ComponentModel.DesignerProperties thinks we are in design-mode.");
                        //}
                        //else
                        //{
                        //    Debug.WriteLine("Yeah, but System.ComponentModel.DesignerProperties thinks we are not in design-mode.");
                        //}
                        _isInDesignMode = System.ComponentModel.DesignerProperties.GetIsInDesignMode( new DependencyObject() );
                        //_isInDesignMode = true;
                    }
                    else
                    {
                        // At this point, Application.Current is not null, so let's check it's root visual element.
                        Window theMainWindow = Application.Current.MainWindow;
                        if (theMainWindow == null)
                        {
                            _isInDesignMode = true;
                        }
                        else
                        {
                            _isInDesignMode = System.ComponentModel.DesignerProperties.GetIsInDesignMode( theMainWindow );
                        }
                    }
                }
                else
                {
                    _isInDesignMode = System.ComponentModel.DesignerProperties.GetIsInDesignMode( dependencyObject );
                }
            }
            return _isInDesignMode.Value;
#endif
        }

        /// <summary>
        /// Get whether we are in design mode (ie, running under Blend or Visual Studio designer).
        /// Does the same thing as GetIsInDesignMode(null) -- just a shortcut method.
        /// </summary>
        public static bool IsInDesignMode
        {
            get { return GetIsInDesignMode( null ); }
        }

        /// <summary>
        /// Stores the computed InDesignMode value. A nullable is used such that the null state indicates that the value has not been set yet.
        /// </summary>
#if !SILVERLIGHT
        private static bool? _isInDesignMode;
#endif
    }
    #endregion class DesignerProperties

    public static class WindowExtensions
    {
        #region LoadPosition
        /// <summary>
        /// Set the location and/or size of the given WPF-window from the values
        /// contained within this <see cref="WindowPosition"/>.
        /// </summary>
        /// <param name="windowPosition">the saved position to load the window's new position from</param>
        /// <param name="window">the WPF-window to set the position of</param>
//        public static void LoadPosition( this WindowPosition windowPosition, Window window )
//        {
//            if (windowPosition == null)
//            {
//                throw new ArgumentNullException( "windowPosition" );
//            }
//            if (window == null)
//            {
//                throw new ArgumentNullException( "window" );
//            }
//            //CBL
//#if !NETFX_CORE
//            if (windowPosition.WindowState == WindowState.Normal)
//            {
//                // Deal with multiple monitors.
//                if (windowPosition.IsSavingSize && windowPosition.SavedSize.Width > 0 && windowPosition.SavedSize.Height > 0)
//                {
//                    window.Width = windowPosition.SavedSize.Width;
//                    window.Height = windowPosition.SavedSize.Height;
//                }
//                if (windowPosition.IsSavingLocation && (Math.Abs( windowPosition.SavedLocation.X ) > Double.Epsilon || Math.Abs( windowPosition.SavedLocation.Y ) > Double.Epsilon))
//                {
//                    window.Left = windowPosition.SavedLocation.X;
//                    window.Top = windowPosition.SavedLocation.Y;

//                    // Apply a correction if the previous settings had it located on a monitor that no longer is available.
//                    //
//                    double virtualScreenTop = System.Windows.SystemParameters.VirtualScreenTop;
//                    double virtualScreenWidth = System.Windows.SystemParameters.VirtualScreenWidth;
//                    double virtualScreenHeight = System.Windows.SystemParameters.VirtualScreenHeight;
//                    double virtualScreenLeft = System.Windows.SystemParameters.VirtualScreenLeft;
//                    double virtualScreenRight = virtualScreenLeft + virtualScreenWidth;
//                    double virtualScreenBottom = virtualScreenTop + virtualScreenHeight;
//                    double myWidth = window.Width;
//                    double myBottom = window.Top + window.Height;

//                    // If the 2nd monitor was to the right, and is now not..
//                    if (window.Left > (virtualScreenRight - myWidth))
//                    {
//                        window.Left = virtualScreenRight - myWidth;
//                    }
//                    // or if it was to the left..
//                    else if (window.Left < virtualScreenLeft)
//                    {
//                        window.Left = virtualScreenLeft;
//                    }
//                    // or if there was a vertical change..
//                    if (myBottom > virtualScreenBottom)
//                    {
//                        window.Top = virtualScreenBottom - window.Height;
//                    }
//                    else if (window.Top < virtualScreenTop)
//                    {
//                        window.Top = virtualScreenTop;
//                    }

//                    // I had one case wherein my 2nd monitor, to my right, was of lesser resolution, location.y = 1075, but the monitor height was 1080.
//                    // CBL  I could not see the dam thing.
//                    if (Screen.AllScreens.Length > 1)
//                    {
//                        // We have more than one monitor.
//                        // Note: This requires that we pull in assembly System.Drawing.
//                        Screen screen1 = Screen.AllScreens[0];
//                        Screen screen2 = Screen.AllScreens[1];
//                        System.Drawing.Point point = new System.Drawing.Point( (int)windowPosition.SavedLocation.X, (int)windowPosition.SavedLocation.Y );
//                        //var p = Screen.FromPoint( _location );
//                        Screen screenTarget = Screen.FromPoint( point );
//                        if (screenTarget.Equals( screen2 ))
//                        {
//                            int y = (int)windowPosition.SavedLocation.Y;
//                            //Console.WriteLine( "y = {0} and window.Height = {1} and screen2.Bounds.Bottom = {2}", y, window.Height, screen2.Bounds.Bottom );
//                            if (y + window.Height > screen2.Bounds.Bottom)
//                            {
//                                double offset = SystemParameters.WindowCaptionHeight;
//                                window.Top = screen2.Bounds.Bottom - window.Height - offset;
//                            }
//                        }
//                    }

//                }
//            }
//            else if (windowPosition.IsSavingLocation && (Math.Abs( windowPosition.RestorationLocation.X ) > Double.Epsilon || Math.Abs( windowPosition.RestorationLocation.Y ) > Double.Epsilon))
//            {
//                window.Left = windowPosition.RestorationLocation.X;
//                window.Top = windowPosition.RestorationLocation.Y;
//                if (windowPosition.IsSavingSize)
//                {
//                    window.Width = windowPosition.RestorationSize.Width;
//                    window.Height = windowPosition.RestorationSize.Height;
//                }
//            }
//            window.WindowState = windowPosition.WindowState;
//#endif
//        }
        #endregion LoadPosition

        //#region SaveIfChanged
        ///// <summary>
        ///// Save the position of the specified Window, and if the properties have changed
        ///// since the last save - call the <see cref="Save"/> method to store these properties to permanent storage
        ///// and then clear the <see cref="UserSettings.IsChanged"/> flag.
        ///// </summary>
        ///// <param name="userSettings">this is your class that derives from UserSettings that contains the saved position</param>
        ///// <param name="mainWindow">the WPF window to save the position of</param>
        //public static void SaveIfChanged( this UserSettings userSettings, Window mainWindow )
        //{
        //    userSettings.PositionOfMainWindow.SavePosition( mainWindow );
        //    // If anything has changed, or, if no settings file was ever found,
        //    // save the current settings to it's underlying data-store.
        //    if (userSettings.IsChanged || userSettings.PositionOfMainWindow.HasChanged || !UserSettings.IsFound)
        //    {
        //        userSettings.Save();
        //        userSettings.PositionOfMainWindow.HasChanged = false;
        //    }
        //}
        //#endregion

        //#region SaveWindowPositionIfChanged
        ///// <summary>
        ///// If the position of the main-window has changed since the last save, save it.
        ///// </summary>
        ///// <param name="userSettings">this is your class that derives from UserSettings that contains the saved position</param>
        ///// <param name="mainWindow">the WPF-window to save the position of</param>
        //public static void SaveWindowPositionIfChanged( this UserSettings userSettings, Window mainWindow )
        //{
        //    if (userSettings.HasPositionBeenSet)
        //    {
        //        userSettings.SaveIfChanged( mainWindow );
        //    }
        //}
        //#endregion

        //#region SetWindowToSavedPosition
        ///// <summary>
        ///// Position the given WPF-window on the display-screen
        ///// according to the state-values that were previously saved for it.
        ///// This only happens once -- thereafter it does nothing.
        ///// </summary>
        ///// <param name="userSettings">this is your class that derives from UserSettings that contains the saved position</param>
        ///// <param name="window">the WPF-window to set the position of</param>
        //public static void SetWindowToSavedPosition( this WpfUserSettings userSettings, Window window )
        //{
        //    if (!userSettings.HasPositionBeenSet)
        //    {
        //        userSettings.PositionOfMainWindow.LoadPosition( window );
        //        userSettings.HasPositionBeenSet = true;
        //    }
        //}
        //#endregion
    }


    /// <summary>
    /// This class is simply for hanging some WPF-specific extension methods upon.
    /// </summary>
    public static class WpfExtensions
    {
        //#region SetWindowToSavedPosition
        ///// <summary>
        ///// Position the given WPF-window on the display-screen
        ///// according to the state-values that were previously saved for it.
        ///// This only happens once -- thereafter it does nothing.
        ///// </summary>
        ///// <param name="userSettings">this is your class that derives from UserSettings that contains the saved position</param>
        ///// <param name="window">the WPF-window to set the position of</param>
        //public static void SetWindowToSavedPosition(this Window window, WindowPosition where)
        //{
        //    if (!where.HasPositionBeenSet)
        //    {
        //        PositionOfMainWindow.LoadPosition(window);
        //        where.HasPositionBeenSet = true;
        //    }
        //}
        //#endregion

        //public static double GetDpiScaleX(Window window)
        //{
        //    var x = VisualTreeHelper.GetDpi( window );
        //    return x.DpiScaleX;
        //}

        #region GetTaskBarLocation
        /// <summary>
        /// Determine the position of the Windows TaskBar and return it as a TaskBarLocation enum value.
        /// </summary>
        /// <returns></returns>
        public static TaskBarLocation GetTaskBarLocation()
        {
            TaskBarLocation answer;
            //System.Windows.SystemParameters....
            if (SystemParameters.WorkArea.Left > 0)
            {
                answer = TaskBarLocation.Left;
            }
            else if (SystemParameters.WorkArea.Top > 0)
            {
                answer = TaskBarLocation.Top;
            }
            else if (MathLib.IsEssentiallyEqual( SystemParameters.WorkArea.Left, 0 ) && SystemParameters.WorkArea.Width < SystemParameters.PrimaryScreenWidth)
            {
                answer = TaskBarLocation.Right;
            }
            else
            {
                answer = TaskBarLocation.Bottom;
            }
            return answer;
        }
        #endregion

        #region TaskBarHeight
        /// <summary>
        /// Get the height (in pixels) of the Windows Task-Bar that is currently displayed on the screen.
        /// </summary>
        public static double TaskBarHeight
        {
            get
            {
                var bounds = Screen.PrimaryScreen.Bounds;
                var workingArea = Screen.PrimaryScreen.WorkingArea;
                return bounds.Height - workingArea.Height;
            }
        }
        #endregion

        #region IsInDesignMode
        /// <summary>
        /// Return a flag indicating whether we are currently running within the Visual Studio or Blend designer.
        /// </summary>
        /// <param name="anyDependencyObject">The Window or other DependencyObject to make this test on</param>
        /// <returns>true if we're now running within a designer, false if executing normally</returns>
        public static bool IsInDesignMode( this DependencyObject anyDependencyObject )
        {
            // Thanks to Alan Le for this tip.
#if Silverlight
            return !HtmlPage.IsEnabled;
#else
            return System.ComponentModel.DesignerProperties.GetIsInDesignMode( anyDependencyObject );
#endif
        }
        #endregion

        public static bool IsMultipleScreensHorizontally
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (SystemParameters.VirtualScreenWidth > SystemParameters.PrimaryScreenWidth); }
        }

        public static double PrimaryScreenWidth
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return SystemParameters.PrimaryScreenWidth; }
        }

        public static double VirtualScreenBottom
        {
            //[System.Diagnostics.DebuggerStepThrough]
            get
            {
//CBL Comment this.
                double bottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;
                if (GetTaskBarLocation() == TaskBarLocation.Bottom)
                {
                    bottom -= TaskBarHeight;
                }
                return bottom;
            }
        }

        public static double VirtualScreenLeft
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return SystemParameters.VirtualScreenLeft; }
        }

        public static double VirtualScreenRight
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth; }
        }

        #region AlignToParent
        /// <summary>
        /// Causes the given Window to align itself alongside the parent (ie, Owner) window.
        /// You should put the call to this within the LayoutUpdated or Loaded event handler of the Window you want to align itself
        /// to it's parent. The Owner property has to have been set, otherwise there's nothing for it to align against.
        /// </summary>
        /// <param name="dialog">The given Window that is to be aligned</param>
        /// <param name="inWhichDirection">Specifieds a preference toward which side or edge to align it to, if there's room on the display for that.</param>
        public static void AlignToParent( this Window dialog, AlignmentType inWhichDirection )
        {
            //TODO ?
#if !SILVERLIGHT
            Window parent = dialog.Owner as Window;
            if (parent != null)
            {
                if (inWhichDirection == AlignmentType.ToRightOfParent)
                {
                    // Try the right side, then the left.
                    if (AlignToRight( dialog ))
                    {
                        return;
                    }
                    else if (AlignToLeft( dialog ))
                    {
                        return;
                    }
                }
                else if (inWhichDirection == AlignmentType.ToLeftOfParent)
                {
                    // Try the left side, then the right.
                    if (AlignToLeft( dialog ))
                    {
                        return;
                    }
                    else if (AlignToRight( dialog ))
                    {
                        return;
                    }
                }
                else if (inWhichDirection == AlignmentType.AboveParent)
                {
                    double parentTop = parent.Top;
                    double myHeight = dialog.Height;
                    double separation = 2;
                    if (parentTop >= myHeight)
                    {
                        if (parentTop > (myHeight + separation))
                        {
                            dialog.Top = parentTop - separation - myHeight;
                        }
                        else
                        {
                            dialog.Top = parentTop - myHeight;
                        }
                        dialog.Left = parent.Left;
                        return;
                    }
                }
                // failing that, I'll try underneath
                AlignToBottom( dialog );
                // otherwise.. at this point I think it's time to throw in the towel and forget about it.
            }
#endif
        }

        #region internal helper methods for AlignToParent

        //public static bool IsWithin( this IBaseWindow window, WPFDisplayScreen screen )
        //{
        //    bool answer = false;

        //    if (window.Left > screen.Left && window.Left + window.Width < screen.Right)
        //    {
        //        if (window.Top > screen.Top && window.Top + window.Height < screen.Bottom)
        //        {
        //            answer = true;
        //        }
        //    }

        //    return answer;
        //}

        /// <summary>
        /// This is a helper method for AlignToParent; it aligns the given Window to the right of the parent-Window, if possible.
        /// </summary>
        /// <param name="dialog">the Window to align</param>
        /// <returns>true if successful, false if there wasn't sufficient space on the display</returns>
        private static bool AlignToRight( Window dialog )
        {
            UiWindow window = new WPFBaseWindow( dialog );
            System.Windows.Forms.Screen[] allScreens = System.Windows.Forms.Screen.AllScreens;
            DisplayScreen otherScreen = null;
            if (allScreens.Count() > 1)
            {
                //CBL Here, screen2 is actually the primary screen (the left one) !
                System.Windows.Forms.Screen screen1 = allScreens[0];
                System.Windows.Forms.Screen screen2 = allScreens[1];

                // See which of these 2 screen the dialog's parent is on..

                DisplayScreen displayScreen1 = new DisplayScreen( screen1 );
                DisplayScreen displayScreen2 = new DisplayScreen( screen2 );
                UiWindow parent = window.Parent;

                if (parent.IsWithin( displayScreen2 ))
                {
                    // 'other screen' is the one that is NOT the one the parent-window is on.
                    otherScreen = new DisplayScreen( screen1 );
                }
                else
                {
                    otherScreen = new DisplayScreen( screen2 );
                }
            }

            return UiBaseLib.UiBaseLib.AlignToRight( window, IsMultipleScreensHorizontally,
                (int)PrimaryScreenWidth, (int)VirtualScreenRight, (int)VirtualScreenBottom,
                otherScreen, GetTaskBarLocation(), TaskBarHeight);
        }

        /// <summary>
        /// Just a helper method for AlignToParent
        /// </summary>
        /// <param name="dialog">the Window to align</param>
        /// <returns>true if successful, false if there wasn't sufficient space on the display</returns>
        private static bool AlignToLeft( Window dialog )
        {
            //TODO  ?
#if SILVERLIGHT
            return true;
#else
            Window parent = dialog.Owner as Window;
            const double separation = 1;
            double virtualScreenBottom = VirtualScreenBottom;
            double screenLeft = VirtualScreenLeft;

            if (IsMultipleScreensHorizontally)
            {
                System.Windows.Forms.Screen[] allScreens = System.Windows.Forms.Screen.AllScreens;
                if (allScreens.Count() > 1)
                {
                    System.Windows.Forms.Screen s2 = allScreens[1];
                    double left2 = s2.Bounds.Left;
                    double right2 = s2.Bounds.Right;

                    if (parent.Left.IsInRange( left2, right2 ))
                    {
                        // Evidently, this Window is on Screen 2, so use it's area.
                        double xToLeftOfParent = parent.Left - left2;
                        if (xToLeftOfParent > dialog.Width)
                        {
                            dialog.Left = parent.Left - dialog.Width - separation;
                            dialog.Top = Math.Min( parent.Top, virtualScreenBottom - dialog.Height );
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            // Try the left side.
            //TODO: Check this for various positions of secondary display.

            if ((parent.Left - screenLeft) > dialog.Width)
            {
                dialog.Left = parent.Left - dialog.Width - separation;
                dialog.Top = Math.Min( parent.Top, virtualScreenBottom - dialog.Height );
                return true;
            }
            else
            {
                return false;
            }
#endif
        }

        private static bool AlignToBottom( Window dialog )
        {
#if SILVERLIGHT
            return true;
#else
            bool ok = true;
            Window parent = dialog.Owner as Window;
            // CBL  Is VirtualScreenTop always zero? !!!
            double virtualScreenBottom = VirtualScreenBottom;
            double parentBottom = parent.Top + parent.Height;
            const double separation = 1;

            if (parentBottom < (virtualScreenBottom - dialog.Height))
            {
                dialog.Top = parentBottom + separation;
                dialog.Left = Math.Max( parent.Left, VirtualScreenLeft );
                // Ensure the right-edge of this dialog-window has not gone off the right edge of the display.
                var rightEdge = VirtualScreenRight;
                if (dialog.Left + dialog.Width > rightEdge)
                {
                    dialog.Left = rightEdge - dialog.Width;
                }
            }
            // failing that, I'll try over-top
            else if (parent.Top > dialog.Height)
            {
                dialog.Left = Math.Max( parent.Left, VirtualScreenLeft );
                dialog.Top = parent.Top - dialog.Height - separation;
                var displayTop = SystemParameters.VirtualScreenTop;
                if (dialog.Top > displayTop)
                {
                    // That plants it to high - beyond the top edge of the display.
                    ok = AlignToLeft( dialog );
                }
            }
            else
            {
                ok = false;
            }
            return ok;
#endif
        }
        #endregion

        #endregion

        #region SetFocusUponControl
        /// <summary>
        /// Call the <c>Focus</c> method on the control that has the given name.
        /// </summary>
        /// <param name="parent">a UI-element (a DependencyObject) that contains the control you want to go to</param>
        /// <param name="controlName">the name of the control to place focus upon</param>
        /// <returns>true if that control was found</returns>
        /// <remarks>
        /// This requires that you employ a certain naming-convention for your UX controls.
        /// These prefices are expected:
        /// "txt"  => TextBox
        /// "btn"  => Button
        /// "ckbx" => CheckBox
        /// "cb"   => ComboBox
        /// </remarks>
        public static bool SetFocusUponControl( DependencyObject parent, string controlName )
        {
            if (controlName != null)
            {
                // Check for TextBox, CheckBox, Button.
                FrameworkElement fieldToGoTo = null;
                if (controlName.StartsWith( "txt" ))
                {
                    fieldToGoTo = FindChild<TextBox>( parent, controlName );
                }
                else if (controlName.StartsWith( "btn" ))
                {
                    fieldToGoTo = FindChild<Button>( parent, controlName );
                }
                else if (controlName.StartsWith( "ckbx" ))
                {
                    fieldToGoTo = FindChild<CheckBox>( parent, controlName );
                }
                else if (controlName.StartsWith( "cb" ))
                {
                    fieldToGoTo = FindChild<ComboBox>( parent, controlName );
                }
                else
                {
                    fieldToGoTo = FindChild<Control>( parent, controlName );
                }
                if (fieldToGoTo != null)
                {
                    fieldToGoTo.Focus();
                    return true;
                }
                Console.WriteLine( "In SetFocusUponControl, unable to find control with name {0} .", controlName );
            }
            return false;
        }
        #endregion

        #region FindChild
        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>( DependencyObject parent, string childName ) where T : DependencyObject
        {
            // This is by "CrimsonX" on Stack Overflow: http://stackoverflow.com/questions/636383/how-can-i-find-wpf-controls-by-name-or-type

            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount( parent );
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild( parent, i );
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>( child, childName );

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty( childName ))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
        #endregion

        #region IsEssentiallyEqualTo

        public static bool IsEssentiallyEqualTo( this Size sizeValue1, Size sizeValue2 )
        {
            bool bResult = false;
            // If both are Empty, then consider them equal
            if (sizeValue1.IsEmpty)
            {
                bResult = sizeValue2.IsEmpty;
            }
            else if (sizeValue2.IsEmpty)
            {
                bResult = sizeValue1.IsEmpty;
            }
            else
            {
                bResult = MathLib.IsEssentiallyEqual( sizeValue1.Width, sizeValue2.Width ) && MathLib.IsEssentiallyEqual( sizeValue1.Height, sizeValue2.Height );
            }
            return bResult;
        }
        #endregion

        #region TextBox extension method InsertText
        /// <summary>
        /// Insert the given text into this TextBox at the current CaretIndex, and replacing any already-selected text.
        /// </summary>
        /// <param name="textbox">The TextBox to insert the new text into</param>
        /// <param name="sTextToInsert">The text to insert into this TextBox</param>
        public static void InsertText( this System.Windows.Controls.TextBox textbox, string sTextToInsert )
        {
#if !SILVERLIGHT
            int iCaretIndex = textbox.CaretIndex;
            int iOriginalSelectionLength = textbox.SelectionLength;
            textbox.SelectedText = sTextToInsert;
            if (iOriginalSelectionLength > 0)
            {
                textbox.SelectionLength = 0;
            }
            textbox.CaretIndex = iCaretIndex + 1;
#else
            //TODO ?

            // Newer method, if needed (TODO: Test to see if this is needed.

            //int iOriginalTextLength = sOriginalContent.Length;
            //if (iOriginalSelectionLength == 0)
            //{
            //    if (iCaretIndex == iOriginalTextLength)
            //    {
            //        textbox.Text = sOriginalContent + sTextToInsert;
            //    }
            //    else if (iCaretIndex == 0)
            //    {
            //        textbox.Text = sTextToInsert + sOriginalContent;
            //    }
            //    else
            //    {
            //        string sPartBefore = sOriginalContent.Substring(0, iCaretIndex);
            //        string sRemainder = sOriginalContent.Substring(iCaretIndex);
            //        textbox.Text = sPartBefore + sTextToInsert + sRemainder;
            //    }
            //}
            //else  //TODO: Perhaps only this last part is sufficient?
            //{
            //    textbox.SelectedText = sTextToInsert;
            //    if (iOriginalSelectionLength > 0)
            //    {
            //        textbox.SelectionLength = 0;
            //    }
            //}
            //textbox.CaretIndex = iCaretIndex + sTextToInsert.Length;
#endif
        }
        #endregion

        #region TextBox extension method InsertTextLeftToRight
        /// <summary>
        /// Insert the given text into this TextBox at the current CaretIndex, and replacing any already-selected text.
        /// </summary>
        /// <param name="textbox">The TextBox to insert the new text into</param>
        /// <param name="sTextToInsert">The text to insert into this TextBox</param>
        public static void InsertTextLeftToRight( this System.Windows.Controls.TextBox textbox, string sTextToInsert )
        {
            // This replaces the algorithm that the preceding method uses, just to ensure the concatenation is LeftToRight.
            //TODO: This needs to be tested.
            //TODO  ?
#if !SILVERLIGHT
            int iCaretIndex = textbox.CaretIndex;
            int iOriginalSelectionLength = textbox.SelectionLength;
            string sOriginalContent = textbox.Text;
            int iOriginalTextLength = sOriginalContent.Length;
            if (iOriginalSelectionLength == 0)
            {
                if (iCaretIndex == iOriginalTextLength)
                {
                    textbox.Text = sOriginalContent.ConcatLeftToRight( sTextToInsert );
                }
                else if (iCaretIndex == 0)
                {
                    textbox.Text = sTextToInsert.ConcatLeftToRight( sOriginalContent );
                }
                else
                {
                    string sPartBefore = sOriginalContent.Substring( 0, iCaretIndex );
                    string sRemainder = sOriginalContent.Substring( iCaretIndex );
                    // This next line does this, but using the ConcatLeftToRight method.
                    // textbox.Text = sPartBefore + sTextToInsert + sRemainder;
                    textbox.Text = sPartBefore.ConcatLeftToRight( sTextToInsert ).ConcatLeftToRight( sRemainder );
                }
            }
            else  //TODO: Perhaps only this last part is sufficient?
            {
                textbox.SelectedText = sTextToInsert;
                if (iOriginalSelectionLength > 0)
                {
                    textbox.SelectionLength = 0;
                }
            }
            textbox.CaretIndex = iCaretIndex + sTextToInsert.Length;
#endif
        }
        #endregion

        #region RichTextBox extension method InsertText
        /// <summary>
        /// Insert the given text into this RichTextBox at the current CaretPosition, replacing any already-selected text.
        /// </summary>
        /// <param name="richTextBox">The RichTextBox to insert the new text into</param>
        /// <param name="sTextToInsert">The text to insert into this RichTextBox</param>
        public static void InsertText( this System.Windows.Controls.RichTextBox richTextBox, string sTextToInsert )
        {
            if (StringLib.HasSomething( sTextToInsert ))
            {
                //TODO
#if !SILVERLIGHT
                richTextBox.BeginChange();
                if (richTextBox.Selection.Text != string.Empty)
                {
                    richTextBox.Selection.Text = string.Empty;
                }
                TextPointer tp = richTextBox.CaretPosition.GetPositionAtOffset( 0, LogicalDirection.Forward );
                richTextBox.CaretPosition.InsertTextInRun( sTextToInsert );
                richTextBox.CaretPosition = tp;
                richTextBox.EndChange();
                Keyboard.Focus( richTextBox );
#endif
            }
        }
        #endregion

        #region TextBox extension method TextTrimmed
        /// <summary>
        /// This is just a convenience extension-method to simplify the getting of strings
        /// from a WPF TextBox.
        /// It was a pain in da butt, having to remember to test for nulls, whitespace, etc.
        /// Now, all you have to do is check the .Length
        /// </summary>
        /// <param name="textbox">The WPF TextBox to get the Text from</param>
        /// <returns>If the TextBox was empty, then "" (empty string) otherwise the Text with leading and trailing whitespace trimmed</returns>
        public static string TextTrimmed( this System.Windows.Controls.TextBox textbox )
        {
            string sText = textbox.Text;
            if (StringLib.HasNothing( sText ))
            {
                return String.Empty;
            }
            else
            {
                return sText.Trim();
            }
        }
        #endregion

        #region Window extension method MoveAsAGroup

        public static void MoveAsAGroup( this Window me,
                                        double desiredXDisplacement, double desiredYDisplacement,
                                        ref bool isIgnoringLocationChangedEvent )
        {
            //TODO ?
#if !SILVERLIGHT
            // Ensure we don't recurse when we reposition.
            isIgnoringLocationChangedEvent = true;

            Window windowToMoveWith = me.Owner;

            // Try to prevent me from sliding off the screen horizontally.
            double bitToShow = 32;
            double leftLimit = SystemParameters.VirtualScreenLeft - me.Width + bitToShow;
            double rightLimit = SystemParameters.VirtualScreenWidth - bitToShow;
            bool notTooMuchXDisplacement = Math.Abs( me.Left - windowToMoveWith.Left ) < Math.Abs( desiredXDisplacement );
            if (me.Left >= rightLimit && notTooMuchXDisplacement)
            {
                // bumping against the right.
                me.Left = rightLimit;
            }
            else if (me.Left <= leftLimit && notTooMuchXDisplacement)
            {
                // bumping against the left.
                me.Left = leftLimit;
            }
            else // it's cool - just slide along with the other window.
            {
                me.Left = windowToMoveWith.Left + desiredXDisplacement;
            }

            // Try to prevent me from sliding off the screen vertically.
            double topLimit = SystemParameters.VirtualScreenTop - me.Height + bitToShow;
            double bottomLimit = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - bitToShow;
            bool notTooMuchYDisplacement = Math.Abs( me.Top - windowToMoveWith.Top ) < Math.Abs( desiredYDisplacement );
            if (me.Top <= topLimit && notTooMuchYDisplacement)
            {
                // bumping up against the top.
                //Debug.WriteLine("setting to topLimit of " + topLimit);
                me.Top = topLimit;
            }
            else if (me.Top >= bottomLimit && notTooMuchYDisplacement)
            {
                // bumping against the bottom.
                me.Top = bottomLimit;
            }
            else // it's cool - just slide along with the other window.
            {
                me.Top = windowToMoveWith.Top + desiredYDisplacement;
            }

            // Reset the handler for the LocationChanged event.
            isIgnoringLocationChangedEvent = false;
#endif
        }
        #endregion

        #region InvokeIfRequired
#if !SILVERLIGHT
        public static void InvokeIfRequired( this DispatcherObject control, Action methodCall )
        {
            //Provided for VS2008
            InvokeIfRequired( control, methodCall, DispatcherPriority.Background );
        }

        public static void InvokeIfRequired( this DispatcherObject control, Action methodCall, DispatcherPriority priorityForCall )
        {
            // This comes directly from Sacha Barber's article at http://www.codeproject.com/Articles/37314/Useful-WPF-Threading-Extension-Method.aspx

            if (control != null)
            {
                // See whether we need to invoke the call from the Dispatcher thread.
                if (control.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
                {
                    control.Dispatcher.Invoke( priorityForCall, methodCall );
                    return;
                }
            }
            methodCall();
        }
#endif
        #endregion

        #region SetText_WithThreadSafety
        /// <summary>
        /// Assign the given string to the Text property of the WPF TextBox, but in a thread-safe manner.
        /// This may be called from a thread or Task other than that of the GUI.
        /// </summary>
        /// <param name="textBox">The TextBox to put the text into</param>
        /// <param name="textToPutIntoTheTextBox">The text to put into the TextBox</param>
        public static void SetText_WithThreadSafety( this TextBox textBox, string textToPutIntoTheTextBox )
        {
            //TODO  How to do in SL?
#if SILVERLIGHT
            textBox.Text = textToPutIntoTheTextBox;
#else
            // Fror the following, thanks to Simon Knox.  See article http://www.codeproject.com/KB/WPF/ThreadSafeWPF.aspx
            InvokeIfRequired( textBox, () => { textBox.Text = textToPutIntoTheTextBox; }, DispatcherPriority.Background );
#endif
        }

        /// <summary>
        /// Assign the given string to the Text property of the WPF TextBlock, but in a thread-safe manner.
        /// This may be called from a thread or Task other than that of the GUI.
        /// </summary>
        /// <param name="textBlock">The TextBlock to put the text into</param>
        /// <param name="textToPutIntoTheTextBlock">The text to put into the TextBlock</param>
        public static void SetText_WithThreadSafety( this TextBlock textBlock, string textToPutIntoTheTextBlock )
        {
            //TODO  How to do in SL?
#if SILVERLIGHT
            textBlock.Text = textToPutIntoTheTextBlock;
#else
            InvokeIfRequired( textBlock, () => { textBlock.Text = textToPutIntoTheTextBlock; }, DispatcherPriority.Background );
#endif
        }
        #endregion

        #region GetText_WithThreadSafety
        /// <summary>
        /// Return the the contents of the Text property of the WPF TextBox, but in a thread-safe manner.
        /// This may be called from a thread or Task other than that of the GUI.
        /// In addition, the string that is returned is String.Empty if the text had only whitespace.
        /// </summary>
        /// <param name="textBox">The TextBox to get the text from</param>
        public static string GetText_WithThreadSafety( this TextBox textBox )
        {
            string text = String.Empty;
            //TODO  How to do in SL?
#if SILVERLIGHT
            text = textBox.Text;
#else
            InvokeIfRequired( textBox, () => { text = textBox.Text; }, DispatcherPriority.Background );
#endif
            if (StringLib.HasNothing( text ))
            {
                return String.Empty;
            }
            else
            {
                return text;
            }
        }
        #endregion

        #region Sort(ListView,..)
#if !SILVERLIGHT
        public static void Sort( this System.Windows.Controls.ListView listview, string sortBy, ListSortDirection direction )
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView( listview.ItemsSource );
            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription( sortBy, direction );
                dataView.SortDescriptions.Add( sd );
                dataView.Refresh();
            }
            else
            {
                Debug.WriteLine( "Why the heck is GetDefaultView returning null? !!!" );
            }
        }
#endif
        #endregion

        #region Reactivate

        #region class WindowEventArgs
        /// <summary>
        /// This subclass of EventArgs is intended for alerting the rest of the application of changes to the options, in realtime.
        /// </summary>
        public class WindowEventArgs : EventArgs
        {
            public WindowEventArgs( Window value )
            {
                this.Window = value;
            }

            public Window Window { get; set; }
        }
        #endregion class WindowEventArgs

        /// <summary>
        /// Make this Window the active one, by calling Activate and Focus
        /// but after a half-second delay in order to allow the user's clicking to not interfere.
        /// </summary>
        public static void Reactivate( this Window window )
        {
            //TODO: I ought to be able to pass the window within the args to the timer-handler!
            //var windowEventArgs = new WindowEventArgs(window);
            //EventArgs eventArgs = EventArgs.Empty;
            _windowToBeReactivated = window;

            _reactivationTimer = new DispatcherTimer( TimeSpan.FromSeconds( 0.5 ),
                                                      DispatcherPriority.Input,
                                                      new EventHandler( OnReactivationTimer2 ),
                                                      //new EventHandler<WindowEventArgs>(OnReactivationTimer, windowEventArgs),
                                                      window.Dispatcher );
        }

        private static DispatcherTimer _reactivationTimer;
        private static Window _windowToBeReactivated;

        private static void OnReactivationTimer2( object sender, EventArgs args )
        {
            if (_reactivationTimer != null)
            {
                _reactivationTimer.Stop();
                _reactivationTimer.Tick -= OnReactivationTimer2;
                _reactivationTimer = null;
            }
            if (_windowToBeReactivated != null)
            {
                // Both are needed.
                _windowToBeReactivated.Activate();
                _windowToBeReactivated.Focus();
                // Clear this reference so that it doesn't prevent the Window from being garbage-collected.
                _windowToBeReactivated = null;
            }
        }

        private static void OnReactivationTimer( object sender, WindowEventArgs args )
        {
            _reactivationTimer.Stop();
            //            _reactivationTimer.Tick -= OnReactivationTimer;
            _reactivationTimer = null;
            // Both are needed.
            //            Activate();
            //            Focus();
        }
        #endregion Reactivate
    }
}
