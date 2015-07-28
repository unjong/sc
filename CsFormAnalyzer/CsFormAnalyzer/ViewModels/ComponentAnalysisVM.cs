using CsFormAnalyzer.Core;
using CsFormAnalyzer.Utils;
using CsFormAnalyzer.Mvvm;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Data;
using System.Windows.Controls;

namespace CsFormAnalyzer.ViewModels
{
	class ComponentAnalysisVM : ViewModelBase
    {
        #region Properties

        public string TargetFile { get { return _TargetFile; } set { _TargetFile = value; OnPropertyChanged(); } }
        private string _TargetFile;

        public string SearchTypes { get { return _SearchTypes; } set { _SearchTypes = value; OnPropertyChanged(); } }
        private string _SearchTypes;

        public string SearchProperties { get { return _SearchProperties; } set { _SearchProperties = value; OnPropertyChanged(); } }
        private string _SearchProperties;

        public string SearchEvents { get { return _SearchEvents; } set { _SearchEvents = value; OnPropertyChanged(); } }
        private string _SearchEvents;

        public string RemovePrefixs { get { return _RemovePrefixs; } set { _RemovePrefixs = value; OnPropertyChanged(); } }
        private string _RemovePrefixs;

        public string SelectorTypes { get { return _SelectorTypes; } set { _SelectorTypes = value; OnPropertyChanged(); } }
        private string _SelectorTypes;

        public string ExceptValues { get { return _ExceptValues; } set { _ExceptValues = value; OnPropertyChanged(); } }
        private string _ExceptValues;

        public IEnumerable ComponentInfoList { get { return _ComponentInfoList; } set { _ComponentInfoList = value; OnPropertyChanged(); } }
        private IEnumerable _ComponentInfoList;

        private object _SelectedComponentInfo;
        public object SelectedComponentInfo
        {
            get
            {
                return _SelectedComponentInfo;
            }
            set
            {
                _SelectedComponentInfo = value;
                OnPropertyChanged();

                var drv = value as DataRowView;
                if (drv == null) return;

                this.FilteredPropertyList = this.PropertyList
                    .Where(p => string.IsNullOrEmpty(Convert.ToString(drv.Row["name"])) != true
                        && p.Name.Equals(Convert.ToString(drv.Row["name"]))).Cast<ComponentAnalysis.ComponentPropertyInfo>();
                this.SelectedPropertyListItem = this.FilteredPropertyList.Where(p => p.Line == Convert.ToString(drv.Row["line"])).FirstOrDefault();
            }
        }


        public IEnumerable CheckLines { get { return _CheckLines; } set { _CheckLines = value; OnPropertyChanged(); } }
        private IEnumerable _CheckLines;

        public IEnumerable<ComponentAnalysis.ComponentPropertyInfo> FilteredPropertyList { get { return _FilteredPropertyList; } set { _FilteredPropertyList = value; OnPropertyChanged(); } }
        private IEnumerable<ComponentAnalysis.ComponentPropertyInfo> _FilteredPropertyList;

        private ComponentAnalysis.ComponentPropertyInfo _SelectedPropertyListItem;
        public ComponentAnalysis.ComponentPropertyInfo SelectedPropertyListItem
        {
            get
            {
                return _SelectedPropertyListItem;
            }
            set
            {
                _SelectedPropertyListItem = value;
                OnPropertyChanged();

                if (value != null)
                {
                    this.SelectedLine = value.Line;
                }
            }
        }

        public string FullCode { get { return _FullCode; } set { _FullCode = value; OnPropertyChanged(); } }
        private string _FullCode;

        public List<ComponentAnalysis.ComponentPropertyInfo> PropertyList { get { return _PropertyList; } set { _PropertyList = value; OnPropertyChanged(); } }
        private List<ComponentAnalysis.ComponentPropertyInfo> _PropertyList;

        public string SelectedLine { get { return _SelectedLine; } set { _SelectedLine = value; OnPropertyChanged(); } }
        private string _SelectedLine;

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand ClearComand { get; private set; }
        
        public override void InitCommands()
        {
            this.RefreshCommand = base.CreateCommand(OnExecuteRefreshCommand);
            this.SaveCommand = base.CreateCommand(OnExecuteSaveCommand);
            this.ClearComand = base.CreateCommand(OnExecuteClearComand);

            OnExecuteRefreshCommand(null);
        }

        private void OnExecuteRefreshCommand(object param)
        {
            var path = System.IO.Path.Combine(Properties.SCResxControlText.ConfigPath, @"ComponentAnalysisConfig.xml");
            var config = Utils.XmlSerialize.XmlToObject(path, typeof(ComponentAnalysisConfig)) as ComponentAnalysisConfig;
            if (config == null)
            {
                config = new ComponentAnalysisConfig();
                config.LoadDefaultConfig();

                Utils.XmlSerialize.ObjectToXml(path, config);
            }

            this.SearchTypes = config.SearchTypes;
            this.SearchProperties = config.SearchProperties;
            this.SearchEvents = config.SearchEvents;
            this.RemovePrefixs = config.RemovePrefixs;
            this.SelectorTypes = config.SelectorTypes;
            this.ExceptValues = config.ExceptValues;
        }

        private void OnExecuteSaveCommand(object param)
        {
            var path = System.IO.Path.Combine(Properties.SCResxControlText.ConfigPath, @"ComponentAnalysisConfig.xml");
            var config = new ComponentAnalysisConfig()
            {
                SearchTypes = this.SearchTypes,
                SearchProperties = this.SearchProperties,
                SearchEvents = this.SearchEvents,
                RemovePrefixs = this.RemovePrefixs,
                SelectorTypes = this.SelectorTypes,
                ExceptValues = this.ExceptValues,
            };
            Utils.XmlSerialize.ObjectToXml(path, config);
        }

        private void OnExecuteClearComand(object obj)
        {
            ComponentInfoList = null;
        }

        #endregion

        #region Methods

        internal void Run()
        {
#if !DEBUG
			try
			{				
#endif
            //var path = @"D:\720 연세의료원\His2\HIS2.0\Source\WinUI\SP\PHA\HIS.WinUI.SP.PHA.AM.Furn\TdrgCnslMFNew.cs";
            var path = TargetFile;
            var ca = new ComponentAnalysis()
            {
                Path = path,
                SearchTypes = this.SearchTypes.Replace(" ", "").Split(','),
                SearchProperties = this.SearchProperties.Replace(" ", "").Split(','),
                SearchEvents = this.SearchEvents.Replace(" ", "").Split(','),
                RemovePrefixs = this.RemovePrefixs.Replace(" ", "").Split(','),
                SelectorTypes = this.SelectorTypes.Replace(" ", "").Split(','),
                ExceptValues = this.ExceptValues != null ? this.ExceptValues.Replace(" ", "").Split(',') : new string[] { },
            };
            var bSuccess = ca.Run();
            if (bSuccess)
            {
                this.ComponentInfoList = ca.ResultTable.DefaultView;
                this.CheckLines = ca.CheckLines;
                this.FullCode = ca.Code;
                this.PropertyList = ca.PropertyList;
            }
            else
            {
                this.ComponentInfoList = null;
                this.CheckLines = null;
                this.FullCode = null;
                this.PropertyList = null;
            }
#if !DEBUG
			}
			catch(Exception ex)
			{
				this.ComponentInfoList = null;
                System.Windows.MessageBox.Show(ex.SxGetErrorMessage());
			}
#endif
        }

        #endregion        
	}
}
