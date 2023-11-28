#if PRE_4
#define PRE_5
#endif
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
#if !SILVERLIGHT
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
#if !(PRE_5)
using System.Runtime.CompilerServices;
#endif
#endif
using Hurst.LogNut.Util;


namespace Hurst.BaseLibWpf
{
    /// <summary>
    /// A (minimal) base class for a something that implements INotifyPropertyChanged.
    /// </summary>
    public abstract class ViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Subclasses can override this method to do stuff after a property value is set. The base implementation does nothing.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void AfterPropertyChanged( string propertyName )
        {
        }

        /// <summary>
        /// Get an instance of a PropertyChangedEventArgs object, by taking it from our private cache if it is there - otherwise creating it.
        /// </summary>
        /// <param name="propertyName">The name of the property that the PropertyChangedEventArgs object is constructed from</param>
        /// <returns>An instance of PropertyChangedEventArgs constructed from the given propertyName</returns>
        public static PropertyChangedEventArgs GetPropertyChangedEventArgs( string propertyName )
        {
            if (StringLib.HasNothing( propertyName ))
            {
                throw new ArgumentNullException( "propertyName cannot be null or empty." );
            }

            PropertyChangedEventArgs args;

            // Get the event args from the cache, creating and adding them to the cache only if necessary.
            lock (_lockObject)
            {
                bool isCached = _eventArgCache.ContainsKey(propertyName);
                if (!isCached)
                {
                    _eventArgCache.Add( propertyName, new PropertyChangedEventArgs( propertyName ) );
                }
                args = _eventArgCache[propertyName];
            }
            return args;
        }

        private static readonly Dictionary<string, PropertyChangedEventArgs> _eventArgCache = new Dictionary<string, PropertyChangedEventArgs>();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Check that the given property exists on this type.
        /// </summary>
        /// <param name="propertyName">The name of the property to check</param>
        [Conditional( "DEBUG" )]
        private void VerifyProperty( string propertyName )
        {
            Type type = this.GetType();
            // Look for a public property with the specified name.
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            // The property couldn't be found, so get upset.
            Debug.Assert( propertyInfo != null, String.Format( "Unable to verify that type {0} has property {1} !", type.FullName, propertyName ) );
        }

        #region INotifyPropertyChanged

        // This is after reading a few articles online, such as Jeremy Likness' article at http://www.codeproject.com/KB/silverlight/mvvm-explained.aspx

        /// <summary>
        /// Raise the PropertyChanged event, with the given propertyName as the argument.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed in value. The spelling must be correct.</param>
#if !(PRE_5 || PRE_4)
        public void Notify([CallerMemberName] string propertyName = "")
#else
        public void Notify( string propertyName )
#endif
        {
            this.VerifyProperty( propertyName );

            // Test the PropertyChanged event first for null,
            // because another thread might change it between the two statements.
            // And use the cached event-arg.
            PropertyChanged?.Invoke( this, ViewModel.GetPropertyChangedEventArgs( propertyName ) );

            this.AfterPropertyChanged( propertyName );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        #region IsInDesignMode
        /// <summary>
        /// Get whether the control is in design mode (running in Blend or Cider).
        /// </summary>
        public static bool IsInDesignModeStatic
        {
            get
            {
                // Thanks to Laurent Bugnion for the this.
                // http://geekswithblogs.net/lbugnion/archive/2009/09/05/detecting-design-time-mode-in-wpf-and-silverlight.aspx
                // except, it would've been rather nice if it had actually been correct!  :>(
                if (!_isInDesignMode.HasValue)
                {
#if SILVERLIGHT
                    _isInDesignMode = System.ComponentModel.DesignerProperties.IsInDesignTool;
#else
#if WIN8
                    _isInDesignMode = Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#else
                    // If there is a MainWindow, then use that to determine whether we are in design-mode.
                    Window mainWindow = System.Windows.Application.Current?.MainWindow;
                    if (mainWindow != null)
                    {
                        _isInDesignMode = System.ComponentModel.DesignerProperties.GetIsInDesignMode( mainWindow );
                    }
                    else
                    {
                        // otherwise, just use this static property (which has been returning True when running in Visual Studio Debug!).
                        _isInDesignMode = DesignerProperties.IsInDesignMode;
                    }
                    //TODO: Why does this not compile?
                    //var prop1 = System.ComponentModel.DesignerProperties.GetIsInDesignMode();
                    //var prop = DesignerProperties.IsInDesignModeProperty;
                    //_isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;
#endif
#endif
                }
                return _isInDesignMode.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control is in design mode (running under Blend or Cider).
        /// </summary>
        [SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Non static member needed for data binding" )]
        public bool IsInDesignMode
        {
            get { return IsInDesignModeStatic; }
        }

        private static bool? _isInDesignMode;

        #endregion IsInDesignMode
    }
}
