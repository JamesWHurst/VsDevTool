using Hurst.LogNut.Util;


namespace Hurst.BaseLibWpf
{
    #region Doc-stuff
    /// <summary>
    /// This namespace encompasses the base-level library of general-purpose C# stuff upon which the other libaries and applications depend.
    /// James W. Hurst
    /// </summary>
    public static class NamespaceDoc
    {
        // This class exists only to provide a hook for adding summary documentation
        // for the Hurst.BaseLib namespace.
    }
    #endregion

    #region interface IDesktopApplication
    /// <summary>
    /// Providing this interface helps contribute a sort of 'aspect-oriented programming' structure
    /// by interjecting an Interlocution and other subsystems across the board.
    /// </summary>
    public interface IDesktopApplication : IApp
    {
        /// <summary>
        /// Provide access to the IInterlocution object that provides us with the user-notification services.
        /// </summary>
        IInterlocution Interlocution { get; }

        /// <summary>
        /// Provide the name of this application as it would be displayed to the user (in other than abbreviated form).
        /// </summary>
        string ProductName { get; }

        /// <summary>
        /// Provide the name of this application in abbreviated form
        /// for display for such uses as dialog-window titlebars, logging output and the like.
        /// </summary>
        string ProductNameShort { get; }

        /// <summary>
        /// Get the string to use to identify this software product to the user,
        /// for use - for example - as a prefix in the titlebar for all windows and dialogs of this application,
        /// or for Windows Event Log entries.
        /// This implements the mandated standard which is the vendor followed by the application's name.
        /// To set the titlebar you would append a separator (such as a colon and a space) before your specific title information.
        /// </summary>
        string ProductIdentificationPrefix { get; }

        /// <summary>
        /// Get the pathname of this application's executable.
        /// </summary>
        string ThisApplicationExecutablePath { get; }

        /// <summary>
        /// Get the filesystem-path for storing this application's data, such as would be common across local users of this desktop computer.
        /// This path value contains the vendor-name and this product-name.
        /// </summary>
        string ThisApplicationLocalMachineDataFolder { get; }
    }
    #endregion interface IDesktopApplication
}
