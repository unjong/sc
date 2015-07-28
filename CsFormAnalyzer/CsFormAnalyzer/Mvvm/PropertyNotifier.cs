using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using CsFormAnalyzer.Utils;
using System.Diagnostics;
using CsFormAnalyzer.Foundation;

namespace CsFormAnalyzer.Mvvm
{
    /// <summary>
    /// 프로퍼티 변경통지를 지원합니다.
    /// </summary>
    public abstract class PropertyNotifier : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region simple property pattern

        private Dictionary<string, dynamic> properties;

        /// <summary>
        /// 프로퍼티를 가져옵니다.        
        /// </summary>
        public dynamic GetPropertyValue([CallerMemberName] string propertyName = null)
        {
            if (properties == null) properties = new Dictionary<string, dynamic>();

            if (properties.ContainsKey(propertyName))
                return properties[propertyName];
            else
            {
                var type = this.GetType().GetProperty(propertyName).PropertyType;
                var restoreAttribute = this.GetType().GetProperty(propertyName).GetCustomAttributes(typeof(RestoreValueAttribute), false).FirstOrDefault();
                if (restoreAttribute != null)
                {
                    var value = ((RestoreValueAttribute)restoreAttribute).Value;
                    properties.Add(propertyName, value);
                    return value;
                }

                var defaultAttribute = this.GetType().GetProperty(propertyName).GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault();
                if (defaultAttribute != null)
                {
                    var value = ((DefaultValueAttribute)defaultAttribute).Value;
                    properties.Add(propertyName, value);
                    return value;
                }

                var instanceNewAttribute = this.GetType().GetProperty(propertyName).GetCustomAttributes(typeof(InstanceNewAttribute), false).FirstOrDefault();
                if (instanceNewAttribute != null)
                {
                    var value = ((InstanceNewAttribute)instanceNewAttribute).NewInstance(type);
                    properties.Add(propertyName, value);
                    return value;
                }                    
                else
                    return type.GetDefaultValue();
            }
        }

        /// <summary>
        /// 프로퍼티를 설정하고 PropertyChanged 이벤트를 호출합니다. 
        /// </summary>
        public bool SetPropertyValue(dynamic value, [CallerMemberName] string propertyName = null, bool raiseChanged = true)
        {
            lock (this)
            {
                if (properties == null) properties = new Dictionary<string, dynamic>();

                bool isChanged = true;
                if (properties.ContainsKey(propertyName))
                {
                    var oldValue = properties[propertyName];
                    if (oldValue != null && oldValue.Equals(value))
                        isChanged = false;
                    else
                        properties[propertyName] = value;
                }
                else
                    properties.Add(propertyName, value);

                var type = this.GetType().GetProperty(propertyName).PropertyType;
                var restoreAttribute = this.GetType().GetProperty(propertyName).GetCustomAttributes(typeof(RestoreValueAttribute), false).FirstOrDefault();
                if (restoreAttribute != null)
                {
                    ((RestoreValueAttribute)restoreAttribute).Save(value);
                }

                if (raiseChanged && isChanged) OnPropertyChanged(propertyName);
                return isChanged;
            }
        }

        #endregion        

        #region constructor & initialize...

        [DebuggerStepThrough]
        public PropertyNotifier()
        {
            this.Init();
        }

        /// <summary>
        /// 초기화를 수행합니다.
        /// </summary>
        public virtual void Init() 
        {
            Task.Run(delegate()
            {
                Task.WaitAll
                (
                    Task.Run(delegate() { this.InitCommands(); }),
                    Task.Run(delegate() { this.InitProperties(); })
                );

            }).ContinueWith(_ =>
            {
                if (InitComplated != null)
                    InitComplated();

                this.InitAfter();
            });

            //this.InitCommands();
            //this.InitProperties();

            //if (InitComplated != null)
            //    InitComplated();

            //this.InitAfter();
        }

        /// <summary>
        /// 커맨드를 초기화합니다.
        /// </summary>
        public virtual void InitCommands() { }

        /// <summary>
        /// 프로퍼티를 초기화합니다.
        /// </summary>
        public virtual void InitProperties() { }

        /// <summary>
        /// 초기화가 모두 수행된 이후 작업을 진행합니다.
        /// </summary>
        public virtual void InitAfter() { }

        public delegate void VoidHandler();
        /// <summary>
        /// 초기화가 모두 끝나면 발생합니다.
        /// </summary>
        public event VoidHandler InitComplated;
        
        #endregion
    }


    /// <summary>
    /// 객체가 존재하지 않을때 새 인스턴스를 생성합니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
        
    public class InstanceNewAttribute : Attribute
    {
        public dynamic NewInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
