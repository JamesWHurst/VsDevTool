#if PRE_4
#define PRE_5
#endif
using System;
using Hurst.LogNut.Util.Annotations;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This bit-field enumeration-type specifies the combination of buttons to be shown.
    /// </summary>
    [Flags]
    public enum DisplayBoxButtons
    {
        /// <summary>
        /// Provide no buttons
        /// </summary>
        None = 0,
        /// <summary>
        /// Provide an "Ok" button
        /// </summary>
        Ok = 0x0001,
        /// <summary>
        /// Provide a "Yes" button
        /// </summary>
        Yes = 0x0002,
        /// <summary>
        /// Provide a "No" button
        /// </summary>
        No = 0x0004,
        /// <summary>
        /// Provide a "Cancel" button
        /// </summary>
        Cancel = 0x0008,
        /// <summary>
        /// Provide a "Retry" button
        /// </summary>
        Retry = 0x0010,
        /// <summary>
        /// Provide a "Close" button
        /// </summary>
        Close = 0x0020,
        /// <summary>
        /// Provide an "Ignore" button
        /// </summary>
        Ignore = 0x0040
    }

    /// <summary>
    /// This is a more abstract alternative to using the MessageBoxIcon to set the default graphics and audio for the display-box.
    /// </summary>
    public enum DisplayBoxType
    {
        /// <summary>
        /// Do not display any icon at all
        /// </summary>
        None,
        /// <summary>
        /// Display the icon that represents "information"
        /// </summary>
        Information,
        /// <summary>
        /// Display a question-mark icon
        /// </summary>
        Question,
        /// <summary>
        /// Display a warning icon
        /// </summary>
        Warning,
        /// <summary>
        /// Display an icon that indicates the user has made a mistake
        /// </summary>
        UserMistake,
        /// <summary>
        /// Display an error icon
        /// </summary>
        Error,
        /// <summary>
        /// Display an icon that represents a blocking security-related issue
        /// </summary>
        SecurityIssue,
        /// <summary>
        /// Display an icon that represents a successful security-related function result
        /// </summary>
        SecuritySuccess,
        /// <summary>
        /// Display a stop-sign
        /// </summary>
        Stop
    }

    /// <summary>
    /// This is the "result" that indicates what the user's choice was
    /// in response to a dialog-window.
    /// </summary>
    /// <remarks>
    /// These follow the TaskDialogResult values, except for the TimedOut value.
    /// </remarks>
    public enum DisplayUxResult
    {
        /// <summary>
        /// The user has clicked on "Ok", or some equivalent result
        /// </summary>
        Ok = 1,
        /// <summary>
        /// The displaybox result is to cancel
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// The displaybox result is asking to retry it
        /// </summary>
        Retry = 4,
        /// <summary>
        /// The displaybox result is "Yes"
        /// </summary>
        Yes = 6,
        /// <summary>
        /// The displaybox result is "No"
        /// </summary>
        No = 7,
        /// <summary>
        /// The displaybox was simply closed
        /// </summary>
        Close = 8,
        /// <summary>
        /// The displaybox result is to "abort"
        /// </summary>
        Abort,
        /// <summary>
        /// The displaybox result is "Ignore it"
        /// </summary>
        Ignore,
        /// <summary>
        /// The displaybox timed out
        /// </summary>
        TimedOut
    }

    /// <summary>
    /// This interface specifies the defeault user and developer conversation facility that any IDesktopApplication
    /// is expected to provide, including MessageBox/DisplayBox dialog windows and logging.
    /// </summary>
    public interface IInterlocution
    {
        #region NotifyUser methods

        //CBL  If these have only an OK button, does it make sense to return a result?
        //     The only reason I can see is to see whether it timed out, or the user clicked on the Ok button.

#if FALSE
        /// <summary>
        /// Display a display-box to the user, based upon the given options.
        /// </summary>
        /// <param name="allOptions">a DisplayBoxConfiguration object that fully specifies everything about the display-box</param>
        /// <returns>a DisplayUxResult indicating which action the user took, or TimedOut if the user took no action before the timeout expired</returns>
        DisplayUxResult NotifyUser(DisplayBoxConfiguration allOptions);
#endif

        /// <summary>
        /// Display a display-box to the user. Wait for his response or else close itself after the timeout has expired.
        /// The message-type is assumed to be DisplayBoxType.Information
        /// </summary>
        /// <param name="summaryText">The text message to display to the user</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        /// <returns>A DisplayUxResult that indicates the user's response</returns>
#if !PRE_4
        DisplayUxResult NotifyUser(string summaryText, Object parent = null);
#else
        DisplayUxResult NotifyUser(string summaryText, Object parent);
#endif

        /// <summary>
        /// Display a display-box to the user. Wait for his response or else close itself after the timeout has expired.
        /// The message-type is assumed to be DisplayBoxType.Information
        /// </summary>
        /// <param name="summaryText">the summary text to show in the upper area</param>
        /// <param name="detailText">the detail text to show in the lower area</param>
        /// <param name="caption">what to show in the titlebar of this display-box, after the standard prefix (optional)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        /// <returns>a DisplayUxResult that signals what the user clicked, or whether it timed-out</returns>
#if !PRE_4
        DisplayUxResult NotifyUser(string summaryText, string detailText, string caption = null, Object parent = null);
#else
        DisplayUxResult NotifyUser(string summaryText, string detailText, string caption, Object parent);
#endif

        /// <summary>
        /// Display a display-box to the user. Wait for his response or else close itself after the timeout has expired.
        /// This is the overload that has all of the options, which the other methods call.
        /// </summary>
        /// <param name="summaryText">the summary text to show in the upper area</param>
        /// <param name="detailText">the detail text to show in the lower area</param>
        /// <param name="buttons">which buttons to show</param>
        /// <param name="displayType">which basic type of message this is</param>
        /// <param name="caption">what to show in the titlebar of this display-box, after the standard prefix (this may be null)</param>
        /// <param name="isTopmostWindow">whether to make the display-box the top-most window on the user's desktop</param>
        /// <param name="timeout">the maximum time to show it, in seconds (set this to zero to get the default)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box</param>
        /// <returns>a DisplayUxResult indicating which action the user took, or TimedOut if the user took no action before the timeout expired</returns>
        DisplayUxResult NotifyUser(string summaryText,
                                   string detailText,
                                   DisplayBoxButtons buttons,
                                   DisplayBoxType displayType,
                                   string caption,
                                   bool? isTopmostWindow,
                                   int timeout,
                                   Object parent);

#if !PRE_4
        /// <summary>
        /// Display a display-box to the user to inform of an end-user mistake. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="summaryText">a brief announcement of the mistake</param>
        /// <param name="detailText">(optional) additional details that might be of use to the end-user</param>
        /// <param name="caption">what to show in the titlebar of this display-box, after the standard prefix (this may be null)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        /// <returns>a DisplayUxResult indicating which action the user took, or TimedOut if the user took no action before the timeout expired</returns>
        DisplayUxResult NotifyUserOfMistake(string summaryText, string detailText = null, string caption = null, Object parent = null);

        /// <summary>
        /// Display a display-box to the user to inform of an end-user mistake. Wait for his response or else close itself after the timeout has expired.
        /// This overload eschews any detail-text, and uses the default caption and parent.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to display</param>
        /// <returns>a DisplayUxResult indicating which action the user took, or TimedOut if the user took no action before the timeout expired</returns>
        [StringFormatMethod("format")]
        DisplayUxResult NotifyUserOfMistake(string format, params object[] args);
#else
        /// <summary>
        /// Display a display-box to the user to inform of an end-user mistake. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="summaryText">the summary text to show in the upper area</param>
        /// <param name="detailText">the detail text to show in the lower area</param>
        /// <param name="caption">what to show in the titlebar of this display-box, after the standard prefix (this may be null)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box</param>
        /// <returns>a DisplayUxResult indicating which action the user took, or TimedOut if the user took no action before the timeout expired</returns>
        DisplayUxResult NotifyUserOfMistake( string summaryText, string detailText, string caption, Object parent );
#endif

        //TODO: How to handle the toDeveloper part?

#if !PRE_4
        /// <summary>
        /// Display a display-box to the user to inform of an error. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="toUser">the enduser-oriented message to show</param>
        /// <param name="toDeveloper">the developer-oriented message to show</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        void NotifyUserOfError(string toUser, string toDeveloper = null, Object parent = null);
#else
        /// <summary>
        /// Display a display-box to the user to inform of an error. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="toUser">the enduser-oriented message to show</param>
        /// <param name="toDeveloper">the developer-oriented message to show</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (may be null)</param>
        void NotifyUserOfError(string toUser, string toDeveloper, Object parent);
#endif

#if !PRE_4
        /// <summary>
        /// Display a display-box to the user to inform of an error. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="toUser">the enduser-oriented message to show</param>
        /// <param name="toDeveloper">the developer-oriented message to show</param>
        /// <param name="exception">an Exception to describe to the user</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        void NotifyUserOfError(string toUser, string toDeveloper, Exception exception, Object parent = null);
#else
        /// <summary>
        /// Display a display-box to the user to inform of an error. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="toUser">the enduser-oriented message to show</param>
        /// <param name="toDeveloper">the developer-oriented message to show</param>
        /// <param name="exception">an Exception to describe to the user</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (may be null)</param>
        void NotifyUserOfError(string toUser, string toDeveloper, Exception exception, Object parent);
#endif

        /// <summary>
        /// Display a display-box to the user to inform of an error. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="exception">an Exception to describe to the user</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional - may be null)</param>
#if !PRE_4
        void NotifyUserOfError(Exception exception, Object parent = null);
#else
        void NotifyUserOfError(Exception exception, Object parent);
#endif

#if !PRE_4
        /// <summary>
        /// Display a display-box to the user to inform of an error. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="toUser">the enduser-oriented message to show</param>
        /// <param name="toDeveloper">the developer-oriented message to show</param>
        /// <param name="captionAfterPrefix">what to show in the titlebar of this display-box, after the standard prefix (this may be null)</param>
        /// <param name="exception">an Exception to describe to the user</param>
        /// <param name="timeout">the maximum time to show it, in seconds. Set this to zero to get the default. (optional - default is 0)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        void NotifyUserOfError(string toUser, string toDeveloper, string captionAfterPrefix, Exception exception, int timeout = 0, Object parent = null);
#else
        /// <summary>
        /// Display a display-box to the user to inform of an error. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="toUser">the enduser-oriented message to show</param>
        /// <param name="toDeveloper">the developer-oriented message to show</param>
        /// <param name="captionAfterPrefix">what to show in the titlebar of this display-box, after the standard prefix (this may be null)</param>
        /// <param name="exception">an Exception to describe to the user</param>
        /// <param name="timeout">the maximum time to show it, in seconds (set this to zero to get the default)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (may be null)</param>
        void NotifyUserOfError(string toUser, string toDeveloper, string captionAfterPrefix, Exception exception, int timeout, Object parent);
#endif

        /// <summary>
        /// Display a display-box to the user asking a Yes-or-No question. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="question">the text of the question to pose to the user, which will go into the summary-text area of the display-box</param>
        /// <param name="defaultAnswer">this is the user-response to assume if the display-box is closed via its system-menu or times-out</param>
        /// <param name="detailText">additional text that can go into the detail-text area (optional)</param>
        /// <param name="caption">text to add to the normal default title-bar prefix that will appear as the 'caption' for this display-box (optional)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        /// <returns>a DisplayUxResult indicating which action the user took, or TimedOut if the user took no action before the timeout expired</returns>
#if !PRE_4
        DisplayUxResult AskYesOrNo(string question,
                                   DisplayUxResult defaultAnswer,
                                   string detailText = null,
                                   string caption = null,
                                   Object parent = null);
#else
        DisplayUxResult AskYesOrNo(string question,
                                  DisplayUxResult defaultAnswer,
                                  string detailText,
                                  string caption,
                                  Object parent);
#endif

        #region asynchronous versions

#if !PRE_4
        /// <summary>
        /// Display a display-box to the user as a non-modal dialog window, and return immediately.
        /// The message-type is assumed to be DisplayBoxType.Information
        /// </summary>
        /// <param name="summaryText">The text message to display to the user</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        void NotifyUserAsync(string summaryText, Object parent = null);

        /// <summary>
        /// Display a display-box to the user as a non-modal dialog window, and return immediately.
        /// The message-type is assumed to be DisplayBoxType.Information
        /// </summary>
        /// <param name="summaryText">the summary text to show in the upper area</param>
        /// <param name="detailText">the detail text to show in the lower area</param>
        /// <param name="caption">what to show in the titlebar of this display-box, after the standard prefix (optional)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        void NotifyUserAsync(string summaryText, string detailText, string caption = null, Object parent = null);

        /// <summary>
        /// Display a display-box to the user. Wait for his response or else close itself after the timeout has expired.
        /// This is the overload that has all of the options, which the other methods call.
        /// </summary>
        /// <param name="summaryText">the summary text to show in the upper area</param>
        /// <param name="detailText">the detail text to show in the lower area</param>
        /// <param name="buttons">which buttons to show</param>
        /// <param name="displayType">the basic type of display-box to show (optional - default is Information)</param>
        /// <param name="caption">what to show in the titlebar of this display-box, after the standard prefix (optional)</param>
        /// <param name="isTopmostWindow">whether to force this display-box to be over top of all other windows (optional)</param>
        /// <param name="timeoutInSeconds">the maximum time to show it, in seconds (optional)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        void NotifyUserAsync(string summaryText,
                             string detailText,
                             DisplayBoxButtons buttons = DisplayBoxButtons.Ok,
                             DisplayBoxType displayType = DisplayBoxType.Information,
                             string caption = null,
                             bool? isTopmostWindow = null,
                             int timeoutInSeconds = 0,
                             Object parent = null);
