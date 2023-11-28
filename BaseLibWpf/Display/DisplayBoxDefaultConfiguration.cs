#if PRE_4
#define PRE_5
#endif
using Hurst.LogNut.Util;


namespace Hurst.BaseLibWpf.Display
{
    #region class DisplayBoxDefaultConfiguration
    /// <summary>
    /// This class encapsulates the default argument values that you may want to set on your DisplayBox.
    /// This is apart from the instant argument values that are provided by DisplayBoxConfiguration.
    /// </summary>
    /// <remarks>
    /// The argument values that are provided by this class are those which you would normally set just once,
    /// as at the beginning of your program or, if your Application derives from DesktopApplication - within your override
    /// of method ConfigureDisplayBox.
    /// </remarks>
    public class DisplayBoxDefaultConfiguration
    {
        #region constructors
        /// <summary>
        /// Default Constructor: Create a new DisplayBoxConfiguration object with the a DisplayBoxType of None.
        /// </summary>
        public DisplayBoxDefaultConfiguration()
        {
        }

        /// <summary>
        /// The Copy-Constructor.
        /// </summary>
        /// <param name="source">the DisplayBoxDefaultConfiguration object to copy the values from</param>
        public DisplayBoxDefaultConfiguration(DisplayBoxDefaultConfiguration source)
        {
            this._backgroundTexture = source.BackgroundTexture;
            this._defaultCaptionForUserMistakes = source.DefaultCaptionForUserMistakes;
            this._defaultCaptionForErrors = source.DefaultCaptionForErrors;
            this._defaultSummaryTextForErrors = source.DefaultSummaryTextForErrors;
            this._isHurstButtonStyles = source.IsUsingHurstButtonStyles;
            this._isSoundEnabled = source.IsSoundEnabled;
            this._isTopmostWindowByDefault = source.IsTopmostWindowByDefault;
            this._isTouch = source.IsTouch;
            this._isUsingAeroGlassEffect = source.IsUsingAeroGlassEffect;
            this._isUsingNewerIcons = source.IsUsingNewerIcons;
            this._isUsingNewerSoundScheme = source.IsUsingNewerSoundScheme;
            this._defaultTimeout = source._defaultTimeout;
            this._defaultTimeoutForErrors = source._defaultTimeoutForErrors;
            this._defaultTimeoutForMistakes = source._defaultTimeoutForMistakes;
            this._defaultTimeoutForQuestions = source._defaultTimeoutForQuestions;
            this._defaultTimeoutForWarnings = source._defaultTimeoutForWarnings;
        }
        #endregion

        #region BackgroundTexture
        /// <summary>
        /// Get or set one (or none) out of a set of predefined background images for the display-box window. Default is None.
        /// </summary>
        public DisplayBoxBackgroundTexturePreset BackgroundTexture
        {
            get { return _backgroundTexture; }
            set { _backgroundTexture = value; }
        }

        /// <summary>
        /// Set one (or none) out of a set of predefined background images for the display-box window. Default is None.
        /// </summary>
        /// <param name="imageToUse">Which of the predefined images to use for the background texture</param>
        /// <returns>a reference to this same object such that other method-calls may be chained</returns>
        public DisplayBoxDefaultConfiguration SetBackgroundTexture(DisplayBoxBackgroundTexturePreset imageToUse)
        {
            _backgroundTexture = imageToUse;
            return this;
        }
        #endregion

        #region default captions

        /// <summary>
        /// Get or set the text that is to be shown by default (after the standard prefix) in the title-bar of error display-boxes.
        /// </summary>
        public string DefaultCaptionForErrors
        {
            get { return _defaultCaptionForErrors; }
            set { _defaultCaptionForErrors = value; }
        }

        /// <summary>
        /// Set the text that is to be shown by default (after the standard prefix) in the title-bar of error display-boxes.
        /// </summary>
        /// <param name="caption">the text to use by default for the title-bar of error display-boxes</param>
        /// <returns>a reference to this same object such that other method-calls may be chained</returns>
        public DisplayBoxDefaultConfiguration SetDefaultCaptionForErrors(string caption)
        {
            _defaultCaptionForErrors = caption;
            return this;
        }

        /// <summary>
        /// Get or set the text that is to be shown by default (after the standard prefix)
        /// in the title-bar of display-boxes that announce mistakes by the user.
        /// </summary>
        public string DefaultCaptionForUserMistakes
        {
            get { return _defaultCaptionForUserMistakes; }
            set { _defaultCaptionForUserMistakes = value; }
        }

