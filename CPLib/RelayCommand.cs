using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CPLib
{

    public class RelayCommand<T> : System.Windows.Input.ICommand
    {
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        readonly Action<T> _execute = null;
        readonly Predicate<T> _canExecute = null;
    }

    public class CheckCanExecuteEventArgs : EventArgs
    {
        public CheckCanExecuteEventArgs(bool canExecute)
        {
            this.CanExecute = canExecute;
        }
        public bool CanExecute { get; set; }
    }

}
