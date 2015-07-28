using CsFormAnalyzer.Controls;
using CsFormAnalyzer.Properties;
using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;

namespace CsFormAnalyzer.Mvvm
{
	public abstract class ViewModelBase : PropertyNotifier
    {
        #region fields & static

        static ScResxLocator scResxContext;
        public static ScResxLocator ScRsLocator
        {
            get
            {
                return ScResxLocator.Instance;
            }
        }
        
        #endregion

        #region Initialize...

        public override void Init()
        {
            IsShowProgressRing = true;
            base.Init();
        }

        public override void InitAfter()
        {
            base.InitAfter();

            IsShowProgressRing = false;
            IsLoaded = true;
        }

        #endregion

        #region Properties

        public virtual string Title { get; set; }
        public FrameworkElement ViewControl { get; set; }

        public bool DialogResult
        {
            get
            {
                var win = GetWindow();
                if (win == null && win.IsModal()) return false;

                return (bool)win.DialogResult;
            }
            set
            {
                var win = GetWindow();
                if (win == null) return;

                if (win.IsModal())
                    win.DialogResult = value;
                else
                    win.Close();
            }
        }

        public bool IsLoaded { get; set; }

        #endregion

        #region VM Pattern Support

        /// <summary>
        /// 명령객체를 새로 가져옵니다.
        /// </summary>
        public SimpleCommand CreateCommand(Action<object> onExcute, Func<bool> onCanExcute = null)
        {
            return new SimpleCommand(onExcute, onCanExcute);
        }

        /// <summary>
        /// 비동기 명령객체를 새로 가져옵니다.
        /// 비동기 작업수행간 이 커맨드의 CanExecute() 리턴이 false 가 됩니다.
        /// ((AsyncCommand)asyncCommand).IsBusyChanged += (c, b) => { ...logic... };
        /// </summary>
        internal AsyncCommand CreateAsyncCommand(Func<Task> onExcute)
        {
            return new AsyncCommand(onExcute);
        }

        public Action Close;

        public void OpenDialog(Type viewType, ViewModelBase vm, int width, int height, bool isModal = false)
        {
            OpenDialog(viewType, vm, width, height, null,null, isModal);
        }

        public bool OpenDialog(Type viewType, ViewModelBase vm, int width, int height, string title, FrameworkElement owner = null, bool isModal=false)
        {
            var view = ReflectionHelper.CreateNewObjectFactory<FrameworkElement>(viewType);
            view.DataContext = vm;
            
            var sw = new SCPopWinBase(vm);
            sw.layoutRootGrid.Children.Add(view);
            view.Tag = sw;
            if (!string.IsNullOrEmpty(title)) sw.Title = title;
            sw.Height = height;
            sw.Width = width;
            sw.Owner = owner as Window;
            if (owner != null)
                sw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            else
                sw.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            //sw.ResizeMode = curSettings.ResizeMode;
            //sw.WindowState = WindowState.Maximized;
            //sw.WindowStyle = WindowStyle.None;
            //sw.Topmost = curSettings.Topmost;
            //sw.Title = curSettings.Title;
            if (isModal)
                sw.ShowDialog();
            else
                sw.Show();

            return (sw.DialogResult==null||sw.DialogResult==false)?false:true;
        }

        public bool OpenConfirm(string message,int width, int height)
        {
           return OpenDialog(typeof(Controls.ConfirmView), new Controls.ConfirmVM() { Message = message }, width, height,"Confirm",null, true);
        }



        public void OpenDialog(Type viewType, ViewModelBase vm, FrameworkElement owner = null, bool isModal =false)
        {
            OpenDialog(viewType, vm, (int)owner.Width - 100, (int)owner.Height - 100, "", owner, isModal);
        }


        public void OpenMessage(string message)
        {
            OpenDialog(typeof(SCMessageWin), new SCMessageWinVM() { Message = message }, 600, 450, "MESSAGE ",null, true);
        }

        public void OpenMessage(string message, int width, int height)
        {
            OpenDialog(typeof(SCMessageWin), new SCMessageWinVM() { Message = message }, width, height, "MESSAGE",null, true);
        }

        /// <summary>
        /// 이 뷰모델의 윈도우에 팝업을 표시합니다.
        /// </summary>
        public void ShowPopup(UIElement element)
        {
            AppManager.Current.ShowPopup(element, GetWindow());
        }

        #endregion

        #region Wait & Progress

        private bool _IsShowProgressRing;        
        public bool IsShowProgressRing
        {
            get { return _IsShowProgressRing; }
            set
            {
                _IsShowProgressRing = value;
                OnPropertyChanged();
                                
                // 이 뷰모델의 시각계층에 Wait Adorner 를 붙입니다.                
                if (IsLoaded != true)
                {
                    AppManager.Current.BeginInvoke(DispatcherPriority.Normal, delegate
                    {
                        var win = GetWindow();
                        if (win == null) return;

                        var visual = win.Content as Visual;
                        Behaviours.WaitAdornerDecoratorBehavior.VisualAttached(visual, value);
                    });
                }
                else
                {
                    var win = GetWindow();
                    if (win == null) return;

                    var visual = win.Content as Visual;
                    Behaviours.WaitAdornerDecoratorBehavior.VisualAttached(visual, value);
                }
            }
        }

        /// <summary>
        /// 비동기 action 을 수행하는 동안 UI를 Wait 상태로 표시합니다.
        /// </summary>
        public async void InvokeAsyncAction(Action action, Action continueWith = null)
        {
            IsShowProgressRing = true;
            try
            {
                if (continueWith == null)
                {
                    await Task.Factory.StartNew(() => { action.Invoke(); });
                }
                else
                {
                    await Task.Factory.StartNew(() => { action.Invoke(); })
                        .ContinueWith(_ => continueWith.Invoke());
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                IsShowProgressRing = false;
            }
        }

        #endregion

        #region Methods

        public Window GetWindow()
        {
            Window win = null;

            if (this.ViewControl != null)
            {
                if (this.ViewControl is Window)
                    win = this.ViewControl as Window;
                else
                    win = Window.GetWindow(this.ViewControl);
            }

            if (win == null)
                win = Application.Current.Windows.Cast<Window>().Where(p => ((FrameworkElement)p.Content).DataContext == this).FirstOrDefault();

            return win;
        }

        #endregion        
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
