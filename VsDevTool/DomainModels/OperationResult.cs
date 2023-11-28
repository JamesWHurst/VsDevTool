

namespace VsDevTool.DomainModels
{
    /// <summary>
    /// Class OperationResult holds the result of something that was performed against all applicable projects -
    /// including NumberThatWereProcessed, NumberThatWereSuccessful, NumberOfFailures, Reason, and WasAllSuccessful.
    /// </summary>
    public class OperationResult
    {
        public bool WasAllSuccessful { get; set; }

        public string Reason { get; set; }

        public int NumberThatWereProcessed { get; set; }

        public int NumberThatWereSuccessful { get; set; }

        public int NumberOfFailures
        {
            get { return NumberThatWereProcessed - NumberThatWereSuccessful; }
        }
    }
}
