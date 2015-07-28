using CsFormAnalyzer.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CsFormAnalyzer.ViewModels
{
	class ViewModelLocator
	{
		public static ViewModelLocator Current { get { return Application.Current.Resources["ViewModelLocator"] as ViewModelLocator; } }

		private Dictionary<Type, object> instanceCache = new Dictionary<Type, object>();

		public T GetInstance<T>(bool isNew = false)
		{
            if (isNew)
            {
                var instance = Activator.CreateInstance<T>();
                return instance;
            }
            else
            {
                if (instanceCache.ContainsKey(typeof(T)))
                {
                    return (T)instanceCache[typeof(T)];
                }
                else
                {
                    var instance = Activator.CreateInstance<T>();
                    instanceCache.Add(typeof(T), instance);
                    return instance;
                }
            }
		}

		public MainWindowVM MainWindowVM { get { return GetInstance<MainWindowVM>(); } }

		public ComponentAnalysisVM ComponentAnalysisVM { get { return GetInstance<ComponentAnalysisVM>(); } }

		public DataColumnAnalysisVM DataColumnAnalysisVM { get { return GetInstance<DataColumnAnalysisVM>(); } }

        public SAFCodeGenViewModel SAFCodeGenVM { get { return GetInstance<SAFCodeGenViewModel>(); } }
        public SuggestViewModel SuggestVM { get { return GetInstance<SuggestViewModel>(); } }        

        public ProjectConterVM ProjectConterVM { get { return GetInstance<ProjectConterVM>(); } }

        public CallTreeVM CallTreeVM { get { return GetInstance<CallTreeVM>(); } }

        public CodeGenViewModelVM CodeGenViewModelVM { get { return GetInstance<CodeGenViewModelVM>(); } }

        public EtcToolsVM EtcToolsVM { get { return GetInstance<EtcToolsVM>(); } }        
    }
}