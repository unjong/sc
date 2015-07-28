using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CsFormAnalyzer.ViewModels
{
    partial class ModelGeneratorVM : ViewModelBase
    {
        #region Fields

        private int maxStep = 1;

        #endregion

        #region Properties

        [InstanceNew]
        public ModelInfo CurrentModelInfo { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        [InstanceNew]
        public ObservableCollection<ItemInfo> ItemInfoList { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public string ResultCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public int Step
        {
            get { return GetPropertyValue(); }
            set
            {
                SetPropertyValue(value);
                if (value == 1) GenerateModelCode();
            }
        }

        #endregion

        #region Commands

        public ICommand PreviousCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        
        public override void InitCommands()
        {
            PreviousCommand = base.CreateCommand(delegate { Step -= 1; }, delegate { return Step > 0; });
            NextCommand = base.CreateCommand(delegate { if (ValidateData()) Step += 1; }, delegate { return Step < maxStep; });
            ClearCommand = base.CreateCommand(delegate { ItemInfoList = new ObservableCollection<ItemInfo>(); });
        }

        #endregion

        #region Methods

        private bool ValidateData()
        {
            try
            {
                if (string.IsNullOrEmpty(this.CurrentModelInfo.Name))
                    throw new ArgumentException("Model Name 이 지정되지 않았습니다.");

                var bindingList = this.ItemInfoList.Where(p => (string.IsNullOrEmpty(p.PropertyName) != true));
                {   // 바인딩이 있는 아이템은 데이터 타입이 필수
                    var count = bindingList.Where(p => string.IsNullOrEmpty(p.DataType)).Count();
                    if (count > 0)
                    {
                        FillDataType();
                        return false;
                    }
                }
                
                //if (bindingList.Where(p => string.IsNullOrEmpty(p.PropertyName) || string.IsNullOrEmpty(p.DataType)).Count() > 0)
                //{
                //    throw new ArgumentException("PropertyName, DataType은 Null일 수 없습니다.");
                //}
            }
            catch (ArgumentException e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            return true;
        }

        private void FillDataType()
        {
            var bindingList = this.ItemInfoList.Where(p => (string.IsNullOrEmpty(p.PropertyName) != true));

            foreach (var item in bindingList.Where(p => string.IsNullOrEmpty(p.DataType)))
            {
                if (string.IsNullOrEmpty(item.CellType))
                {
                    item.DataType = "string";
                    continue;
                }

                if ("Check".IndexOf(item.CellType) >= 0)
                    item.DataType = "bool";

                else
                    item.DataType = "string";
            }
        }

        private void GenerateModelCode()
        {
            var bindingList = this.ItemInfoList.Where(p => (string.IsNullOrEmpty(p.PropertyName) != true));

            var parts = new List<string>();

            var sb = new StringBuilder();
            { // class start
                string syntax = string.Format(@"
    public class {0}Model : SAF.Model.SAFModel
    {{
", this.CurrentModelInfo.Name);
                sb.AppendLine(syntax);
            }

            var codeBlocks = new List<KeyValuePair<string, string>>();
            foreach (var item in bindingList)
            {
                if (string.IsNullOrEmpty(item.PropertyName)) continue;

                var binding = item.PropertyName.Trim();
                binding = binding.Left(1).ToUpper() + binding.Substring(1);

                string propertyName = binding;
                string dataType = item.DataType;

                if (parts.Contains(propertyName)) continue; // 이미 존재하는 프로퍼티 건너뛰기
                parts.Add(propertyName);

                if (string.IsNullOrEmpty(item.Title))
                {
                    var block = GetPropertyBlock(propertyName, dataType);
                    codeBlocks.Add(new KeyValuePair<string, string>(propertyName, block));
                }
                else
                {
                    var block = GetPropertyBlock(propertyName + "Title", "string", @"""" + item.Title + @"""");
                    codeBlocks.Add(new KeyValuePair<string, string>(propertyName, block));

                    if (string.IsNullOrEmpty(item.Hide) != true)
                    {
                        block = GetPropertyBlock(propertyName + "Hide", "bool", Convert.ToBoolean(item.Hide).ToString().ToLower());
                        codeBlocks.Add(new KeyValuePair<string, string>(propertyName, block));
                    }
                    if (string.IsNullOrEmpty(item.NotNull) != true)
                    {
                        block = GetPropertyBlock(propertyName + "NotNull", "bool", Convert.ToBoolean(item.NotNull).ToString().ToLower());
                        codeBlocks.Add(new KeyValuePair<string, string>(propertyName, block));
                    }
                    if (string.IsNullOrEmpty(item.ReadOnly) != true)
                    {
                        block = GetPropertyBlock(propertyName + "ReadOnly", "bool", Convert.ToBoolean(item.ReadOnly).ToString().ToLower());
                        codeBlocks.Add(new KeyValuePair<string, string>(propertyName, block));
                    }
                    if (string.IsNullOrEmpty(item.Length) != true)
                    {
                        block = GetPropertyBlock(propertyName + "Length", "double", Convert.ToDouble(item.Length).ToString() + "d");
                        codeBlocks.Add(new KeyValuePair<string, string>(propertyName, block));
                    }
                    if (string.IsNullOrEmpty(item.Width) != true)
                    {
                        block = GetPropertyBlock(propertyName + "Width", "double", Convert.ToDouble(item.Width).ToString() + "d");
                        codeBlocks.Add(new KeyValuePair<string, string>(propertyName, block));
                    }
                    if (string.IsNullOrEmpty(item.HorizontalAlignment) != true)
                    {
                        block = GetPropertyBlock(propertyName + "HorizontalAlignment", "HorizontalAlignment", "HorizontalAlignment." + item.HorizontalAlignment.ToCamel());
                        codeBlocks.Add(new KeyValuePair<string, string>(propertyName, block));
                    }
                    if (string.IsNullOrEmpty(item.BGColor) != true)
                    {
                        block = GetPropertyBlock(propertyName + "BGColor", "Color", item.BGColor);
                        codeBlocks.Add(new KeyValuePair<string, string>(propertyName, block));
                    }
                    if (string.IsNullOrEmpty(item.Format) != true)
                    {
                        block = GetPropertyBlock(propertyName + "Format", "string", item.Format);
                        codeBlocks.Add(new KeyValuePair<string, string>(propertyName, block));
                    }
                }
            }

            // 코드블럭을 클래스코드에 추가합니다.
            foreach (var codeBlock in codeBlocks)
            {
                sb.AppendLine(codeBlock.Value);
            }

            sb.AppendLine(@"    }"); // class end

            this.ResultCode = sb.ToString().RegexReplace("(\r\n){2,}", "\r\n\r\n"); // 2개 이상의 개행을 1개 개행으로 변경
        }

        private string GetPropertyBlock(string propertyName, string dataType, string defaultValue = null)
        {
            string _propertyName = string.Format("_{0}{1}", propertyName.Left(1).ToLower(), propertyName.Substring(1));

            if (string.IsNullOrEmpty(defaultValue) != true)
                defaultValue = string.Format(" = {0}", defaultValue);

            string codeBlock = string.Format(@"
        #region {0}

        private {2} {1}{3};
        public {2} {0}
        {{
            get
            {{
                return {1};
            }}
            set
            {{
                RaiseAndSetIfChanged(ref {1}, value);
            }}
        }}

        #endregion", propertyName, _propertyName, dataType, defaultValue);
            
            return codeBlock;            
        }

        #endregion
    }

    partial class ModelGeneratorVM : ViewModelBase
    {
        public class ModelInfo : PropertyNotifier
        {
            public string Name { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        }

        public class ItemInfo : PropertyNotifier
        {
            public string DataType { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string PropertyName { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string Title { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string DefaultValue { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string CellType { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string CellDataType { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string Hide { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string NotNull { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string ReadOnly { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string Length { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string Width { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string HorizontalAlignment { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string BGColor { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string Format { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        }
    }
}

