using System;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml;
#endif


namespace UiBaseLib
{
    /// <summary>
    /// This class is intended to denote the location, size, and state of a UX Window.
    /// </summary>
    [Serializable]
    public class WindowPosition
    {
        #region constructors
        /// <summary>
        /// Create a new <see cref="WindowPosition"/> instance with all default values.
        /// </summary>
        public WindowPosition()
        {
        }

        /// <summary>
        /// Create a new <see cref="WindowPosition"/> instance containing the given flags that indicate whether to save location, and whether to save the size.
        /// </summary>
        /// <param name="isToSaveLocation">whether to save the window's location ('where'), as distinct from its size</param>
        /// <param name="isToSaveSize">whether to save the window's size, as distinct from its location</param>
        public WindowPosition( bool isToSaveLocation, bool isToSaveSize )
        {
            IsSavingLocation = isToSaveLocation;
            IsSavingSize = isToSaveSize;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Get or set whether the pertinent properties of this WindowPosition object has changed
        /// since the last time this was set to false.
        /// </summary>
        [XmlIgnore]
        public bool HasChanged { get; set; }

        #region HasPositionBeenSet
        /// <summary>
        /// Get or set whether this application has been positioned upon the desktop yet from the saved position-setting,
        /// so that it is not done more than once.
        /// </summary>
        [XmlIgnore]
        public bool HasPositionBeenSet

        {
            get { return _hasPositionBeenSet; }
            set { _hasPositionBeenSet = value; }
        }
        /// <summary>
        /// This flag indicates whether this application has been positioned upon the desktop yet from the saved position-setting,
        /// so that it is not done more than once.
        /// </summary>
        protected bool _hasPositionBeenSet;
        #endregion

        /// <summary>
        /// Get or set whether we're saving this Window's location, vs location+size or just the size alone.
        /// </summary>
        [XmlAttribute( "SaveLocation" )]
        public bool IsSavingLocation { get; set; }

        /// <summary>
        /// Get or set whether we're saving this Window's size, vs position+size, or just the position alone.
        /// </summary>
        [XmlAttribute( "SaveSize" )]
        public bool IsSavingSize { get; set; }

        /// <summary>
        /// Get whether a value has been set for the Location.
        /// </summary>
        [XmlIgnore]
        public bool IsLocationValue
        {
            get 
            {
                bool hasValues = !Double.IsNaN(SavedLocation.X) && !Double.IsNaN(SavedLocation.Y);
                bool isNotAllZeros = SavedLocation.X != 0 || SavedLocation.Y != 0;
                return hasValues && isNotAllZeros;
            }
        }

        /// <summary>
        /// Get whether a value has been set for the Size.
        /// </summary>
        [XmlIgnore]
        public bool IsSizeValue
        {
            get 
            {
                bool hasValues = !Double.IsNaN(SavedSize.Width) && !Double.IsNaN(SavedSize.Height);
                bool isNotAllZeros = SavedSize.Width != 0 && SavedSize.Height != 0;
                return hasValues && isNotAllZeros;
            }
        }

        /// <summary>
        /// Get or set the <c>Point</c> that denotes the location that the window had before being
        /// minimized or maximized.
        /// </summary>
        [XmlElement( "RestorationLocation" )]
        public System.Windows.Point RestorationLocation
        {
            get { return _restorationLocation; }
            set { _restorationLocation = value; }
        }

        /// <summary>
        /// Get or set the <c>Size</c> that denotes the dimensions that the window had before being
        /// minimized or maximized.
        /// </summary>
        [XmlElement( "RestorationSize" )]
        public System.Windows.Size RestorationSize
        {
            get { return _restorationSize; }
            set { _restorationSize = value; }
        }

        /// <summary>
        /// Get or set the <c>Point</c> object whose Left and Top properties denote the location of the Window.
        /// </summary>
        [XmlElement( "Location" )]
        public System.Windows.Point SavedLocation
        {
            get { return _location; }
            set { _location = value; }
        }

        /// <summary>
        /// Get or set the <c>Size</c> object that denotes the Width and Height of the Window.
        /// </summary>
        [XmlElement( "Size" )]
        public System.Windows.Size SavedSize
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Get or set the WindowState that was saved within this object - Normal, Maximized, or Minimized.
        /// </summary>
        [XmlAttribute( "State" )]
        public System.Windows.WindowState WindowState
        {
            get { return _windowState; }
            set { _windowState = value; }
        }

        /// <summary>
        /// Get or set the FormWindowState that was saved within this object - Normal, Maximized, or Minimized.
        /// </summary>
        //[XmlAttribute( "FormWindowState" )]
        //public System.Windows.Forms.FormWindowState FormWindowState
        //{
        //    get { return _formWindowState; }
        //    set { _formWindowState = value; }
        //}

        public System.Windows.Forms.FormWindowState FormWindowState
        {
            get
            {
                switch (WindowState)
                {
                    case WindowState.Maximized:
                        return System.Windows.Forms.FormWindowState.Maximized;
                    case WindowState.Minimized:
                        return System.Windows.Forms.FormWindowState.Minimized;
                    default:
                        return System.Windows.Forms.FormWindowState.Normal;
                }
            }
            set
            {
                switch (value)
                {
                    case System.Windows.Forms.FormWindowState.Maximized:
                        _windowState = WindowState.Maximized;
                        break;
                    case System.Windows.Forms.FormWindowState.Minimized:
                        _windowState = WindowState.Minimized;
                        break;
                    default:
                        _windowState = WindowState.Normal;
                        break;
                }
            }
        }

        #endregion public properties

        #region public methods

        #region ToString
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "WindowPosition(" );
            if (IsSavingLocation && IsSavingSize)
            {
                sb.Append( "saving both location and size, " );
            }
            else if (IsSavingLocation)
            {
                sb.Append( "IsSavingLocation, " );
            }
            else if (IsSavingSize)
            {
                sb.Append( "IsSavingSize, " );
            }
            if (!IsSavingLocation && !IsSavingSize)
            {
                sb.Append( "saving neither location nor size, " );
            }
            sb.AppendFormat( "WindowState = {0}, ", _windowState );
            if (_windowState == WindowState.Normal)
            {
                if (IsSavingLocation)
                {
                    sb.Append( "IsLocationValue=" ).Append( IsLocationValue );
                    sb.AppendFormat( ", _location={0}", _location ).Append( ", " );
                }
                if (IsSavingSize)
                {
                    if (IsSizeValue)
                    {
                        sb.AppendFormat( "size = {0}", _size );
                    }
                    else
                    {
                        sb.Append( "no size value" );
                    }
                }
            }
            return sb.ToString();
        }
        #endregion

        #endregion public methods

        #region private fields

        private System.Windows.Point _location;
        private System.Windows.Point _restorationLocation;
        private System.Windows.Size _size;
        private System.Windows.Size _restorationSize;
        private System.Windows.WindowState _windowState;
        //private System.Windows.Forms.FormWindowState _formWindowState;

        #endregion private fields
    }
}
