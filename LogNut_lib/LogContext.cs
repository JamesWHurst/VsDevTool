using System;


namespace Hurst.LogNut
{
    /// <summary>
    /// LogContext exists simply to provide a way to contain a 'context' for all logging for a program or some operation,
    /// such that all settings and values are reset and resources disposed of upon exiting a 'using' block.
    /// </summary>
    public class LogContext : IDisposable
    {
        /// <summary>
        /// Release any managed resources held by the LogNut objects.
        /// This currently simply calls LogManager.Clear.
        /// </summary>
        public void Dispose()
        {
            LogManager.Clear();
        }
    }
}
