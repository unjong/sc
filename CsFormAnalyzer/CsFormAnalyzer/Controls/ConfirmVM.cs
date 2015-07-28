using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Controls
{
    public class ConfirmVM : Mvvm.ViewModelBase
    {

        #region Message

        private string _Message;
        ///
        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                this._Message = value;
                OnPropertyChanged("Message");
            }
        }

        #endregion

    }
}
