using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GenerateDocumentation.Properties;

namespace GenerateDocumentation
{
    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, 
            [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value))
                return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class RelayCommand :ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {

        }

        private RelayCommand([CanBeNull] Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new System.ArgumentNullException("execute//выполнить");
            }

            _execute = execute;
            _canExecute = canExecute;
        }

    }

}
