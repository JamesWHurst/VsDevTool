using System;
using System.Text;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This EventArgs subclass carries information that indicates an error occurred.
    /// </summary>
    public class ErrorOccurredEventArgs : EventArgs
    {
        #region constructors
        /// <summary>
        /// Create a new ErrorOccurredEventArgs object.
        /// </summary>
        /// <param name="errorMessageToUser">a summary description of the error to present to the user</param>
        /// <param name="messageToDeveloper">details of the error to present to the developer (may be null)</param>
        public ErrorOccurredEventArgs( string errorMessageToUser, string messageToDeveloper )
        {
            MessageToUser = errorMessageToUser;
            MessageToDeveloper = messageToDeveloper;
        }

        /// <summary>
        /// Create a new ErrorOccurredEventArgs object.
        /// </summary>
        /// <param name="errorMessageToUser">a summary description of the error to present to the user</param>
        /// <param name="messageToDeveloper">details of the error to present to the developer (may be null)</param>
        /// <param name="isFatal">set this true to signal that this error should be considered as fatal and merits cessation of the program's operation</param>
        public ErrorOccurredEventArgs( string errorMessageToUser, string messageToDeveloper, bool isFatal )
        {
            IsFatal = isFatal;
            MessageToUser = errorMessageToUser;
            MessageToDeveloper = messageToDeveloper;
        }

        /// <summary>
        /// Create a new ErrorOccurredEventArgs object.
        /// </summary>
        /// <param name="errorMessageToUser">a summary description of the error to present to the user</param>
        /// <param name="messageToDeveloper">details of the error to present to the developer (may be null)</param>
        /// <param name="isFatal">set this true to signal that this error should be considered as fatal and merits cessation of the program's operation</param>
        /// <param name="uxElement">the name (at the GUI-design-tool level) of the UX-visual-element to set focus to</param>
        public ErrorOccurredEventArgs( string errorMessageToUser, string messageToDeveloper, bool isFatal, string uxElement )
        {
            IsFatal = isFatal;
            MessageToUser = errorMessageToUser;
            MessageToDeveloper = messageToDeveloper;
            UxElementName = uxElement;
        }

        /// <summary>
        /// Create a new ErrorOccurredEventArgs object with the given message for the end-user, and no message specifically for the developer.
        /// </summary>
        /// <param name="errorMessageToUser">a summary description of the error to present to the user</param>
        public ErrorOccurredEventArgs( string errorMessageToUser )
        {
            MessageToUser = errorMessageToUser;
            MessageToDeveloper = null;
        }

        /// <summary>
        /// Create a new ErrorOccurredEventArgs object with the given message for the end-user, and no message specifically for the developer.
        /// </summary>
        /// <param name="errorMessageToUser">a summary description of the error to present to the user</param>
        /// <param name="isFatal">set this true to signal that this error should be considered as fatal and merits cessation of the program's operation</param>
        public ErrorOccurredEventArgs( string errorMessageToUser, bool isFatal )
        {
            IsFatal = isFatal;
            MessageToUser = errorMessageToUser;
            MessageToDeveloper = null;
        }

        /// <summary>
        /// Create a new ErrorOccurredEventArgs object with the given message for the end-user, and no message specifically for the developer.
        /// </summary>
        /// <param name="exception">an Exception that this error is concerning</param>
        /// <param name="errorMessageToUser">a summary description of the error to present to the user</param>
        public ErrorOccurredEventArgs( Exception exception, string errorMessageToUser )
        {
            Exception = exception;
            MessageToUser = errorMessageToUser;
            MessageToDeveloper = null;
        }

        /// <summary>
        /// Create a new ErrorOccurredEventArgs object with the given message for the end-user, and no message specifically for the developer.
        /// </summary>
        /// <param name="exception">an Exception that this error is concerning</param>
        /// <param name="errorMessageToUser">a summary description of the error to present to the user</param>
        /// <param name="isFatal">set this true to signal that this error should be considered as fatal and merits cessation of the program's operation</param>
        public ErrorOccurredEventArgs( Exception exception, string errorMessageToUser, bool isFatal )
        {
            IsFatal = isFatal;
            Exception = exception;
            MessageToUser = errorMessageToUser;
            MessageToDeveloper = null;
        }
        #endregion

        /// <summary>
        /// Get the Exception that this object holds (may be null if none apply).
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Get or set the text of the message that this request proposes to show the user.
        /// </summary>
        public string MessageToUser { get; set; }

        /// <summary>
        /// Get or set the description of the error to present to the developer.
        /// This would include more technical information that the user probably doesn't want to see.
        /// </summary>
        public string MessageToDeveloper { get; set; }

        /// <summary>
        /// Get or set whether this particular error should be considered "fatal", in that
        /// cessation of the program operation is merited.
        /// </summary>
        public bool IsFatal { get; set; }

        /// <summary>
        /// Get or set the name (at the GUI level) of the UX visual elment in question.
        /// This can be useful if, for example, the UX needs to highlight that field.
        /// Leave this null if it does not apply.
        /// </summary>
        public string UxElementName { get; set; }

        /// <summary>
        /// Override the ToString method to provide useful information.
        /// </summary>
        /// <returns>a string denoting the contents of this object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "ErrorOccurredEventArgs(" );
            if (IsFatal)
            {
                sb.Append( "IsFatal, " );
            }
            sb.Append( "MessageToUser=" ).Append( StringLib.AsString( MessageToUser ) );
            if (StringLib.HasSomething( MessageToDeveloper ))
            {
                sb.Append( ",MessageToDeveloper=" );
                sb.Append( MessageToDeveloper );
            }
            sb.Append( ")" );
            return sb.ToString();
        }
    }
}
