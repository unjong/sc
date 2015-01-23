using CsFormAnalyzer.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CsFormAnalyzer.ViewModels
{
	class MainWindowVM : ViewModelBase
	{
		public MainWindowVM()
		{
			RunCommand = base.CreateCommand(OnRunCommand);
			CopyToClipboardCommand = base.CreateCommand(OnCopyToClipboardCommand);
			CloseCommand = base.CreateCommand(() => { Application.Current.Shutdown(); });

			this.TargetFile = AppManager.Current.Settings.Get("TargetFile");
		}

		public ICommand RunCommand { get; private set; }
		public ICommand CopyToClipboardCommand { get; private set; }
		public ICommand CloseCommand { get; private set; }


		private string _TargetFile;
		public string TargetFile 
		{ 
			get 
			{ 
				return _TargetFile; 
			} 
			set 
			{ 
				_TargetFile = value; 
				OnPropertyChanged();

				ViewModelLocator.Current.ComponentAnalysisVM.TargetFile = value;
				ViewModelLocator.Current.DataColumnAnalysisVM.TargetFile = value;
			}
		}

		private void OnRunCommand()
		{
			AppManager.Current.Settings.Set("TargetFile", this.TargetFile);
			
			base.InvokeAsyncAction(delegate
			{
				ViewModelLocator.Current.ComponentAnalysisVM.Run();
				ViewModelLocator.Current.DataColumnAnalysisVM.Run();
			});
		}

		private void OnCopyToClipboardCommand()
		{
			ViewModelLocator.Current.ComponentAnalysisVM.CopyToClipboard();
			System.Windows.MessageBox.Show("Complated");
		}
	}
}