        /// <summary>
        /// Set the text that is to be shown by default (after the standard prefix) in the title-bar for display-boxes of type UserMistake.
        /// </summary>
        /// <param name="caption">the text to use by default for the title-bar of user-mistake display-boxes</param>
        /// <returns>a reference to this same object such that other method-calls may be chained</returns>
        public DisplayBoxDefaultConfiguration SetDefaultCaptionForUserMistakes(string caption)
        {
            _defaultCaptionForUserMistakes = caption;
            return this;
        }
        #endregion default captions

        #region DefaultSummaryTextForErrors
        /// <summary>
        /// Get or set the text to use by default for the Summary-Text for error display-boxes,
        /// for whenever it is not specified explicitly. By default, this is "A program-fault has occurred."
        /// </summary>
        public string DefaultSummaryTextForErrors
        {
            get { return _defaultSummaryTextForErrors; }
            set { _defaultSummaryTextForErrors = value; }
        }

        /// <summary>
        /// Set the text to use by default for the Summary-Text for error display-boxes,
        /// for whenever it is not specified explicitly. By default, this is "A program-fault has occurred."
        /// </summary>
        /// <param name="textToUseForErrors">what to put for the summary-text for error display-boxes</param>
        /// <returns>a reference to this same object such that other method-calls may be chained</returns>
        public DisplayBoxDefaultConfiguration SetDefaultSummaryTextForErrors(string textToUseForErrors)
        {
            _defaultSummaryTextForErrors = textToUseForErrors;
            return this;
        }
        #endregion

        #region DisplayBoxWidth
        /// <summary>
        /// Get or set the width of the display-box Window. The default value is 375.
        /// </summary>
        public double DisplayBoxWidth
        {
            get { return _displayBoxWidth; }
            set { _displayBoxWidth = value; }
        }

        /// <summary>
        /// Set the width for the display-box Window. It's default value is 375.
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public DisplayBoxDefaultConfiguration SetDisplayBoxWidth( double width )
        {
            _displayBoxWidth = width;
            return this;
        }
        #endregion

        #region GetDefaultTimeoutValueFor
        /// <summary>
        /// Get the default value to use for the display-box timeout, in seconds, for the given type of display-box.
        /// </summary>
        public int GetDefaultTimeoutValueFor(DisplayBoxType forWhatTypeOfDisplayBox)
        {
            int t;
            switch (forWhatTypeOfDisplayBox)
            {
                case DisplayBoxType.Warning:
                    t = _defaultTimeoutForWarnings;
                    break;
                case DisplayBoxType.Error:
                case DisplayBoxType.SecurityIssue:
                    t = _defaultTimeoutForErrors;
                    break;
                case DisplayBoxType.Question:
                    t = _defaultTimeoutForQuestions;
                    break;
                case DisplayBoxType.UserMistake:
                    t = _defaultTimeoutForMistakes;
                    break;
                default:
                    t = _defaultTimeout;
                    break;
            }
            return t;
        }
        #endregion

        #region IsUsingHurstButtonStyles
        /// <summary>
        /// Get or set whether to use our own custom WPF Styles for the display-box buttons. Default is false - to use the plain standard style.
        /// </summary>
        public bool IsUsingHurstButtonStyles
        {
            get { return _isHurstButtonStyles; }
            set { _isHurstButtonStyles = value; }
        }

        /// <summary>
        /// Set whether to use the custom WPF Styles for the display-box buttons that I created. Default is false.
        /// </summary>
        /// <param name="isToUseJhStyles">true to make use of the custom button styles I provided, false to stick with the standard appearance</param>
        /// <returns>a reference to this same object such that other method-calls may be chained</returns>
        public DisplayBoxDefaultConfiguration SetToUseHurstButtonStyles(bool isToUseJhStyles)
        {
            _isHurstButtonStyles = isToUseJhStyles;
            return this;
        }
        #endregion

        #region IsSoundEnabled
        /// <summary>
        /// Get or set the flag that indicates whether this display-box will make a sound when it shows. Defaults to true.
        /// </summary>
        public bool IsSoundEnabled
        {
            get { return _isSoundEnabled; }
            set { _isSoundEnabled = value; }
        }

        /// <summary>
        /// Set whether to make a sound when this display-box is displayed. Default is true.
        /// </summary>
        /// <param name="isToPlaySoundEffects">true to use the standard icons, false to use the non-standard set</param>
        /// <returns>a reference to this same object such that other method-calls may be chained</returns>
        public DisplayBoxDefaultConfiguration SetIsSoundEnabled(bool isToPlaySoundEffects)
        {
            IsSoundEnabled = isToPlaySoundEffects;
            return this;
        }
        #endregion

