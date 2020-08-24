using System.Windows.Input;

namespace GenerateDocumentation.ViewModel
{
    public class NumericUpDownViewModel : ModelBase
    {
        #region Button Settings
        private ICommand _upCommand;
        public ICommand UpCommand
        {
            get
            {
                return _upCommand ?? (_upCommand = new RelayCommand(o => { Number++; }));
            }
        }

        private ICommand _downCommand;
        public ICommand DownCommand
        {
            get
            {
                return _downCommand ?? (_downCommand = new RelayCommand(o => { Number--; }));
            }
        }

        #endregion

        private int _number = Properties.Settings.Default.NumberCab;
        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged(nameof(Number));
                Properties.Settings.Default.NumberCab = _number;
                Properties.Settings.Default.Save();

            }
        }


    }
}
