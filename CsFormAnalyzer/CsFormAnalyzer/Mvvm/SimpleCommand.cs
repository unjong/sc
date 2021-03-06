﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CsFormAnalyzer.Mvvm
{
	public class SimpleCommand : ICommand
	{
        private Action<object> onExecute;
        private Func<bool> onCanExcute;

		public SimpleCommand(Action<object> onExecute, Func<bool> onCanExcute = null)
		{
			this.onExecute = onExecute;
			this.onCanExcute = onCanExcute;
		}

		public void Execute(object parameter)
		{
			onExecute.Invoke(parameter);
		}

		public bool CanExecute(object parameter)
		{
			if (this.onCanExcute == null)
				return true;
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
