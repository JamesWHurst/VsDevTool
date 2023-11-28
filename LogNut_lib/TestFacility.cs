using System;
using System.Collections.Generic;
using System.Linq;
using Hurst.LogNut.Util;


namespace Hurst.LogNut
{
    /// <summary>
    /// This class is intended to contain most of the unit-testing facilities provided by LogNut.
    /// </summary>
    public class TestFacility : IDisposable
    {
        #region constructor
        /// <summary>
        /// Create a new TestFacility object.
        /// </summary>
        public TestFacility()
        {
            this._isTesting = true;
            this._isCounting = true;
        }
        #endregion

        //TODO:
        // add:
        // level, expectedMessage, start-time, end-time
        // level, expectedMessage, start-time, timeout

        #region properties

        #region IsCounting
        /// <summary>
        /// Gets or sets a flag that indicates whether to maintain a count of log-records as they are sent, as when needed for verifying
        /// that a certain number of log outputs were made during a unit-test. Default is true.
        /// </summary>
        public bool IsCounting
        {
            get { return _isCounting; }
            set { _isCounting = value; }
        }
        #endregion

        #region IsSimulatingOutput
        /// <summary>
        /// Get or set whether to just simulate the sending of log output, saving the log-records in a list for testing,
        /// as opposed to actually sending the log out. Default is false.
        /// </summary>
        public bool IsSimulatingOutput
        {
            get { return _isSimulatingOutput; }
            set { _isSimulatingOutput = value; }
        }

        /// <summary>
        /// Set to just simulate the sending of the log output, saving the log-records in a list for testing,
        /// as opposed to actually sending the log out.
        /// </summary>
        /// <returns>a reference to this same TestFacility object, in case you wish to chain method-calls</returns>
        public TestFacility SimulateOutput()
        {
            _isSimulatingOutput = true;
            return this;
        }
        #endregion

        #region IsTesting
        /// <summary>
        /// Gets a value that indicates whether the object that contains this TestFacility object
        /// is in automated-test mode, as opposed to normal mode. Default is false.
        /// To test for this, you would normally first check for the reference to TestFacility being null.
        /// </summary>
        public bool IsTesting
        {
            get { return _isTesting; }
        }
        #endregion

        #region MaxTestCount
        /// <summary>
        /// Get the maximum number of log-records to track for test-counting purposes. Default is 20.
        /// </summary>
        public int MaxTestCount
        {
            get { return _maxTestCount; }
            set { _maxTestCount = value; }
        }
        #endregion

        #region RecordsSinceLastReset
        /// <summary>
        /// Get the list of log-records sent since the program-under-test started, or since the last call to ResetTest.
        /// This list is only maintained if IsCounting is on.
        /// </summary>
        public IList<LogRecord> RecordsSinceLastReset
        {
            get
            {
                if (_recordsSinceLastReset == null)
                {
                    _recordsSinceLastReset = new List<LogRecord>();
                }
                return _recordsSinceLastReset;
            }
        }
        #endregion

        #region properties to apply to a record just for testing

        /// <summary>
        /// Get or set the value to set IsInDesignMode to, whenever we are in test mode and this property is non-null.
        /// </summary>
        public bool? DesignModeToApply { get; set; }

        /// <summary>
        /// Get or set the string value to assign to the log-record Id property, whenever we are in test-mode.
        /// The default is null - which means NOT to assign any value to it.
        /// </summary>
        public string IdToApply { get; set; }

        #region TimestampToApply
        /// <summary>
        /// Get or set the value to set the timestamp to whenever we are in test-mode and this has been set.
        /// </summary>
        /// <remarks>
        /// We determine whether this has been set by seeing whether it's value is other than default(DateTime).
        /// </remarks>
        public DateTime TimestampToApply
        {
            get { return _timeStampToApply; }
            set { _timeStampToApply = value; }
        }
        #endregion

        /// <summary>
        /// Get or set the value to set the Username to, whenever we are in test-mode and this property has been set.
        /// If this is null - then it is not used.
        /// </summary>
        public string UsernameToApply { get; set; }

        /// <summary>
        /// Get or set the value to set the log-record's Version to, whenever we are in test-mode (when this is non-null).
        /// If this is null - then it is not used.
        /// </summary>
        public string VersionToApply { get; set; }

        #endregion

        #endregion properties

        #region GetRecordCount
        /// <summary>
        /// Get the number of log-records that have been sent of the given LogLevel,
        /// since the the program-under-test started, or since the last call to ResetTestCount.
        /// </summary>
        /// <param name="ofWhatLevel">The level of the records that we want a count of</param>
        /// <returns>the number of log-records that have been sent</returns>
        public int GetRecordCount( LogLevel ofWhatLevel )
        {
            if (_recordsSinceLastReset == null)
            {
                return 0;
            }
            else
            {
                return (from r in _recordsSinceLastReset
                        where r.Level == ofWhatLevel
                        select r).Count();
            }
        }
        #endregion

        #region HasBeenLogged
        /// <summary>
        /// Return true if any log-records have been transmitted with the given LogLevel
        /// since the the subject-program started, or since the last call to ResetTestCount,
        /// that contain the given messageContentFragment, within the given time-period.
        /// </summary>
        /// <param name="ofWhatLevel">the LogLevel to check for</param>
        /// <param name="messageContentFragment">the pattern of text to look for</param>
        /// <param name="recordFound">if a log-record is found that matches the given criteria, it is placed in this</param>
        /// <param name="timeoutPeriodInMilliseconds">the maximum amount of time to wait; if zero then don't wait at all</param>
        /// <returns>true if a log-record has been transmitted at the given LogLevel containing the given text</returns>
        public bool HasBeenLogged( LogLevel ofWhatLevel, string messageContentFragment, out LogRecord recordFound, int timeoutPeriodInMilliseconds )
        {
            if (_recordsSinceLastReset != null)
            {
                foreach (var r in _recordsSinceLastReset)
                {
                    if (r.Level == ofWhatLevel && r.Message.Contains( messageContentFragment ))
                    {
                        recordFound = r;
                        return true;
                    }
                }
            }
            recordFound = null;
            return false;
        }

