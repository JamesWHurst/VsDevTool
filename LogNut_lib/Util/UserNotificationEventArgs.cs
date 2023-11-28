using System;
using System.Text;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This subclass of EventArgs is used to convey information intended for
    /// notifying the end-user of something.
    /// </summary>
    public class UserNotificationEventArgs : EventArgs
    {
        #region constructors
        /// <summary>
        /// Create a new UserNotificationEventArgs instance.
        /// </summary>
        /// <param name="message">the text of the message to show the user</param>
        /// <param name="isWarning">this indicates whether this is a warning-message</param>
        /// <param name="isError">this indicates whether this is an error-message</param>
        public UserNotificationEventArgs( string message, bool isWarning, bool isError )
        {
            this.MessageToUser = message;
            this.IsWarning = isWarning;
            this.IsError = isError;
        }

        /// <summary>
        /// Create a new UserNotificationEventArgs instance.
        /// </summary>
        /// <param name="message">the text of the message to show the user</param>
        /// <param name="isWarning">this indicates whether this is a warning-message</param>
        /// <param name="isError">this indicates whether this is an error-message</param>
        /// <param name="isUserMistake">this indicates whether this is an error-message</param>
        public UserNotificationEventArgs(string message, bool isWarning, bool isError, bool isUserMistake)
        {
            this.MessageToUser = message;
            this.IsWarning = isWarning;
            this.IsError = isError;
            this.IsUserMistake = isUserMistake;
        }

        /// <summary>
        /// Create a new UserNotificationEventArgs instance.
        /// </summary>
        /// <param name="message">the text of the message to show the user</param>
        /// <param name="isWarning">this indicates whether this is a warning-message</param>
        /// <param name="isError">this indicates whether this is an error-message</param>
        /// <param name="isUserMistake">this indicates whether this is an error-message</param>
        /// <param name="uxElement">the name of the GUI form-field that this concerns</param>
        public UserNotificationEventArgs(string message, bool isWarning, bool isError, bool isUserMistake, string uxElement)
        {
            this.MessageToUser = message;
            this.IsWarning = isWarning;
            this.IsError = isError;
            this.IsUserMistake = isUserMistake;
            this.UxElementName = uxElement;
        }
        #endregion constructors

        /// <summary>
        /// Get or set where this request for user-notification involves an error-condition.
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// Get or set where this request for user-notification involves a condition that warrants a warning.
        /// </summary>
        public bool IsWarning { get; set; }

        /// <summary>
        /// Get or set whether this request for user-notification reflects a mistake on the part of the end-user.
        /// Default is false.
        /// </summary>
        public bool IsUserMistake { get; set; }

        /// <summary>
        /// Get or set the text of the message that this request proposes to show the user.
        /// </summary>
        public string MessageToUser { get; set; }

        /// <summary>
        /// Get or set the name (at the GUI level) of the UX visual elment in question.
        /// This can be useful if, for example, the UX needs to highlight that field.
        /// Leave this null if it does not apply.
        /// </summary>
        public string UxElementName { get; set; }

        #region ToString
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the state of current object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "UserNotificationEventArgs(" );
            sb.Append( "message=\"" ).Append( StringLib.AsString( this.MessageToUser ) ).Append( "\"" );
            if (IsWarning)
            {
                sb.Append( ", IsWarning" );
            }
            if (IsError)
            {
                sb.Append( ", IsError" );
            }
            if (IsUserMistake)
            {
                sb.Append( ", IsUserMistake" );
            }
            sb.Append( ")" );
            return sb.ToString();
        }
        #endregion
    }
}
