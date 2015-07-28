using CsFormAnalyzer.Core;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Types;
using CsFormAnalyzer.Utils;
using CsFormAnalyzer.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CsFormAnalyzer.ViewModels
{
    internal partial class CodeGenViewModelVM : ViewModelBase
    {
        #region Fields

        private int maxStep = 1;

        #endregion

        #region Properties

        public int Step
        {
            get { return GetPropertyValue(); }
            set
            {
                SetPropertyValue(value);
                if (value == 1)
                {
                    GenerateViewModelCode();
                }
            }
        }

        [InstanceNew]
        public VmInfo ViewModelInfo { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        [InstanceNew]
        public List<VmItemInfo> VmItemInfoList { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public string ViewModelCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        
        #endregion

        #region Commands

        public ICommand PreviousCommand { get; private set; }
        public ICommand NextCommand { get; private set; }        
        public ICommand ClearCommand { get; private set; }
        public ICommand OpenModelGeneratorCommand { get; private set; }
        public ICommand OpenCodeConverterCommand { get; private set; }
        
        public override void InitCommands()
        {
            PreviousCommand = base.CreateCommand(delegate { Step -= 1; }, delegate { return Step > 0; });
            NextCommand = base.CreateCommand(delegate { if (ValidateData()) Step += 1; }, delegate { return Step < maxStep; });            
            ClearCommand = base.CreateCommand(delegate { VmItemInfoList = new List<VmItemInfo>(); });

            OpenModelGeneratorCommand = base.CreateCommand(OnExecuteOpenModelGeneratorCommand);
            OpenCodeConverterCommand = base.CreateCommand(OnExecuteOpenCodeConverterCommand);
        }

        private void OnExecuteOpenModelGeneratorCommand(object obj)
        {
            var vm = ViewModelLocator.Current.GetInstance<ModelGeneratorVM>(true);
            AppManager.Current.ShowView(typeof(ModelGeneratorView), vm);
        }

        private void OnExecuteOpenCodeConverterCommand(object obj)
        {
            var vm = ViewModelLocator.Current.GetInstance<CodeConverterVM>(true);

            var file = ViewModelLocator.Current.MainWindowVM.TargetFile;
            var code = IOHelper.ReadFileToString(file);
            vm.Namespace = code.Between("namespace ", "\r").Trim();

            var items = this.VmItemInfoList.Where(p => string.IsNullOrEmpty(p.Binding) != true
                && string.IsNullOrEmpty(p.DataType) != true
                && string.IsNullOrEmpty(p.Name) != true);

            foreach (var item in items)
            {
                string key;
                var name = item.Name;
                var target = item.Target;
                var dataType = item.DataType;

                if (dataType.Equals("DateTime") && target.Equals("Date"))
                {
                    vm.ConvertDictionaryByBinding.Add(new Types.MatchReplaceDelegator(string.Format(@"{0}\.{1}", name, "Value"), item.Binding));
                    vm.ConvertDictionaryByBinding.Add(new Types.MatchReplaceDelegator(string.Format(@"{0}\.{1}", name, "Text"), item.Binding));
                    continue;
                }
                else if (item.ElementType.Equals("ComboBox") && target.Equals("SelectedItem"))
                {
                    vm.ConvertDictionaryByBinding.Add(new Types.MatchReplaceDelegator(string.Format(@"{0}\.{1}", name, "Text"), item.Binding));
                    vm.ConvertDictionaryByBinding.Add(new Types.MatchReplaceDelegator(string.Format(@"{0}\.{1}", name, "SelectedValue"), item.Binding));
                }
                else if (item.ElementType.Equals("Button") && target.Equals("Content"))
                    target = "Text";
                else if (target.Equals("IsChecked"))    // chbDispGb1.Checked
                    target = "Checked";
                else if (target.Equals("ItemsSource"))  // spdCelList.DataSource
                    target = "DataSource";
                else if (target.Equals("SelectedItem"))
                    target = "SelectedValue";

                key = string.Format(@"{0}\.{1}", name, target);
                var value = item.Binding;
                vm.ConvertDictionaryByBinding.Add(new Types.MatchReplaceDelegator(key, value));
            }

            AppManager.Current.ShowView(typeof(CodeConverterView), vm);
        }

        #endregion

        #region Methods

        private bool ValidateData()
        {
            try
            {
                if (string.IsNullOrEmpty(this.ViewModelInfo.Name))
                    throw new ArgumentException("ViewModel Name 이 지정되지 않았습니다.");

                var bindingList = this.VmItemInfoList.Where(p => (string.IsNullOrEmpty(p.Binding) != true));
                {   // Name 추적
                    var list = bindingList.Where(p => string.IsNullOrEmpty(p.Name));
                    if (list.Count() > 0)
                    {
                        var dataView = ViewModelLocator.Current.ComponentAnalysisVM.ComponentInfoList as System.Data.DataView;
                        if (dataView != null)
                        {
                            foreach (var item in list)
                            {
                                var drv = dataView.Cast<DataRowView>().Where(p => p.Row.ToStr("바인딩").Equals(item.Binding)).FirstOrDefault();
                                if (drv != null)
                                {
                                    item.Name = drv.Row.ToStr("Name");
                                }
                            }
                            var count = list.Where(p => string.IsNullOrEmpty(p.Binding)).Count();
                            if (count > 0)
                                //throw new ArgumentException(string.Format("ComponentInfo 에서 추측할 수 없는 이름이 {0}개 남았습니다.", count));
                                MessageBox.Show(string.Format("ComponentInfo 에서 추측할 수 없는 이름이 {0}개 남았습니다.", count));
                            else
                                //throw new ArgumentException(string.Format("ComponentInfo 에서 추측할 수 없는 이름이 {0}개 남았습니다.", list.Count()));
                                MessageBox.Show(string.Format("ComponentInfo 에서 추측할 수 없는 이름이 {0}개 남았습니다.", list.Count()));

                            if (MessageBox.Show("그냥 진행 할까요?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                return false;
                        }
                    }
                }

                int ndx = 0;
                string name = string.Empty;
                string elementType = string.Empty;
                foreach (var item in this.VmItemInfoList.ToArray())
                {
                    if (string.IsNullOrEmpty(item.Target) && string.IsNullOrEmpty(item.Binding))
                    {
                        this.VmItemInfoList.Remove(item);
                        continue;
                    }

                    if (string.IsNullOrEmpty(item.ElementType))
                    {
                        item.ElementType = elementType;
                        if (string.IsNullOrEmpty(item.Name)) item.Name = name;
                    }
                    else
                    {
                        elementType = item.ElementType;
                        if (string.IsNullOrEmpty(item.Name)) item.Name = string.Format("#{0}", ++ndx);
                        name = item.Name;
                    }
                }

                {   // 바인딩이 있는 아이템은 데이터 타입이 필수
                    var count = bindingList.Where(p => string.IsNullOrEmpty(p.DataType)).Count();
                    if (count > 0)
                    {
                        //throw new ArgumentException("지정되지 않은 DataType이 있습니다.");
                        FillDataType();
                        return true;
                    }
                }

                {   // 바인딩 이름은 Property, Command 로 끝나야 한다.
                    var list = bindingList.Where(p => (p.Binding.EndsWith("Property") || p.Binding.EndsWith("Command")) != true);
                    if (list.Count() > 0)
                    {
                        var msg = string.Format("바인딩 이름은 Property, Command로 끝나야 합니다. - {0}", string.Join(",", list.Select(p => p.Binding)));
                        throw new ArgumentException(msg);
                    }
                }

                {   // 바인딩 이름은 대문자로 시작해야 한다.
                    var where = bindingList.Where(p => char.IsUpper(Convert.ToChar(p.Binding.Left(1))) != true);
                    if (where.Count() > 0)
                    {
                        throw new ArgumentException(string.Format("바인딩 이름은 대문자로 시작해야 합니다. - {0}", where.ElementAt(0).Binding));
                    }
                }

                foreach (var item in this.VmItemInfoList.Where(p => p.ElementType.Contains("RadioButton", "CheckBox")))
                {                    
                    if (this.VmItemInfoList.Where(p => p.Name.Equals(item.Name) && p.Target.Equals("IsChecked") && string.IsNullOrEmpty(p.Binding) != true).Count() < 1)
                    {
                        throw new ArgumentException(string.Format("{0} - 이 컨트롤에 IsCheckedProperty 바인딩이 필요합니다.", item.Name));
                    }
                }

                foreach (var item in this.VmItemInfoList.Where(p => new string[] { "Button" }.Contains(p.ElementType)))
                {
                    if (this.VmItemInfoList.Where(p => p.Name.Equals(item.Name) && "Command".Equals(p.Target) && string.IsNullOrEmpty(p.Binding) != true).Count() < 1)
                    {
                        throw new ArgumentException(string.Format("{0} - 이 컨트롤에 Command 바인딩이 필요합니다.", item.Name));
                    }
                }

                //{   // 바인딩 이름 중복 체크 - 중복 허용 해야함
                //    var bindingNames = bindingList.Select(p => p.Binding);
                //    if (bindingNames.Distinct().Count() < bindingNames.Count())
                //    {
                //        var checkList = new List<string>();
                //        foreach (var name in bindingNames)
                //        {
                //            if (checkList.Contains(name))
                //            {
                //                throw new ArgumentException(string.Format("바인딩 이름은 중복 될 수 없습니다. - {0}", name));
                //            }
                //            checkList.Add(name);
                //        }
                //    }
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
            foreach (var item in this.VmItemInfoList.OrderBy(p => p.Binding).ToArray())
            {
                if (string.IsNullOrEmpty(item.Binding)) continue;

                var binding = item.Binding.Trim();
                var target = item.Target.Trim();
                if (string.IsNullOrEmpty(target))
                    throw new ArgumentException("Traget은 Null일 수 없습니다.");

                if ((binding.Equals("바인딩") && target.Equals("대상"))
                    || (binding.Equals("DataType") && target.Equals("Title")))
                {
                    this.VmItemInfoList.Remove(item);
                    continue;
                }

                if (target.Contains("Text", "Content", "Header", "CustomFormat", "DisplayMember"))
                    item.DataType = "string";
                else if (target.Contains("ReadOnly", "Enabled", "IsChecked", "Focus", "IsFocused"))
                    item.DataType = "bool";
                else if (target.Contains("SelectedIndex"))
                    item.DataType = "int";
                else if (target.Contains("Command"))
                    item.DataType = "SAFCommand";
                else if (target.Contains("ItemsSource"))
                    item.DataType = string.Format("SAF.Client.SAFCollection<{0}>", item.Binding.LeftBySearch("ListProperty"));
                else if (target.Contains("SelectedItem"))
                    item.DataType = item.Binding.Between("Selected", "Property");
                else if (target.Contains("Date"))
                    item.DataType = "DateTime";
                else if (target.Contains("Visible"))
                    item.DataType = "Visibility";
                else if (target.Contains("MaxLength", "Value"))
                    item.DataType = "double";
                else if (target.Contains("Image"))
                    item.DataType = "ImageSource";
                else if (target.Contains("ViewModel"))
                    item.DataType = "SAFViewModel";
                else if (target.Contains("ForeColor", "BackColor"))
                    item.DataType = "Brush";
                else if (target.Contains("ContextMenu"))
                    item.DataType = "ContextMenu";
                else if (target.Contains("RowStyleSelector", "CellStyleSelector", "StyleSelector"))
                    item.DataType = "StyleSelector";
                else if (target.Contains("Format", "ValueMember", "FrozenColumnCount"))
                    item.Binding = null;
                else
                {
#if DEBUG
                    Debugger.Break();
                    item.DataType = target;
#else
                    //throw new ArgumentException(string.Format("지정되지 않은 DataType '{0}'이 있습니다.", target));                                    
                    item.DataType = target;
#endif
                }
            }
        }

        private void GenerateViewModelCode()
        {
            string path = ViewModelLocator.Current.CallTreeVM.TargetFile;
            //string ns = path.Remove(path.LastIndexOf(@"\")).RightBySearch(@"\");
            var ns = WorkService.Current.GetNamespaceByFilePath(path);

            string classNM = Path.GetFileNameWithoutExtension(path);
            string nsclass = ns + "." + classNM;
            string[] nsArray = ns.Split('.');
            string hpOrSp = nsArray.ElementAt(2);

            var parts = new List<string>();                        
            var sb = new StringBuilder();

            sb.AppendLine("// ================================================");
            sb.AppendLine("// " + ViewModelInfo.Name + "ViewModel --");
            sb.AppendLine("//");
            sb.AppendLine("// [AS-IS] " + nsclass);
            sb.AppendLine("// -----------------------------------------------");
            sb.AppendLine("// " + DateTime.Today.Date.ToShortDateString() + " 작성자(" + Environment.UserName + ")");
            sb.AppendLine("// ===============================================");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");            
            sb.AppendLine("using System.Windows;");
            sb.AppendLine("using System.Windows.Media;");
            sb.AppendLine("using SAF.Client;");
            sb.AppendLine("using SAF.Client.Extensions;");
            sb.AppendLine("using SAF.Client.Helpers;");
            sb.AppendLine("using His3.Common.Client.Utils;");
            sb.AppendLine("using His3.Common.Client.Utils.Behavior;");
            sb.AppendLine("using His3.Hp.Bridge.Common.Ut;");
            sb.AppendLine("using His3.Hp.Client.Common.Ut;");
            if (hpOrSp.Equals("Sp", StringComparison.CurrentCultureIgnoreCase))
            {
                sb.AppendLine("using His3.Sp.Model.Common;");
                sb.AppendLine("using His3.Sp.Client.Common.Ut;");
            }
            else
            {
                sb.AppendLine("using His3.Hp.Model.Common;");
                sb.AppendLine("using His3.Hp.Client.Com.Ut;");
            }

            // 모델 참조 코드
            string[] tmp = ns.Split('.');
            string location = "His3." + tmp[2].Substring(0, 1) + tmp[2].Substring(1, tmp[2].Length - 1).ToLower() + ".Model";
            for (int i = 3; i < tmp.Length; i++)
            {
                tmp[i] = tmp[i].Replace("ZZZ", "Etc");

                if (i == tmp.Length) location += "." + tmp[i];
                else location += "." + tmp[i].ToCamel();
            }

            //sb.AppendLine("using " + location + ";");
            sb.AppendLine();
            sb.AppendLine("namespace " + location.Replace("Model", "Client"));
            sb.Append("{");
            //{
            // class start
            string classSyntax = string.Format(@"
    public class {0}ViewModel : SAF.Client.SAFViewModel, ISAFViewSettingDefault
    {{
        #region ISAFViewSettingDefault - DefaultViewSetting

        public SAFViewSettings DefaultViewSetting
        {{
            get {{ return new SAFViewSettings(); }}
        }}

        #endregion
", this.ViewModelInfo.Name);
            sb.Append(classSyntax);

            var codeBlocks = new List<KeyValuePair<string, string>>();
            foreach (var item in this.VmItemInfoList)
            {
                if (string.IsNullOrEmpty(item.Binding)
                    || string.IsNullOrEmpty(item.DataType)
                    ) continue;

                var binding = item.Binding.Trim();
                binding = binding.Left(1).ToUpper() + binding.Substring(1);

                if (binding.EndsWith("Property"))
                {
                    #region write property
                    string propertyName = binding;
                    string _propertyName = string.Format("_{0}{1}", propertyName.Left(1).ToLower(), propertyName.Substring(1));
                    string dataType = item.DataType;
                    string privateSetter = string.Empty;

                    if (parts.Contains(propertyName)) continue; // 이미 존재하는 프로퍼티 건너뛰기
                    parts.Add(propertyName);

                    string getText = string.Empty;
                    if (dataType.IndexOf("<") >= 0)
                    {
                        var collType = dataType.LeftBySearch("<");
                        var type = dataType.Between("<", ">");                        
                        getText = string.Format(@"
                if ({0} == null)
                    {0} = new {1}<{2}>();", _propertyName, collType, type);
                    }
                    else if (dataType.Equals("string"))
                        privateSetter = " = string.Empty";

                    else if (dataType.Equals("DateTime"))
                        privateSetter = " = DateTime.Now";

                    string codeBlock = string.Format(@"
        #region {0}

        private {2} {1}{4};
        public {2} {0}
        {{
            get
            {{{3}
                return {1};
            }}
            set
            {{
                RaiseAndSetIfChanged(ref {1}, value);
            }}
        }}

        #endregion", propertyName, _propertyName, dataType, getText, privateSetter);
                    codeBlocks.Add(new KeyValuePair<string, string>("Property", codeBlock));
                    #endregion
                }
                else if (binding.EndsWith("Command"))
                {
                    #region write command
                    string commandName = binding;
                    string _commandName = string.Format("_{0}{1}", commandName.Left(1).ToLower(), commandName.Substring(1));

                    if (parts.Contains(commandName)) continue; // 이미 존재하는 커맨드 건너뛰기
                    parts.Add(commandName);

                    var msg = commandName.Equals("CloseCommand") 
                        ? "this.Close();"
                        : string.Format("// TODO : [개발예정] Write Command {0}", commandName);

                    string codeBlock = string.Format(@"
        #region {0}

        private SAF.Client.SAFCommand {1};
        /// <summary>
        /// 무슨 무슨 행위를 하는 커멘드
        /// </summary>
        public SAF.Client.SAFCommand {0}
        {{
            get
            {{
                if ({1} == null)
                    {1} = CreateCommand(CanExec{0}, Exec{0});
                return {1};
            }}
        }}

        private void Exec{0}(object arg)
        {{
            {2}
        }}

        private bool CanExec{0}(object obj)
        {{
            return true;
        }}

        #endregion", commandName, _commandName, msg);
                    //throw new NotImplementedException();
                    codeBlocks.Add(new KeyValuePair<string, string>("Command", codeBlock));
                    #endregion
                }
                else
                    MessageBox.Show("바인딩 이름은 Property 또는 Command 로 끝나야 합니다.");
            }

            #region Insert Lines by Property

            sb.AppendLine(@"
        #region Binding Properties");

            sb.AppendLine(@"
        #region ControlSetters
 
        private ControlSetters _contrlSetter;
        public ControlSetters ControlSetters
        {
            get
            {
                if (_contrlSetter == null)
                    _contrlSetter = new ControlSetters();
                return _contrlSetter;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _contrlSetter, value);
            }
        }
 
        #endregion
");

            // 코드블럭을 클래스코드에 추가합니다.
            foreach (var codeBlock in codeBlocks.Where(p => p.Key.Equals("Property")))
            {
                sb.AppendLine(codeBlock.Value);
            }

            sb.AppendLine(@"
        #endregion");

            #endregion

            #region Insert Lines by Commands

            sb.AppendLine(@"
        #region Commands");

            sb.AppendLine(@"
        #region LoadedCommand

        private SAF.Client.SAFCommand _LoadedCommand;
        /// <summary>
        /// 화면 로드
        /// </summary>
        public SAF.Client.SAFCommand LoadedCommand
        {
            get
            {
                if (_LoadedCommand == null)
                    _LoadedCommand = CreateCommand(CanExecLoadedCommand, ExecLoadedCommand);
                return _LoadedCommand;
            }
        }

        private void ExecLoadedCommand(object arg)
        {
            // TODO : [개발예정] Write Command LoadedCommand
        }

        private bool CanExecLoadedCommand(object obj)
        {
            return true;
        }

        #endregion
");
            foreach (var codeBlock in codeBlocks.Where(p => p.Key.Equals("Command")))
            {
                sb.AppendLine(codeBlock.Value);
            }

            sb.AppendLine(@"
        #endregion");

            #endregion

            sb.AppendLine(@"    }"); // class end
            sb.AppendLine("}");
            this.ViewModelCode = sb.ToString().RegexReplace("(\r\n){2,}", "\r\n\r\n"); // 2개 이상의 개행을 1개 개행으로 변경
        }
        
        #endregion
    }

    internal partial class CodeGenViewModelVM : ViewModelBase
    {
        public class VmInfo : PropertyNotifier
        {
            public string Name { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        }

        public class VmItemInfo : PropertyNotifier
        {
            public string ElementType { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string Target { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string Value { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string ResourceKey { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string Binding { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }            
            public string DataType { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public string Name { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        }

        public interface IGenResult
        {
        }
    }
}