        /// <summary>
        /// Get whether any of the log-records that have been sent of the given LogLevel,
        /// since the the program-under-test started, or since the last call to ResetTestCount,
        /// contains the given bit of text. In other words, as anything been logged that contained this text.
        /// </summary>
        /// <param name="messageContentFragment">The bit of text that we want to see whether has been logged yet</param>
        /// <param name="recordFound">if a log-record is found that matches the given criteria, it is placed in this</param>
        /// <param name="timeoutPeriodInMilliseconds">the maximum amount of time to wait; if zero then don't wait at all.</param>
        /// <returns>true if any log-records have been sent containing the given text</returns>
        public bool HasBeenLogged( string messageContentFragment, out LogRecord recordFound, int timeoutPeriodInMilliseconds )
        {
            //TODO: If timeout is > 0, need to wait for it.
            if (_recordsSinceLastReset != null)
            {
                foreach (var r in _recordsSinceLastReset)
                {
                    if (r.Message.Contains( messageContentFragment ))
                    {
                        recordFound = r;
                        return true;
                    }
                }
            }
            recordFound = null;
            return false;
        }

        /// <summary>
        /// Get whether any of the log-records that have been sent of the given LogLevel,
        /// since the the program-under-test started, or since the last call to ResetTestCount,
        /// contains the given bit of text. In other words, as anything been logged that contained this text.
        /// </summary>
        /// <param name="messageContentFragment">The bit of text that we want to see whether has been logged yet</param>
        /// <param name="recordFound">if a log-record is found that matches the given criteria, it is placed in this</param>
        /// <returns>true if any log-records have been sent containing the given text</returns>
        /// <remarks>
        /// This method does not wait for anything to be logged - it checks and then returns immediately.
        /// Thus it is the same as calling the other otherload of this method with a <c>timeoutPeriodInMilliseconds</c> value of zero.
        /// </remarks>
        public bool HasBeenLogged( string messageContentFragment, out LogRecord recordFound )
        {
            return HasBeenLogged( messageContentFragment, out recordFound, 0 );
        }

        /// <summary>
        /// Get whether anything has been logged at the given LogLevel.
        /// </summary>
        /// <param name="ofWhatLevel">the LogLevel of the log-records that we want to check</param>
        /// <param name="recordFound">if a log-record is found that matches the given criteria, it is placed in this</param>
        /// <returns>true if any log-records have been sent containing the given text</returns>
        public bool HasAnythingBeenLoggedAtLevel( LogLevel ofWhatLevel, out LogRecord recordFound )
        {
            if (_recordsSinceLastReset != null)
            {
                foreach (var r in _recordsSinceLastReset)
                {
                    if (r.Level == ofWhatLevel)
                    {
                        recordFound = r;
                        return true;
                    }
                }
            }
            recordFound = null;
            return false;
        }
        #endregion HasBeenLogged

        #region ResetTestCounts
        /// <summary>
        /// Clear the count of records maintained (for testing purposes) to zero.
        /// </summary>
        /// <returns>a reference to this same TestFacility object, in case you wish to chain method-calls</returns>
        public TestFacility ResetTestCounts()
        {
            _recordsSinceLastReset?.Clear();
            return this;
        }
        #endregion

        #region StartTestCounting
        /// <summary>
        /// Turn on test-counting and clear the list of log-records that have been accumulated thus far, if any.
        /// </summary>
        /// <returns>a reference to this same TestFacility object, in case you wish to chain method-calls</returns>
        public TestFacility StartTestCounting()
        {
            _isTesting = true;
            RecordsSinceLastReset.Clear();
            return this;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Release any test-specific resources and turn off test-mode.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Release any resources held by this object.
        /// </summary>
        /// <param name="isDisposingManagedResources">true to indicate that managed resources are also being released</param>
        protected virtual void Dispose( bool isDisposingManagedResources )
        {
            if (isDisposingManagedResources)
            {
                LogManager._testFacility = null;
                if (_recordsSinceLastReset != null)
                {
                    _recordsSinceLastReset.Clear();
                    _recordsSinceLastReset = null;
                }
            }
        }
        #endregion

        #region fields

        /// <summary>
        /// This flag is intended for use when creating unit-tests. If this is set, then a list of all log records sent
        /// is kept in _recordsSinceLastReset.
        /// </summary>
        private bool _isCounting;
        /// <summary>
        /// This indicates whether this code is being executed as part of a unit-test, whereby the logger outputs are collected
        /// for being checked for specific test outputs.
        /// </summary>
        private bool _isTesting;
        /// <summary>
        /// If this flag is true, we are not actually sending any log output, but rather just saving a list of what was to have gone out (for testing).
        /// </summary>
        private bool _isSimulatingOutput;
        /// <summary>
        /// This is the maximum number of log-records whose content is saved for test-counting purposes. Default is 20.
        /// </summary>
        private int _maxTestCount = 20;
        /// <summary>
        /// When IsCounting is on, this maintains a collection of all log-records that were sent out since that last
        /// ResetTest (or since the program-under-test invocation if ResetTest has not been called).
        /// </summary>
        private IList<LogRecord> _recordsSinceLastReset;
        /// <summary>
        /// This DateTime is the value to set the timestamp to whenever we are in test-mode and this is other than default(DateTime).
        /// </summary>
        private DateTime _timeStampToApply;

        #endregion fields
    }
}
