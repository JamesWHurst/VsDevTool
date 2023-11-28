#if PRE_4
#define PRE_5
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Hurst.LogNut.Util;


namespace Hurst.BaseLibWpf.Display
{
    /// <summary>
    /// This class contains all DisplayBox options that apply specifically to running automated tests.
    /// </summary>
    public class DisplayBoxTestFacility : IDisposable
    {
        #region constructor
        /// <summary>
        /// Make a new DisplayBoxTestFacility object with the default values.
        /// </summary>
        public DisplayBoxTestFacility( DisplayBox manager )
        {
            _displayBoxManager = manager;
            _isTesting = true;
            _isTimeoutsFast = true;
        }
        #endregion

        // The followiing functions are intended for use within Assert-type checks. If you add more, you should consider
        // starting with the prefix "Test", or "SetTest", so that Intellisense will group them together.

        #region IsDisplayBoxBeingShown
        /// <summary>
        /// Return true if there is at least one DisplayBox currently being displayed to the user.
        /// </summary>
        /// <returns>true if a display-box is being displayed</returns>
        public bool IsDisplayBoxBeingShown()
        {
            bool result = false;
            if (_displayBoxManager.DisplayBoxWindow._options.IsAsynchronous)
            {
#if !PRE_4
                bool didNotTimeOut = ResetEventForWindowShowing.Wait( 10000 );
#else
                bool didNotTimeOut = ResetEventForWindowShowing.WaitOne(10000);
#endif
                if (!didNotTimeOut)
                {
                    Debug.WriteLine( "IsDisplayBoxBeingShown: timed out waiting for thread synchronization." );
                }
            }
            if (InstancesCurrentlyDisplaying.Count > 0)
            {
                result = true;
            }
            return result;
        }
        #endregion

        #region HasADisplayBoxBeenShown
        /// <summary>
        /// Return true if there is not at least one DisplayBox currently being, or which has been, displayed to the user.
        /// </summary>
        /// <returns>true if a display-box is or has been displayed</returns>
        public bool HasADisplayBoxBeenShown()
        {
            return InstancesCurrentlyDisplaying.Count > 0 || InstancesThatHaveDisplayed.Count > 0;
        }
        #endregion

