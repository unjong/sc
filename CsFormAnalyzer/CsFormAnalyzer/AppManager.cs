using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CsFormAnalyzer
{
	public class AppManager
	{
		private AppManager()
		{ 
		}

		private static AppManager _Current;
		public static AppManager Current
		{
			get
			{
				if (_Current == null)
				{
					_Current = new AppManager();
				}

				return _Current;
			}			
		}

		public LocalSettings Settings = new LocalSettings(@"UserSetting.config");
	}
}