        #region IsTopmostWindowByDefault
        /// <summary>
        /// Get or set whether we want the display-box to, by default, position itself over top of any other window
        /// that is on the Windows Desktop. This is false unless you explicitly set it to true.
        /// </summary>
        public bool IsTopmostWindowByDefault
        {
            get { return _isTopmostWindowByDefault; }
            set { _isTopmostWindowByDefault = value; }
        }

        /// <summary>
        /// Set whether we want the display-box to position itself over top of any other window
        /// that is on the Windows Desktop by default. Default is false.
        /// </summary>
        /// <param name="isToBeOnTopByDefault">true if we want the default behavior to be to position this over top of all other windows</param>
        /// <returns>a reference to this same object such that other method-calls may be chained</returns>
        public DisplayBoxDefaultConfiguration SetToBeTopmostWindowByDefault(bool isToBeOnTopByDefault)
        {
            _isTopmostWindowByDefault = isToBeOnTopByDefault;
            return this;
        }
        #endregion

        #region IsTouch
        /// <summary>
        /// Gets or sets a value that indicates whether we want the display-box to behave with the assumption that
        /// it is being displayed upon a multi-touch monitor.  Defaults to false.
        /// </summary>
        public bool IsTouch
        {
            get { return _isTouch; }
            set { _isTouch = value; }
        }

        /// <summary>
        /// Sets whether we want the display-box to behave with the assumption that
        /// it is being displayed upon a multi-touch monitor.  Default is false.
        /// </summary>
        /// <param name="isTouchSensitive">True if we want to run on a multi-touch display</param>
        /// <returns>a reference to this same object such that other method-calls may be chained</returns>
        public DisplayBoxDefaultConfiguration SetIsTouch(bool isTouchSensitive)
        {
            _isTouch = isTouchSensitive;
            return this;
        }
        #endregion

        #region IsUsingAeroGlassEffect
        /// <summary>
        /// Get or set whether to use the Windows Vista/7 Aero-glass translucency effect for this Window. Default is no.
        /// </summary>
        public bool IsUsingAeroGlassEffect
        {
            get { return _isUsingAeroGlassEffect; }
            set { _isUsingAeroGlassEffect = value; }
        }
        #endregion

        #region IsUsingNewerIcons
        /// <summary>
        /// Get or set whether to use owr own set, as opposed to the old-style standard operating-system icons. Default is true.
        /// </summary>
        /// <returns>a reference to this same object such that other method-calls may be chained</returns>
        public bool IsUsingNewerIcons
        {
            get { return _isUsingNewerIcons; }
            set { _isUsingNewerIcons = value; }
        }

        /// <summary>
        /// Set whether to use our own newer set of display-box icons, as opposed to the older standard set. Default is true.
        /// </summary>
        /// <param name="isToUseNewerIcons">true to use the new set that I provided, false to use the old standard set</param>
        public DisplayBoxDefaultConfiguration SetToUseNewIcons(bool isToUseNewerIcons)
        {
            IsUsingNewerIcons = isToUseNewerIcons;
            return this;
        }
        #endregion

        #region IsUsingNewerSoundScheme
        /// <summary>
        /// Get or set whether to use the our own custom set of sound effects, as opposed to the old-style default Windows set of messagebox sounds
        /// Default is false.
        /// </summary>
        public bool IsUsingNewerSoundScheme
        {
            get { return _isUsingNewerSoundScheme; }
            set { _isUsingNewerSoundScheme = value; }
        }

        /// <summary>
        /// Set whether to use our own custom set of sound effects, as opposed to the default Windows set of messagebox sounds.
        /// Default is false.
        /// </summary>
        /// <param name="isToUseNewerSoundScheme">true to use the newer set, false to use the old standard set</param>
        /// <returns>a reference to this same object such that other method-calls may be chained</returns>
        public DisplayBoxDefaultConfiguration SetToUseNewSounds(bool isToUseNewerSoundScheme)
        {
            IsUsingNewerSoundScheme = isToUseNewerSoundScheme;
            return this;
        }
        #endregion