        #region IsWithinTitle
        /// <summary>
        /// Return true if the given textPattern string is present within the title-bar text of a display-box
        /// that is currently being displayed.
        /// The test is case-sensitive.
        /// </summary>
        /// <param name="textPattern">the pattern to check for</param>
        /// <returns>true if there is a display-box whose title-text contains the given textPattern</returns>
        public bool IsWithinTitle( string textPattern )
        {
            bool result = false;
            foreach (var displayBox in this.InstancesCurrentlyDisplaying)
            {
                string titleText = displayBox.Title;
                if (titleText.Contains( textPattern ))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        #endregion

        #region IsWithinSummaryText
        /// <summary>
        /// Return true if the give textPattern string is present within the summary-text of a display-box
        /// that is currently being displayed.
        /// The test is case-sensitive.
        /// </summary>
        /// <param name="textPattern">the pattern to check for</param>
        /// <returns>true if the textPattern is within the summary-text</returns>
        public bool IsWithinSummaryText( string textPattern )
        {
            bool result = false;
            foreach (var displayBox in this.InstancesCurrentlyDisplaying)
            {
                string summaryText = displayBox.SummaryText;
                if (summaryText.Contains( textPattern ))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        #endregion

        #region WasWithinSummaryText
        /// <summary>
        /// Return true if the given text was present within the summary-text of any display-box that has been displayed.
        /// The test is case-sensitive.
        /// </summary>
        /// <param name="textPattern">the pattern to check for</param>
        /// <returns>true if textPattern was in the summary-text</returns>
        public bool WasWithinSummaryText( string textPattern )
        {
            bool result = false;
            foreach (var displayBox in this.InstancesThatHaveDisplayed)
            {
                string summaryText = displayBox.SummaryText;
                if (summaryText.Contains( textPattern ))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        #endregion

        #region IsWithinDetailText
        /// <summary>
        /// Return true if the give textPattern string is present within the detail-text of a display-box
        /// that is currently being displayed.
        /// The test is case-sensitive.
        /// </summary>
        /// <param name="textPattern">the pattern to check for</param>
        /// <returns>true if textPattern was within the detail-text</returns>
        public bool IsWithinDetailText( string textPattern )
        {
            bool result = false;
            foreach (var displayBox in this.InstancesCurrentlyDisplaying)
            {
                string detailText = displayBox.DetailText;
                if (detailText.Contains( textPattern ))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        #endregion

        #region WasWithinDetailText
        /// <summary>
        /// Return true if the give textPattern string was present within the detail-text of a display-box
        /// that has been displayed.
        /// The test is case-sensitive.
        /// </summary>
        /// <param name="textPattern">the pattern to check for</param>
        /// <returns>true if textPattern was present within the detail-text</returns>
        public bool WasWithinDetailText( string textPattern )
        {
            bool result = false;
            foreach (var displayBox in this.InstancesThatHaveDisplayed)
            {
                string detailText = displayBox.DetailText;
                if (detailText.Contains( textPattern ))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        #endregion

        // Following are the tests that detect the presence of any of the 6 distinct buttons that may be present.
        // These are suitable for verifying program-operation via automated unit-tests, as they use a different, direct
        // mechanism to detect whether the respective buttons are being displayed, as opposed to that mechanism by which
        // they are controlled at the API level.

        #region IsButtonPresentThatContains
        /// <summary>
        /// Return true if there is a display-box being shown to the user that has a button that contains the given text.
        /// The test is case-insensitive.
        /// </summary>
        /// <param name="withWhatText">the text-pattern to look for</param>
        /// <returns>true if there is at least one display-box currently displaying that contains it</returns>
        public bool IsButtonPresentThatContains( string withWhatText )
        {
            bool result = false;
            string patternAsLowercase = withWhatText.ToLower();
            foreach (var displayBox in this.InstancesCurrentlyDisplaying)
            {
                foreach (var button in displayBox.TheButtons)
                {
                    if (button.Visibility == Visibility.Visible)
                    {
                        //TODO: Alert - this will only work if all the buttons have Content that can be cast into a string.
                        // This is not an assumption that is always true.
                        string buttonText = button.Content as string;
                        if (buttonText != null)
                        {
                            string buttonTextAsLowercase = buttonText.ToLower();
                            if (buttonTextAsLowercase.Contains( patternAsLowercase ))
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region IsOkButtonPresent
        /// <summary>
        /// Return true if there is a DisplayBox being displayed that has the "OK" button showing.
        /// If the text of the "Ok" button has been changed, this will fail.
        /// </summary>
        public bool IsOkButtonPresent()
        {
            return IsButtonPresentThatContains( DisplayBoxWindow.TextOfOkButton );
        }
        #endregion

        #region IsYesButtonPresent
        /// <summary>
        /// Return true if there is a DisplayBox being displayed that has the "Yes" button showing.
        /// If the text of the "Yes" button has been changed, this will fail.
        /// </summary>
        public bool IsYesButtonPresent()
        {
            return IsButtonPresentThatContains( DisplayBoxWindow.TextOfYesButton );
        }
        #endregion

        #region IsNoButtonPresent
        /// <summary>
        /// Return true if there is a DisplayBox being displayed that has the "No" button showing.
        /// If the text of the "No" button has been changed, this will fail.
        /// </summary>
        public bool IsNoButtonPresent()
        {
            return IsButtonPresentThatContains( DisplayBoxWindow.TextOfNoButton );
        }
        #endregion

        #region IsCancelButtonPresent
        /// <summary>
        /// Return true if there is a DisplayBox being displayed that has the "Cancel" button showing.
        /// If the text of the "Cancel" button has been changed, this will fail.
        /// </summary>
        public bool IsCancelButtonPresent()
        {
            return IsButtonPresentThatContains( DisplayBoxWindow.TextOfCancelButton );
        }
        #endregion

        #region IsRetryButtonPresent
        /// <summary>
        /// Return true if there is a DisplayBox being displayed that has the "Retry" button showing.
        /// If the text of the "Retry" button has been changed, this will fail.
        /// </summary>
        public bool IsRetryButtonPresent()
        {
            return IsButtonPresentThatContains( DisplayBoxWindow.TextOfRetryButton );
        }
        #endregion

        #region IsCloseButtonPresent
        /// <summary>
        /// Return true if there is a DisplayBox being displayed that has the "Close" button showing.
        /// If the text of the "Close" button has been changed, this will fail.
        /// </summary>
        public bool IsCloseButtonPresent()
        {
            return IsButtonPresentThatContains( DisplayBoxWindow.TextOfCloseButton );
        }
        #endregion

        #region ButtonResultToSelect
        /// <summary>
        /// Get the DisplayUxResult that we want to emulate the user having selected. If none was specified - return null.
        /// </summary>
        public Hurst.LogNut.Util.DisplayUxResult? ButtonResultToSelect
        {
            get { return _buttonResultToSelect; }
        }
        #endregion

        #region InstancesCurrentlyDisplaying
        /// <summary>
        /// This is a collection of all of the display-boxes that are currently showing. This is intended only for running unit-tests.
        /// </summary>
        public IList<DisplayBoxWindow> InstancesCurrentlyDisplaying
        {
            get
            {
                if (_instancesCurrentlyBeingDisplayed == null)
                {
                    _instancesCurrentlyBeingDisplayed = new List<DisplayBoxWindow>();
                }
                return _instancesCurrentlyBeingDisplayed;
            }
        }
        #endregion

        #region InstancesThatHaveDisplayed
        /// <summary>
        /// This is a collection of all of the display-boxes that have been shown since the start of this current test-run.
        /// This is intended only for running unit-tests.
        /// </summary>
        public IList<DisplayBoxWindow> InstancesThatHaveDisplayed
        {
            get
            {
                if (_instancesThatHaveDisplayed == null)
                {
                    _instancesThatHaveDisplayed = new List<DisplayBoxWindow>();
                }
                return _instancesThatHaveDisplayed;
            }
        }
        #endregion

        #region IsTesting
        /// <summary>
        /// Get whether display-boxes are being used in automated-test mode.
        /// </summary>
        public bool IsTesting
        {
            get { return _isTesting; }
        }
        #endregion

        #region IsTimeoutsFast
        /// <summary>
        /// Get whether display-boxes are being used in automated-test mode and the timeouts are being limited to a brief interval.
        /// </summary>
        public bool IsTimeoutsFast
        {
            get { return _isTesting && _isTimeoutsFast; }
        }
        #endregion

        #region IsToWaitForExplicitClose
        /// <summary>
        /// Get or set whether, when in test-mode, instead of timing out - the display-box is to remain open
        /// until the method SimulateClose is called. This applies only in the case of asynchronous NotifyUser.. methods.
        /// Default is false.
        /// </summary>
        public bool IsToWaitForExplicitClose
        {
            get { return _isToWaitForExplicitClose; }
            set { _isToWaitForExplicitClose = value; }
        }

        /// <summary>
        /// Set whether instead of timing out - the display-box is to remain open until the method SimulateClose is called.
        /// This applies only in the case of asynchronous NotifyUser.. methods.  Default is false. 
        /// </summary>
        /// <param name="isToWait">true to wait until an explicit command is issued to close the display-box</param>
        /// <returns>this instance of DisplayBoxConfiguration, such that additional method-calls may be chained together</returns>
        public DisplayBoxTestFacility SetToWaitForExplicitClose( bool isToWait )
        {
            IsToWaitForExplicitClose = isToWait;
            return this;
        }
        #endregion

        #region Reset
        /// <summary>
        /// Clear the options back to their default values, and turns IsTesting and IsTimeoutsFast on.
        /// This is useful when setting up for automated unit-tests.
        /// </summary>
        /// <returns></returns>
        public DisplayBoxTestFacility Reset()
        {
            _buttonResultToSelect = null;
            _buttonTextToSelect = null;
            _isTesting = _isTimeoutsFast = true;
            _instancesCurrentlyBeingDisplayed = null;
            _instancesThatHaveDisplayed = null;
            return this;
        }
        #endregion

#if !PRE_4
        public ManualResetEventSlim ResetEventForWindowShowing
#else
        public ManualResetEvent ResetEventForWindowShowing
#endif
        {
            get
            {
                if (_resetEventForWindowShowing == null)
                {
#if !PRE_4
                    _resetEventForWindowShowing = new ManualResetEventSlim();
#else
                    _resetEventForWindowShowing = new ManualResetEvent(false);
#endif
                }
                return _resetEventForWindowShowing;
            }
        }

        #region SetButtonToSelect
        /// <summary>
        /// Set the display-box such that, when run as part of an automated test, we "mock" the user-interaction such that
        /// it acts as though the user selects the button that yields the given DisplayUxResult.
        /// </summary>
        /// <param name="buttonResult">This determines which button to emulate being pushed.</param>
        /// <returns>this instance of DisplayBoxConfiguration, such that additional method-calls may be chained together</returns>
        public DisplayBoxTestFacility SetButtonToSelect( DisplayUxResult buttonResult )
        {
            _buttonResultToSelect = buttonResult;
            return this;
        }
        #endregion

        #region SimulateClosing
        /// <summary>
        /// Cause the display-box window to immediately close with the given Result. Used for testing.
        /// </summary>
        /// <param name="withWhatResult">the DisplayUxResult to assign to the Result property upon closing</param>
        public void SimulateClosing( DisplayUxResult withWhatResult )
        {
            this._displayBoxManager.DisplayBoxWindow.Result = withWhatResult;
            this._displayBoxManager.DisplayBoxWindow.Close();
        }
        #endregion

        #region TextOfButtonToSelect
        /// <summary>
        /// Get the text of the button that we want to emulate the user having selected. If none was specified - return null.
        /// </summary>
        public string TextOfButtonToSelect
        {
            get
            {
                if (StringLib.HasNothing( _buttonTextToSelect ))
                {
                    return null;
                }
                else
                {
                    return _buttonTextToSelect;
                }
            }
        }
        #endregion

        #region SetButtonTextToSelect
        /// <summary>
        /// Set the display-box such that, when run as part of an automated test, we "mock" the user-interaction such that
        /// it acts as though the user selects the button with the given text on it. The text is case-insensitive.
        /// </summary>
        /// <param name="buttonText">this determines which button to emulate being pushed, by virtue of the text on it</param>
        /// <returns>this instance of DisplayBoxConfiguration, such that additional method-calls may be chained together</returns>
        public DisplayBoxTestFacility SetButtonTextToSelect( string buttonText )
        {
            if (buttonText == null)
            {
                throw new ArgumentNullException( nameof( buttonText ) );
            }
            _buttonTextToSelect = buttonText.ToLower();
            return this;
        }
        #endregion

        #region SetTestTimeoutsToBeFast
        /// <summary>
        /// For running automated unit-tests, this limits all timeouts to one second - so that you can see it, but it will still
        /// run reasonably fast. This should ONLY be used for running tests, not while your end-user is using your product. Default is true.
        /// If you set this to true, it also sets IsTesting to true.
        /// </summary>
        /// <param name="isToGoAtFullSpeed">true to enable special auto-test mode (with short timeouts), false for normal operation</param>
        public DisplayBoxTestFacility SetTestTimeoutsToBeFast( bool isToGoAtFullSpeed )
        {
            _isTimeoutsFast = isToGoAtFullSpeed;
            if (isToGoAtFullSpeed)
            {
                _isTesting = true;
            }
            return this;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Release any test-specific resources and turn off test-mode.
        /// </summary>
        public void Dispose()
        {
            if (_instancesCurrentlyBeingDisplayed != null)
            {
                _instancesCurrentlyBeingDisplayed.Clear();
                _instancesCurrentlyBeingDisplayed = null;
            }
            if (_instancesThatHaveDisplayed != null)
            {
                _instancesThatHaveDisplayed.Clear();
                _instancesThatHaveDisplayed = null;
            }
            if (_resetEventForWindowShowing != null)
            {
                _resetEventForWindowShowing.Dispose();
            }
            GC.SuppressFinalize( this );
        }
        #endregion

        #region fields

#if !PRE_4
        internal ManualResetEventSlim _resetEventForWindowShowing;
#else
        internal ManualResetEvent _resetEventForWindowShowing;
#endif
        /// <summary>
        /// This string represents the text of the button that we want to assume the user has clicked on when the display-box is shown,
        /// for use when running automated tests.
        /// </summary>
        private string _buttonTextToSelect;

        private DisplayUxResult? _buttonResultToSelect;

        /// <summary>
        /// This maintains a collection of the current DisplayBox instances, intended for running automated unit-tests
        /// - such that the test method can check for the presence and properties of "a" display-box.
        /// </summary>
        private static List<DisplayBoxWindow> _instancesCurrentlyBeingDisplayed;

        /// <summary>
        /// This maintains a collection of the DisplayBox instances that have been shown to the user,
        /// and then closed. This is intended for running automated unit-tests
        /// - such that the test method can check that display-box was shown and examine its properties.
        /// </summary>
        private static List<DisplayBoxWindow> _instancesThatHaveDisplayed;

        /// <summary>
        /// When true, this flag indicates that the calling program is running automated unit-tests.
        /// </summary>
        private bool _isTesting;

        /// <summary>
        /// This flag indicates that, when in test-mode, instead of timing out - the display-box is to remain open until the method SimulateClose is called.
        /// Default is false.
        /// </summary>
        private bool _isToWaitForExplicitClose;

        /// <summary>
        /// This flag dictates whether, when in test mode, all timeouts are limited to one second. Default is true - be fast.
        /// </summary>
        private bool _isTimeoutsFast;

        /// <summary>
        /// A reference back to the DisplayBox that is managing this.
        /// </summary>
        private DisplayBox _displayBoxManager;

        #endregion fields
    }
}
