using System;
using System.Text;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This class is used to indicate a request to close the currently-running application.
    /// </summary>
    public class CloseApplicationRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new instance of <c>CloseApplicationRequestEventArgs</c> with the given information.
        /// </summary>
        /// <param name="reason">the English-text reason for the application-close request, if applicable (may be null)</param>
        /// <param name="isDueToError">this indicates whether this application-close-request is due to some kind of fault-condition</param>
        public CloseApplicationRequestEventArgs( string reason, bool isDueToError )
            : base()
        {
            this.Reason = reason;
            this.IsDueToError = isDueToError;
        }

        /// <summary>
        /// Create a new instance of <c>CloseApplicationRequestEventArgs</c> with an empty string for the <c>Reason</c>,
        /// and a value of false for <c>IsDueToError</c>.
        /// </summary>
        public CloseApplicationRequestEventArgs()
            : base()
        {
            this.Reason = String.Empty;
        }

        /// <summary>
        /// Get or set whether this application-close-request was because of some kind of error or fault-condition.
        /// </summary>
        public bool IsDueToError
        {
            get { return _isDueToError; }
            set { _isDueToError = value; }
        }

        /// <summary>
        /// Get or set the English-text reason for the application-close request (if applicable).
        /// </summary>
        public string Reason
        {
            get { return _reason; }
            set { _reason = value; }
        }

        /// <summary>
        /// Override the ToString method in order to provide a useful description of this object state.
        /// </summary>
        /// <returns>a concise denotation of the state of this object</returns>
        public override string ToString()
        {
            bool hasContent = false;
            var sb = new StringBuilder( "CloseApplicationRequestEventArgs(" );
            if (IsDueToError)
            {
                sb.Append( "IsDueToError" );
                hasContent = true;
            }
            if (StringLib.HasSomething( Reason ))
            {
                if (hasContent)
                {
                    sb.Append( ", " );
                }
                sb.Append( @"Reason = """ ).Append( Reason ).Append( @"""" );
            }
            sb.Append( ")" );
            return sb.ToString();
        }

        private bool _isDueToError;
        private string _reason;
    }
}