#endif
        #endregion asynchronous versions

        #endregion NotifyUser methods

        #region WarnUser methods

        /// <summary>
        /// Display a display-box to the user, with a warning icon. Wait for his response or else close itself after the timeout has expired.
        /// The message-type is assumed to be DisplayBoxType.Information
        /// </summary>
        /// <param name="summaryText">The text message to display to the user</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional - default is Warning)</param>
        /// <returns>A DisplayUxResult that indicates the user's response</returns>
#if !PRE_4
        DisplayUxResult WarnUser(string summaryText, Object parent = null);
#else
        DisplayUxResult WarnUser(string summaryText, Object parent);
#endif

        /// <summary>
        /// Display a display-box to the user with a warning icon. Wait for his response or else close itself after the timeout has expired.
        /// The message-type is assumed to be DisplayBoxType.Information
        /// </summary>
        /// <param name="summaryText">the summary text to show in the upper area</param>
        /// <param name="detailText">the detail text to show in the lower area</param>
        /// <param name="captionAfterPrefix">what to show in the titlebar of this display-box, after the standard prefix (optional - default is Warning)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        /// <returns>a DisplayUxResult that signals what the user clicked, or whether it timed-out</returns>
#if !PRE_4
        DisplayUxResult WarnUser(string summaryText, string detailText, string captionAfterPrefix = null, Object parent = null);
