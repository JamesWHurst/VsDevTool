

namespace Hurst.LogNut.Util
{
    #region interface IVersionable
    /// <summary>
    /// An application that implements IVersionable provides a method for getting it's program-version.
    /// This provides an aspect-oriented way for your applic to supply it's version,
    /// and for low-level library routines to get it without having to reference your assembly.
    /// </summary>
    public interface IVersionable
    {
        /// <summary>
        /// Return this application's version, as a simple string.
        /// </summary>
        string ProgramVersionText { get; }
    }
    #endregion
}
