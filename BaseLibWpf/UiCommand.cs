using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
#if WIN8
using Windows.UI.Xaml.Input;
using EventHandler = Windows.UI.Xaml.EventHandler;
#else
using System.Windows.Input;
#endif

// Credit for this belongs to Josh Smith's article "WPF Apps With The Model-
// View-ViewModel Design Pattern".

namespace Hurst.BaseLibWpf
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates.
    /// </summary>
    /// <remarks>
    /// Note: I changed this from RelayCommand to UiCommand to avoid conflict with GalaSoft.MvvmLight.Command.RelayCommand.
    /// </remarks>
    public class UiCommand : ICommand
    {
        #region constructors
        /// <summary>
        /// Create a new command, given an Action to execute.
        /// </summary>
        /// <param name="executionAction">The code to run when this command executs.</param>
        public UiCommand( Action<object> executionAction )
            : this( executionAction, null )
        {
        }

        /// <summary>
        /// Create a new command given an Action to execute and a Predicate that dicates it's enabled state.
        /// </summary>
        /// <param name="executionAction">The code that this command will run when Execute is called</param>
        /// <param name="canExecutePredicate">The Predicate that dictates whether this command is enabled</param>
        public UiCommand( Action<object> executionAction, Predicate<object> canExecutePredicate )
        {
            if (executionAction == null)
            {
                throw new ArgumentNullException( nameof( executionAction ) );
            }
            _executionAction = executionAction;
            _canExecutePredicate = canExecutePredicate;
        }
        #endregion constructors

        #region ICommmand implementation

        [DebuggerStepThrough]
        public bool CanExecute( object parameter )
        {
            return _canExecutePredicate == null ? true : _canExecutePredicate( parameter );
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "The this keyword is used in the Silverlight version" )]
        [SuppressMessage(
            "Microsoft.Design",
            "CA1030:UseEventsWhereAppropriate",
            Justification = "This cannot be an event" )]
        public void RaiseCanExecuteChanged()
        {
            // *This* source-code is lifted straight from Mvvm-Light.  2016/2/16.
#if SILVERLIGHT
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
#elif NETFX_CORE
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
#elif XAMARIN
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
#else
            CommandManager.InvalidateRequerySuggested();
#endif
        }

        public void Execute( object parameter )
        {
            // Invoke the Action.
            _executionAction( parameter );
        }
        #endregion ICommand implementation

        #region Fields

        /// <summary>
        /// This is the Action that this command exists to execute.
        /// </summary>
        private readonly Action<object> _executionAction;

        /// <summary>
        /// To say when changes happen that affect whether the command be enabled.
        /// </summary>
        readonly Predicate<object> _canExecutePredicate;

        #endregion Fields
    }
}
