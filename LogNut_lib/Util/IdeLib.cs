using System;
using System.ComponentModel;
using System.Diagnostics;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This class exists just to provide the IsInDesignMode property, to distinguish between running normally or within the Visual Studio IDE.
    /// </summary>
    public static class IdeLib
    {
        // See this excellent post by Fabio Augusto Pandolfo at this URL:
        // http://stackoverflow.com/questions/2427381/how-to-detect-that-c-sharp-windows-forms-code-is-executed-within-visual-studio

#if !PRE_4
        /// <summary>
        /// Get whether the program is currently executing within the Visual Studio IDE (that is, within a 'design surface' such as the VS Designer),
        /// as opposed to running normally.
        /// This is applicable to Windows Forms applications. For WPF we have other
        /// facilities.
        /// </summary>
        public static bool GetWhetherIsInDesignMode()
        {
            //CBL We should also incorporate the method IsInDesignMode from ViewModel.
            bool isInDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            string processName = "?";

            if (!isInDesignMode)
            {
                using (var process = Process.GetCurrentProcess())
                {
                    processName = process.ProcessName;
                    isInDesignMode = processName.ToLowerInvariant().Contains("devenv");
                }
            }

#if DEBUG_EXTRA
            if (logger != null)
            {
                string msg = String.Format("in IdeLib.GetWhetherIsInDesignMode, LicenseManager.UsageMode = {0}, processName = {1}", LicenseManager.UsageMode, processName);
                logger.LogInfo(msg);
                if (isInDesignMode)
                {
                    logger.LogInfo("  returning true  -- YES we are in design mode.");
                }
                else
                {
                    logger.LogInfo("  returning false  - NOT in design mode.");
                }
            }
#endif
            return isInDesignMode;
        }
#else
        /// <summary>
        /// Get whether the program is currently executing within the Visual Studio IDE (that is, within a 'design surface' such as the VS Designer),
        /// as opposed to running normally. This is applicable to Windows Forms applications. For WPF we have other facilities.
        /// </summary>
        public static bool GetWhetherIsInDesignMode()
        {
            bool isInDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            if (!isInDesignMode)
            {
                using (var process = Process.GetCurrentProcess())
                {
                    string processName = process.ProcessName;
                    isInDesignMode = processName.ToLowerInvariant().Contains("devenv");
                }
            }
            return isInDesignMode;
        }
#endif
    }
}
