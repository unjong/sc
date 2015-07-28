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
        #region Initialize...

        public MainWindowVM()
		{
			this.TargetFile = AppManager.Current.Settings.Get("TargetFile");
		}

        #endregion

        #region Properties

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
                ViewModelLocator.Current.CallTreeVM.TargetFile = value;
                ViewModelLocator.Current.SuggestVM.TargetFile = value;
            }
        }

        public enum TabItems
        {
            ComponentInfo,
            GridColumnInfo,
            CallTree,
            ClassList,
            SAFCodeGen,
            EtcTools,
            Suggest,
        }
        private TabItems _SelectedTabItem = TabItems.ComponentInfo;
        public TabItems SelectedTabItem
        {
            get { return _SelectedTabItem; }
            set { _SelectedTabItem = value; OnPropertyChanged(); }
        }

        public string Password { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        #endregion

        #region Commands

        public ICommand RunCommand { get; private set; }
		public ICommand CopyToClipboardCommand { get; private set; }
		public ICommand CloseCommand { get; private set; }
                
        public override void InitCommands()
        {
            RunCommand = base.CreateCommand(OnExecuteRunCommand);
            CloseCommand = base.CreateCommand(delegate { Application.Current.Shutdown(); });
        }

        private void OnExecuteRunCommand(object param)
        {
            AppManager.Current.Settings.Set("TargetFile", this.TargetFile);

            base.InvokeAsyncAction(delegate
            {
                if (SelectedTabItem.Equals(TabItems.ComponentInfo)
                    || SelectedTabItem.Equals(TabItems.GridColumnInfo))
                {
                    ViewModelLocator.Current.ComponentAnalysisVM.Run();
                    ViewModelLocator.Current.DataColumnAnalysisVM.Run();
                }
                else if (SelectedTabItem.Equals(TabItems.CallTree))
                {
                    ViewModelLocator.Current.CallTreeVM.Run();
                }
                else if (SelectedTabItem.Equals(TabItems.ClassList))
                {
                    ViewModelLocator.Current.ProjectConterVM.Run();
                }
                else if (SelectedTabItem.Equals(TabItems.SAFCodeGen))
                {
                    ViewModelLocator.Current.SAFCodeGenVM.Run();
                }
                else if (SelectedTabItem.Equals(TabItems.Suggest))
                {
                    ViewModelLocator.Current.SuggestVM.Run(); 
                }
            });
        }

        #endregion
    }
}
