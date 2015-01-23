using CsFormAnalyzer.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace CsFormAnalyzer.Mvvm
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		public void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public SimpleCommand CreateCommand(Action onExcute)
		{
			return new SimpleCommand(onExcute);
		}

		internal AsyncCommand CreateAsyncCommand(Func<Task> onExcute)
		{
			return new AsyncCommand(onExcute);
		}

        #region IsShowProgressRing
        private bool _IsShowProgressRing;
        public bool IsShowProgressRing
        {
            get { return _IsShowProgressRing; }
            set
            {
                _IsShowProgressRing = value;
                OnPropertyChanged();

				// 메인윈도우의 최상위 시각계층을 찾아서 Wait Adorner 를 붙입니다.
                var mainWin = Application.Current.Windows.Cast<Window>().Where(p => p is MainWindow).FirstOrDefault();
                var visual = mainWin.Content as Visual;

                var behavior = Interaction.GetBehaviors(mainWin.Content as Visual).Where(p => p.GetType().Equals(typeof(Behaviors.WaitAdornerDecoratorBehavior))).FirstOrDefault() as Behaviors.WaitAdornerDecoratorBehavior;
                if (behavior == null)
                {
                    behavior = new Behaviors.WaitAdornerDecoratorBehavior();
                    Interaction.GetBehaviors(visual).Add(behavior);
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    behavior.IsActive = value;
                }));
            }
        } 
        #endregion


        static ScResxLocator scResxContext;
        public static ScResxLocator ScRsLocator
        {
            get
            {
                return ScResxLocator.Instance;
            }
        }

		/// <summary>
		/// 비동기 action 을 수행하는 동안 UI를 Wait 상태로 표시합니다.
		/// </summary>
		public async void InvokeAsyncAction(Action action)
		{
			IsShowProgressRing = true;

			await Task.Factory.StartNew(() => { action.Invoke(); });

			IsShowProgressRing = false;
		}
	}

    public class ScResxLocator : INotifyPropertyChanged
    {
        #region Instance
        // Static Instance;
        private static ScResxLocator mInstance;

        public static ScResxLocator Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ScResxLocator();
                }

                return mInstance;
            }
        } 
        #endregion


        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseCultureChanged(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = cultureInfo;
            OnPropertyChanged("ResxText");
        }

        SCResxControlText mEzResxControlText = new SCResxControlText();
        /// <summary>
        ///  컨트롤 lable 정보 제공
        /// </summary>
        public SCResxControlText ResxText
        {
            get { return mEzResxControlText; }
        }
    }
}