        #region SetDefaultTimeoutFor
        /// <summary>
        /// Set the default value to use for the given type of display-box.
        /// </summary>
        /// <param name="forWhatTypeOfDisplayBox">the DisplayBoxType for which this default value will apply</param>
        /// <param name="timeoutPeriodInSeconds">the amount of time, in seconds, for display-boxes of this type to wait before timing-out</param>
        public DisplayBoxDefaultConfiguration SetDefaultTimeoutFor(DisplayBoxType forWhatTypeOfDisplayBox, int timeoutPeriodInSeconds)
        {
            switch (forWhatTypeOfDisplayBox)
            {
                case DisplayBoxType.Warning:
                    _defaultTimeoutForWarnings = timeoutPeriodInSeconds;
                    break;
                case DisplayBoxType.Error:
                case DisplayBoxType.SecurityIssue:
                    _defaultTimeoutForErrors = timeoutPeriodInSeconds;
                    break;
                case DisplayBoxType.Question:
                    _defaultTimeoutForQuestions = timeoutPeriodInSeconds;
                    break;
                case DisplayBoxType.UserMistake:
                    _defaultTimeoutForMistakes = timeoutPeriodInSeconds;
                    break;
                default:
                    _defaultTimeout = timeoutPeriodInSeconds;
                    break;
            }
            return this;
        }
        #endregion

        #region fields

        /// <summary>
        /// This selects one (or none) out of a set of predefined background images for the display-box window.
        /// The default value is None.
        /// </summary>
        private DisplayBoxBackgroundTexturePreset _backgroundTexture = DisplayBoxBackgroundTexturePreset.None;
        /// <summary>
        /// If this is non-null, then this is what the developer has explicitly specified to use as the default titlebar-suffix
        /// for display-boxes that express errors.
        /// </summary>
        private string _defaultCaptionForErrors;
        /// <summary>
        /// The caption-text to show by default for the display-box title-bar text (after the prefix) for user-mistake message types.
        /// </summary>
        private string _defaultCaptionForUserMistakes;
        /// <summary>
        /// This is the default value to use for the Summary-Text for error display-boxes.
        /// </summary>
        private string _defaultSummaryTextForErrors = "A program-fault has occurred.";
        /// <summary>
        /// The timeout period to use by default for display-boxes, in seconds, that are of type Information. Initial value is 10.
        /// </summary>
        private int _defaultTimeout = 10;
        /// <summary>
        /// The timeout period to use by default for error or security-issue display-boxes, in seconds. Initial value is 30.
        /// </summary>
        private int _defaultTimeoutForErrors = 30;
        /// <summary>
        /// The timeout period to use by default for display-boxes responding to user mistakes, in seconds. Initial value is 5 minutes.
        /// </summary>
        private int _defaultTimeoutForMistakes = 5 * 60;
        /// <summary>
        /// The timeout period to use by default for question display-boxes, in seconds. Initial value is 60.
        /// </summary>
        private int _defaultTimeoutForQuestions = 60;
        /// <summary>
        /// The timeout period to use by default for warning display-boxes, in seconds. Initial value is 15.
        /// </summary>
        private int _defaultTimeoutForWarnings = 15;

        /// <summary>
        /// This denotes the width of the display-box Window. The default value is 375.
        /// </summary>
        private double _displayBoxWidth = 375;

        /// <summary>
        /// This indicates whether to use our own custom WPF Styles for the display-box buttons. Default is false.
        /// </summary>
        private bool _isHurstButtonStyles;
        /// <summary>
        /// This flag controls whether the audial effects are generated when a display-box is shown. Default is true.
        /// </summary>
        private bool _isSoundEnabled = true;
        /// <summary>
        /// This dictates whether we want the display-box to, by default, position itself over top of any other window
        /// that is on the Windows Desktop. This can be specified by the current DisplayBoxConfiguration, so this is only a fallback value.
        /// This is false by default.
        /// </summary>
        private bool _isTopmostWindowByDefault;
        /// <summary>
        /// This flag indicates that we are to act as though executing on a multi-touch screen. Default is false.
        /// </summary>
        private bool _isTouch;
        /// <summary>
        /// If true then the Windows Vista/7 Aero-glass translucency effect is used for this Window. Default is false.
        /// </summary>
        private bool _isUsingAeroGlassEffect;
        /// <summary>
        /// This flag indicates whether to use the custom set of audio files I provided, as opposed to the original sounds. Default is false.
        /// </summary>
        private bool _isUsingNewerSoundScheme;
        /// <summary>
        /// This flag indicates whether to use the custom set of icons I designed, as opposed to the old set. Default is true.
        /// </summary>
        private bool _isUsingNewerIcons = true;

        #endregion fields
    }
    #endregion class DisplayBoxDefaultConfiguration
}
