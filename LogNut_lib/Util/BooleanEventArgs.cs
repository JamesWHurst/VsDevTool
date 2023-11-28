using System;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This subclass of EventArgs provides a Boolean state information.
    /// </summary>
    public class BooleanEventArgs : EventArgs
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="booleanState">the boolean state-information that is to be contained within this EventArgs-derived class</param>
        public BooleanEventArgs( bool booleanState )
            : base()
        {
            this.State = booleanState;
        }

        /// <summary>
        /// Get or set the boolean state-information.
        /// </summary>
        public bool State { get; set; }

        /// <summary>
        /// Override the ToString method to provide useful information.
        /// </summary>
        /// <returns>a string denoting the state of this object</returns>
        public override string ToString()
        {
            return "BooleanEventArgs(" + this.State + ")";
        }
    }
}
