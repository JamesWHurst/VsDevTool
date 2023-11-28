using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;


// The purpose of the classes within this file, is simply to provide ways to set the mouse wait-cursor, and then reset it.
//
// See this rather-busy StackOverflow thread, on how to 'properly' set the mouse wait-cursor.
// http://stackoverflow.com/questions/3480966/display-hourglass-when-application-is-busy
//
// 2015-1-13, James W. Hurst


namespace Hurst.BaseLibWpf
{
    #region class UiServices
    /// <summary>
    /// The UiServices class is intended to contain helper methods for UX.
    /// Thus far it has just one - for showing a wait-cursor.
    /// </summary>
    public class UiServices
    {
        /// <summary>
        /// Set the busy-state to "busy". It reverts back to the default cursor automatically as soon as the UI thead becomes idle.
        /// </summary>
        public static void SetBusyState()
        {
            SetBusyState( true );
        }

        /// <summary>
        /// Set the busy-state to either busy or not busy. If busy - set the mouse wait-cursor.
        /// </summary>
        /// <param name="busy">if set to true, the application is now busy indicated as being busy</param>
        private static void SetBusyState( bool busy )
        {
            if (busy != _isBusy)
            {
                _isBusy = busy;
                Mouse.OverrideCursor = busy ? Cursors.Wait : null;

                if (_isBusy)
                {
                    _timer = new DispatcherTimer( TimeSpan.FromSeconds( 0 ), DispatcherPriority.ApplicationIdle, OnTick, Application.Current.Dispatcher );
                }
                else
                {
                    ClearTimer( _timer );
                }
            }
        }

        /// <summary>
        /// Handles the Tick event of the dispatcherTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void OnTick( object sender, EventArgs e )
        {
            var dispatcherTimer = sender as DispatcherTimer;
            if (dispatcherTimer != null)
            {
                SetBusyState( false );
                ClearTimer( dispatcherTimer );
            }
        }

        private static void ClearTimer( DispatcherTimer timer )
        {
            if (timer != null)
            {
                timer.Stop();
            }
        }

        /// <summary>
        /// This flag is used to indicate whether the UI is currently busy.
        /// </summary>
        private static bool _isBusy;

        private static DispatcherTimer _timer;
    }
    #endregion


    #region class WaitCursor
    /// <summary>
    /// The WaitCursor class exists simply to provide a concise way to set the wait-cursor,
    /// and ensure it gets set back.
    /// </summary>
    public class WaitCursor : IDisposable
    {
        /// <summary>
        /// Create a new WaitCursor object - for the purpose of setting the mouse cursor to the Wait-cursor,
        /// and ensuring that it gets reset when this object goes out of Using-scope.
        /// </summary>
        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }

        /// <summary>
        /// The original mouse-cursor that was set when this WaitCursor object was created.
        /// </summary>
        private readonly Cursor _previousCursor;
    }
    #endregion


    #region DisplayCursorFacility
    /// <summary>
    /// This static class provides yet another screen mouse-cursor utility,
    /// for showing the wait-cursor and then setting it back.
    /// This used to be Configuration\LuVivaApplication.
    /// </summary>
    public static class DisplayCursorFacility
    {
        //CBL This really should be migrated into a parent-class of the application.

        public static bool IsShowingBusyCursor { get; set; }

        public static void SetDefaultCursor( Cursor cursor )
        {
            //App.Logger.LogDebug( "LuVivaApplication.SetDefaultCursor( {0} )", cursor );
            _defaultCursor = cursor;
        }

        public static void ShowDefaultCursor()
        {
            //App.Logger.LogDebug( "LuVivaApplication.ShowDefaultCursor" );
            ShowCursor( _defaultCursor );
            IsShowingBusyCursor = false;
        }

        public static void ShowBusyCursor()
        {
            //App.Logger.LogDebug( "LuVivaApplication.ShowBusyCursor" );
            if (_defaultCursor == null)
            {
                _defaultCursor = Mouse.OverrideCursor;
            }
            ShowCursor( Cursors.Wait );
            IsShowingBusyCursor = true;
        }

        private static void ShowCursor( Cursor cursor )
        {
            if (Application.Current != null)
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    Mouse.OverrideCursor = cursor;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke( method: new Action( () =>
                    {
                        Mouse.OverrideCursor = cursor;
                    } ) );
                }
            }
        }

        private static System.Windows.Input.Cursor _defaultCursor;
    }
    #endregion DisplayCursorFacility
}
