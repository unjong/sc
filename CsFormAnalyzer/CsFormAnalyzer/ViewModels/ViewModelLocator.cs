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

		private Dictionary<Type, object> instances = new Dictionary<Type, object>();

		private T GetInstance<T>()
		{			
			if (instances.ContainsKey(typeof(T)))
			{
				return (T)instances[typeof(T)];
			}
			else
			{
				var instance = Activator.CreateInstance<T>();
				instances.Add(typeof(T), instance);
				return instance;
			}
		}

		public MainWindowVM MainWindowVM { get { return GetInstance<MainWindowVM>(); } }

		public ComponentAnalysisVM ComponentAnalysisVM { get { return GetInstance<ComponentAnalysisVM>(); } }

		public DataColumnAnalysisVM DataColumnAnalysisVM { get { return GetInstance<DataColumnAnalysisVM>(); } }

        public SAFCodeGenViewModel SAFCodeGenVM { get { return GetInstance<SAFCodeGenViewModel>(); } }	
		
	}
}