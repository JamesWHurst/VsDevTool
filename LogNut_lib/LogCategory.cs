using System;
using System.Text;
using Hurst.LogNut.Util;


namespace Hurst.LogNut
{
    public class LogCategory
    {
        #region constructors

        public LogCategory(string name)
        {
            _name = name;
            _mask = 0UL;
        }

        //public LogCategory( LogLevel logLevel )
        //{
        //    _name = null;
        //    switch (LogLevel)
        //    {
        //        case LogLevel.Trace:
        //            _mask = MaskTrace;
        //            break;
        //        case LogLevel.Debug:
        //            _mask = MaskDebug;
        //            break;
        //        case LogLevel.Warning:
        //            _mask = MaskWarn;
        //            break;
        //        case LogLevel.Error:
        //            _mask = MaskError;
        //            break;
        //        case LogLevel.Critical:
        //            _mask = MaskFatal;
        //            break;
        //        default:
        //            _mask = MaskInfo;
        //            break;
        //    }
        //}

        //public LogCategory( LogLevel logLevel, string name, ulong mask )
        //{
        //    _name = null;
        //    _mask = mask;
        //    switch (logLevel)
        //    {
        //        case LogLevel.Trace:
        //            Mask |= MaskTrace;
        //            break;
        //        case LogLevel.Debug:
        //            Mask |= MaskDebug;
        //            break;
        //        case LogLevel.Warning:
        //            Mask |= MaskWarn;
        //            break;
        //        case LogLevel.Error:
        //            Mask |= MaskError;
        //            break;
        //        case LogLevel.Critical:
        //            Mask |= MaskFatal;
        //            break;
        //        default:
        //            Mask |= MaskInfo;
        //            break;
        //    }
        //}
        #endregion constructors

        #region instance properties