#else
        DisplayUxResult WarnUser(string summaryText, string detailText, string captionAfterPrefix, Object parent);
#endif

        /// <summary>
        /// Display a display-box to the user, with a warning icon, asking a Yes-or-No question. Wait for his response or else close itself after the timeout has expired.
        /// </summary>
        /// <param name="question">the text of the question to pose to the user, which will go into the summary-text area of the display-box</param>
        /// <param name="defaultAnswer">this is the user-response to assume if the display-box is closed via its system-menu or times-out</param>
        /// <param name="detailText">additional text that can go into the detail-text area (optional)</param>
        /// <param name="caption">text to add to the normal default title-bar prefix that will appear as the 'caption' for this display-box (optional - default is Warning:)</param>
        /// <param name="parent">the visual-element to consider as the parent, or owner, of this display-box (optional)</param>
        /// <returns>a DisplayUxResult indicating which action the user took, or TimedOut if the user took no action before the timeout expired</returns>
        DisplayUxResult WarnUserAndAskYesOrNo(string question,
                                            DisplayUxResult defaultAnswer,
#if !PRE_4
 string detailText = null,
                                            string caption = null,
                                            Object parent = null);
#else
 string detailText,
                                            string caption,
                                            Object parent);
#endif

        #endregion WarnUser methods
    }
}
