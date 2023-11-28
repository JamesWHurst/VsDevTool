

namespace Hurst.LogNut.Util
{
    /// <summary>
    /// Interface IAccessibilityEvaluator exists to specify the method <c>CanWriteTo</c>,
    /// that needs to be accessed via an interface in order to provide unit-testability.
    /// </summary>
    /// <remarks>
    /// It is a bit more awkward and verbose to instantiate an IAccessibilityEvaluator, so this is kept
    /// deliberately sparse and only methods for which mocking is needed, are included here.
    /// </remarks>
    public interface IAccessibilityEvaluator
    {
        /// <summary>
        /// Test the given folder to see whether it exists (or can be created) and can be written to.
        /// If the folder does not exist, this creates it (if it can).
        /// </summary>
        /// <param name="folderPath">the directory to test for writeability</param>
        /// <param name="reason">If the test fails - a description of the failure is assigned to this. Otherwise this is set to null.</param>
        /// <returns>true if the folder can be written to</returns>
        /// <remarks>
        /// If the given <paramref name="folderPath"/> does not exist - this attempts to create it, and if that happens with no exceptions
        /// then it is considered writeable (this folder is not deleted afterward).
        /// </remarks>
        bool CanWriteTo( string folderPath, out string reason );
    }
}