        /// <summary>
        /// Get whether this LogCategory is 'empty' - meaning the Mask is zero.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Name == null || Name == "Empty";
                //return Mask == 0UL;
            }
        }

        /// <summary>
        /// Get or set whether this LogCategory is enabled (turned on).
        /// It's initial default value is null, meaning this category does not dictate whether a particular logging-statement
        /// produces a log output.
        /// </summary>
        public bool? IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public bool IsEnabledViaMask()
        {
            return false;
        }

        #region level enablement

        #region LowestLevelThatIsEnabled
        /// <summary>
        /// Get or set the minimum log-level that is enabled, or null to express no preference for this category.
        /// Default is null; when this is non-null it overrides that same-named property setting of LogManager.Config and of Logger.
        /// </summary>
        /// <remarks>
        /// Get or set the minimum log-level that is enabled for output, or null to express no preference for this category.
        /// 
        /// Setting this to Trace - which is the lowest level, enables all levels.
        ///
        /// When this is non-null, it overrides the setting of LogManager.Config and of Logger.
        /// </remarks>
        public LogLevel? LowestLevelThatIsEnabled
        {
            get { return _lowestLevelThatIsEnabled; }
            set { _lowestLevelThatIsEnabled = value; }
        }
        #endregion

        #region EnableAllLevels
        /// <summary>
        /// Allow output for all logging levels down to the lowest level,
        /// i.e. all log-levels.
        /// </summary>
        /// <remarks>
        /// This is just a shortcut-method for setting LowestLevelThatIsEnabled to Trace.
        /// When this is non-null, it overrides the setting of LogManager.Config and of Logger.
        /// 
        /// Setting this to Trace - which is the lowest level, enables all levels.
        /// </remarks>
        public void EnableAllLevels()
        {
            _lowestLevelThatIsEnabled = default(LogLevel);
        }
        #endregion

        /// <summary>
        /// This denotes lowest-level LogLevel that is currently enabled JUST FOR THIS CATEGORY.
        /// The default is null, which means this category voices no preference and the LogManager.Config or Logger
        /// are what controls.
        /// </summary>
        private LogLevel? _lowestLevelThatIsEnabled;

        #endregion level enablement

        public ulong Mask
        {
            get { return _mask; }
            set
            {
                _mask = value;
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        #endregion instance properties

        #region static properties

        /// <summary>
        /// This is a LogCategory with a Mask value of zero and a Name property value of null.
        /// When a logging-statement specifies an Empty category - that is always logged
        /// regardless of which categories are enabled at that moment.
        /// </summary>
        public static LogCategory Empty
        {
            get
            {
                if (_emptyCategory is null)
                {
                    _emptyCategory = new LogCategory("Empty");
                    //CBL Commented this out when I moved ETW output to it's own library. Is this essential? How to fix this?
                    // 2020/1/27
                    //                    _emptyCategory.Mask = (ulong)LognutEventSource.Keywords.All;
                }
                return _emptyCategory;
            }
        }
        private static LogCategory _emptyCategory;

        /// <summary>
        /// Get the LogCategory that denotes logs that simply identify the entry into and exit from a method.
        /// </summary>
        public static LogCategory MethodTrace
        {
            get
            {
                if (_catMethodTrace is null)
                {
                    _catMethodTrace = new LogCategory("MethodTrace");
                }
                return _catMethodTrace;
            }
        }
        private static LogCategory _catMethodTrace;

        public static LogCategory CatExceptions
        {
            get
            {
                if (_catExceptions is null)
                {
                    _catExceptions = new LogCategory("Exceptions");

                    //CBL Commented this out when I moved ETW output to it's own library. Is this essential? How to fix this?
                    // 2020/1/27
                    //                    _catExceptions.Mask = (ulong)LognutEventSource.Keywords.Exceptions;
                }
                return _catExceptions;
            }
        }
        private static LogCategory _catExceptions;

        #endregion static properties

        #region instance methods

        /// <summary>
        /// Reset any overrides of this category.
        /// </summary>
        /// <remarks>
        /// What this does is reset the overrides that this category may contain:
        /// Set the IsEnabled property back to null (expressing no preference), and
        /// set the LowestLevelThatIsEnabled for this category to null, meaning no preference is voiced for this category
        /// </remarks>
        public void ClearOverrides()
        {
            IsEnabled = null;
            LowestLevelThatIsEnabled = null;
        }

        public void Enable()
        {
            this.IsEnabled = true;
        }

        public void Disable()
        {
            this.IsEnabled = false;
        }

        #region ToString
        /// <summary>
        /// Override the ToString method to provide a more useful display.
        /// </summary>
        /// <returns>a string the denotes the state of this object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("LogCategory(");
            sb.Append(StringLib.AsQuotedString(this.Name));
            if (this.IsEmpty)
            {
                sb.Append(",IsEmpty");
            }
            sb.Append(")");
            return sb.ToString();
        }
        #endregion

        #endregion instance methods

        #region static methods

        public static LogCategory CreateNew(string name)
        {
            if (_catCounter < MaxCats)
            {
                _catCounter++;
                LogCategory newCat = new LogCategory(name);
                // I limit this to 58, and not 64 - because 6 bit-positions are already taken for the log-level.
                //if (_maskCounter < 58)
                //{
                ulong newMask = MaskFirstCatBit << _catCounter;
                newCat.Mask = newMask;
                //    _maskCounter++;
                //}
                //else
                //{
                //    newCat = LogCategory.Empty;
                //}
                LogManager.Categories.Add(newCat);
                LogManager.IsCleared = false;
                return newCat;
            }
            else
            {
                throw new InvalidOperationException(message: "You can only create up to " + MaxCats + " categories.");
            }
        }
        private static int _catCounter;

        #endregion static methods

        #region internal implementation

        /// <summary>
        /// This tracks the bit-position of the 64-bit mask that has been allocated thus far.
        /// When this reaches 64, no more categories are available.
        /// </summary>
        //private static int _maskCounter;

        private const int _numberOfAvailableBits = 64 - _numberOfReservedBits;

        private const int _numberOfReservedBits = 6;

        /// <summary>
        /// This 64-bit mask dictates which levels and categories are currently enabled.
        /// </summary>
        public static ulong _maskEnabled;

        /// <summary>
        /// This indicates whether this LogCategory-instance is enabled, as opposed to disabled.
        /// It's initial default value is null, meaning this category does not dictate whether a particular logging-statement
        /// produces a log output.
        /// </summary>
        private bool? _isEnabled;

        /// <summary>
        /// This is the 64-bit bit-mask for this category.
        /// </summary>
        private ulong _mask;

        private const int MaxCats = 10;
        public const ulong MaskFirstCatBit = 0x0000000000000040UL;
        //public const ulong MaskTrace = 0x0000000000000001UL;
        //public const ulong MaskDebug = 0x0000000000000002UL;
        //public const ulong MaskInfo  = 0x0000000000000004UL;
        //public const ulong MaskWarn  = 0x0000000000000008UL;
        //public const ulong MaskError = 0x0000000000000010UL;
        //public const ulong MaskFatal = 0x0000000000000020UL;
        private string _name;

        #endregion internal implementation
    }
}
