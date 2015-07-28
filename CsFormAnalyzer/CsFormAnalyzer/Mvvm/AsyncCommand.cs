using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CsFormAnalyzer.Mvvm
{
	public class AsyncCommand : ICommand
	{
        private Func<Task> onExcute;
        private Func<bool> onCanExcute;

        public delegate void IsBusyChangedHandler(AsyncCommand command, bool isBusy);
        public event IsBusyChangedHandler IsBusyChanged;

        private bool _IsBusy;
        public bool IsBusy
        {
            get
            {
                return _IsBusy;
            }
            set
            {
                _IsBusy = value;
                if (IsBusyChanged != null) IsBusyChanged(this, value);
            }
        }

		public AsyncCommand(Func<Task> onExcute, Func<bool> onCanExcute = null)
		{			
			this.onExcute = onExcute;
			this.onCanExcute = onCanExcute;
		}

		public async void Execute(object parameter)
		{
			IsBusy = true;
			await onExcute.Invoke();
			IsBusy = false;
			RaiseCanExecuteChanged();
		}

		public bool CanExecute(object parameter)
		{
			if (this.onCanExcute == null)
				return !IsBusy;
			else
				return this.onCanExcute.Invoke();
		}
		
		event EventHandler ICommand.CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void RaiseCanExecuteChanged()
		{
			CommandManager.InvalidateRequerySuggested();
		}
	}
}
