

namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This interface is intended as a platform-neutral desktop application
    /// that has a simplified way to get it's name and version, and whether it is in design-mode.
    /// </summary>
    public interface IApp
    {
        /// <summary>
        /// Return the name of the currently-executing program. If this is an Application that implements IDesktopApplication,
        /// then use it's ProductNameShort function, otherwise get it from the assembly. This applies to .Net or Silverlight.
        /// </summary>
        /// <returns>The program (assembly)-name as a string, or String.Empty if unable to determine it</returns>
        string ProgramName { get; }

        /// <summary>
        /// This indicates whether this code is being executed in the context of Visual Studio "Cider" (the visual designer) or Blend,
        /// as opposed to running normally as an application.
        /// </summary>
        bool IsInDesignMode { get; }

        /// <summary>
        /// Get the filesystem-path for storing this application's data - that is specific to the current, non-roaming user.
        /// This path value contains the vendor-name and this product-name.
        /// </summary>
        string ThisApplicationCurrentUserDataFolder { get; }

        /// <summary>
        /// Get the username of the user who is currently running this program.
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// Get the (short, one-word version of) name of the maker or owner of this software,
        /// as would be used for the first part of the CommonDataPath, and the window title-bar prefix.
        /// </summary>
        string VendorName { get; }

        /// <summary>
        /// Return this application's version, as a simple string.
        /// </summary>
        string ProgramVersionText { get; }
    }
}
