using CsFormAnalyzer.Data;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Types;
using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CsFormAnalyzer.ViewModels
{
    public class EtcToolsVM : ViewModelBase
    {
        #region Initialize...

        public EtcToolsVM()
        {
            InitCodeConvert();
        }

        #endregion

        #region Properties

        public string FromText
        {
            get { return GetPropertyValue(); }
            set
            {
                SetPropertyValue(value);

                ToText = ConvertByFromText(value);
            }
        }
        public string ToText { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public IEnumerable<string> ConvertTypes { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string SelectedConvertType 
        { 
            get { return GetPropertyValue(); } 
            set 
            { 
                SetPropertyValue(value);
                FillConvertDictionary(value);
            } 
        }
        public DataTable ConvertDictionary { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        #endregion

        #region Commands

        public ICommand UpateConvertDictionaryCommand { get; private set; }

        public override void InitCommands()
        {
            base.InitCommands();

            UpateConvertDictionaryCommand = this.CreateCommand(OnExecuteUpateConvertDictionaryCommand);
        }

        private void OnExecuteUpateConvertDictionaryCommand(object obj)
        {
            var dt = ConvertDictionary;

            var updateQuery = string.Format(@"
UPDATE TBL_CodeConvertDictionary
   SET Target=@Target, Pattern=@Pattern, Replacement=@Replacement, Type='{0}'
 WHERE seqID=@seqID", this.SelectedConvertType);

            var insertQuery = string.Format(@"
INSERT TBL_CodeConvertDictionary
       (Target, Pattern, Replacement, Type)
VALUES (@Target, @Pattern, @Replacement, '{0}')", this.SelectedConvertType);

            var deleteQuery = @"
DELETE TBL_CodeConvertDictionary
 WHERE seqID=@seqID";

            var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);
            var result = dt.Update(conn, updateQuery, insertQuery, deleteQuery);
            System.Windows.MessageBox.Show(string.Format("{0}개행 저장되었습니다.", result));
        }

        #endregion

        #region Methods - Code Convert
        
        private void InitCodeConvert()
        {
            //var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);
            //var query = "SELECT DISTINCT Type FROM TBL_CodeConvertDictionary";

            //var dt = new DataTable();
            //dt.Fill(conn, query);

            //this.ConvertTypes = from t in dt.AsEnumerable()
            //                    select t.Field<string>("Type");

            this.ConvertTypes = new string[] { "Facade" };
        }

        private void FillConvertDictionary(string type)
        {
            var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);
            var query = string.Format("SELECT * FROM TBL_CodeConvertDictionary WHERE Type='{0}'", type);

            var dt = new DataTable();
            dt.Fill(conn, query);

            ConvertDictionary = dt;
        }

        private string ConvertByFromText(string value)
        {
            var code = value;
            try
            {
                foreach (var row in this.ConvertDictionary.AsEnumerable())
                {
                    var target = row.ToStr("Target");
                    var pattern = row.ToStr("Pattern");
                    var replacement = row.ToStr("Replacement");
                    if (string.IsNullOrEmpty(target) && string.IsNullOrEmpty(pattern)) continue;
                    if (string.IsNullOrEmpty(replacement)) continue;

                    if (string.IsNullOrEmpty(target))
                        code = Regex.Replace(code, pattern, replacement);
                    else
                        code = code.Replace(target, replacement);
                }
                return code;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion
    }
}

