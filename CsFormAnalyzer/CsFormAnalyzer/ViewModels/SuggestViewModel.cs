using CsFormAnalyzer.Core;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace CsFormAnalyzer.ViewModels
{
    public class SuggestViewModel : ViewModelBase
    {
        #region Properties...

        public string TargetFile { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public IEnumerable<ISuggestResult> Results { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        //public ICommand RunCommand { get; set; }

        public override void InitProperties()
        {
            base.InitProperties();

            //this.RunCommand = new AsyncCommand(() => Task.Run(delegate { ActRun(); }));
            //this.RunCommand = new SimpleCommand(delegate { ActRun(); });
        }

        #endregion

        #region Methods...

        public void Run()
        {
            Results = GetResults().ToArray();
        }

        private IEnumerable<ISuggestResult> GetResults()
        {
            if (this.TargetFile.IndexOf("Client") > 0) // TO-BE
            {
                SuggestHelper.ReadyTobe(this.TargetFile);

                var code = IOHelper.ReadFileToString(this.TargetFile);
                if (this.TargetFile.EndsWith(".xaml")) // View
                {
                    yield return new SuggestXamlLocalization(code, this.TargetFile);
                    yield return new SuggestBinding(code, this.TargetFile);
                    yield return new SuggestXamlHint(code, this.TargetFile);
                }
                else // ViewModel
                {
                    yield return new SuggestLocalization(code, this.TargetFile);
                    yield return new SuggestTodoList(code);
                    yield return new SuggestCodeHint(code, this.TargetFile);
                    yield return new SuggestHintFromAsis(code, this.TargetFile);
                    yield return new SuggestCheckService(code, this.TargetFile);
                }
            }
            else if (this.TargetFile.IndexOf("HIS2") > 0) // AS-IS
            {                
                var code = IOHelper.ReadFileToString(this.TargetFile);
                var designerFilePath = this.TargetFile.Replace(".cs", ".Designer.cs"); ;
                if (System.IO.File.Exists(designerFilePath))
                {
                    code += IOHelper.ReadFileToString(designerFilePath);
                }
                                
                yield return new SuggestViewSetting(code);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        #endregion
                
        #region suggest item classes...

        public interface ISuggestResult { }

        #region AS-IS

        public class SuggestViewSetting : ISuggestResult
        {
            public string ResultCode { get; set; }

            public SuggestViewSetting(string code)
            {
                string title, width, height;
                title = width = height = string.Empty;

                var m = Regex.Match(code, @"this\.Text = ""(.*)"";");
                if (m.Success)
                {
                    title = m.Groups[1].Value;
                }

                m = Regex.Match(code, @"this\.ClientSize = new System\.Drawing\.Size\((\d+), (\d+)\);");
                if (m.Success)
                {
                    width = Convert.ToString(Convert.ToDouble(m.Groups[1].Value) + 16);
                    height = Convert.ToString(Convert.ToDouble(m.Groups[2].Value) + 39);
                }

                this.ResultCode = string.Format(@"        #region ISAFViewSettingDefault - DefaultViewSetting

    private SAFViewSettings _DefaultViewSetting;
    public SAFViewSettings DefaultViewSetting
    {{
        get 
        {{
            if (_DefaultViewSetting == null)
            {{
                _DefaultViewSetting = new SAFViewSettings()
                {{
                    Title = ""{0}"",
                    Width = {1},
                    Height = {2},
                }};
            }}
            return new SAFViewSettings(); 
        }}
    }}

    #endregion", title, width, height);
            }
        }

        #endregion

        #region TO-BE ViewModel

        public class SuggestLocalization : PropertyNotifier, ISuggestResult
        {
            private string code;
            private string filePath;

            public SuggestLocalization(string code, string filePath)
            {
                this.code = code;
                this.filePath = filePath;

                var resourceFile = GetResourceFilePath();
                var resource = ResourceHelper.ReadResourceFile(resourceFile);

                this.BaseKey = string.Join("_", filePath.RegexReturn(@"Client\\(\w+)\\", 1), filePath.RegexReturn(@"(\w+)ViewModel", 1), "Msg");

                var items = resource.Where(p => Convert.ToString(p.Key).IndexOf(this.BaseKey) >= 0);
                this.StartIndex = items.Count() < 1
                    ? 1
                    : items.Select(p => Convert.ToInt32(Convert.ToString(p.Key).RightBySearch("_"))).OrderBy(p => p).Last() + 1;

                Run();
            }

            private void Run()
            {
                this.LineItems = GetLineItems(this.code).ToArray();
            }

            private IEnumerable<LineItem> GetLineItems(string code)
            {
                var lineNumbers = Regex.Matches(code, @"\r\n").Cast<Match>().Select(p => p.Index);

                string content, resourceKey, resourceString, replaceTarget, replacement;
                int keyIndex = this.StartIndex;

                var pattern = @".*(?:ShowMessagePopup|ShowConfirmPopup)[^;]+";
                var matches = Regex.Matches(code, pattern);
                foreach (Match match in matches)
                {
                    content = match.Value;
                    resourceKey = resourceString = replaceTarget = replacement = string.Empty;

                    int start = int.MaxValue;
                    int end = int.MinValue;

                    foreach (Match m in Regex.Matches(content, @"""(.*?)"""))
                    {
                        if (Regex.IsMatch(m.Value, @"[가-힣]") != true) continue;

                        resourceString += m.Groups[1].Value;

                        start = Math.Min(start, match.Index + m.Index);
                        end = Math.Max(end, match.Index + m.Index + m.Length);
                    }

                    if (string.IsNullOrEmpty(resourceString) != true)
                    {
                        resourceKey = string.Join("_", this.BaseKey, keyIndex++);

                        if (match.Value.IndexOf("GetLocalizedText") < 0)
                        {
                            replaceTarget = code.Substring(start, end - start);
                            replacement = string.Format(@"GetLocalizedText(""{0}MsgResourceLocalize"", ""{1}"")",
                                filePath.RegexReturn(@"src\\(\w+)", 1), resourceKey);
                        }
                        else
                        {
                            replaceTarget = resourceString;
                            replacement = resourceKey;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    yield return new LineItem()
                    {
                        Check = true,
                        Line = lineNumbers.ToList().IndexOf(lineNumbers.Where(n => n > match.Index).First()) + 1,
                        Content = content,
                        ResourceKey = resourceKey,
                        ResourceString = resourceString,
                        ReplaceTarget = replaceTarget,
                        Replacement = replacement,
                        RegexMatch = match,
                    };
                }
            }

            public string BaseKey { get { return GetPropertyValue(); } set { SetPropertyValue(value); if (this.LineItems != null) this.Run(); } }
            public int StartIndex { get { return GetPropertyValue(); } set { SetPropertyValue(value); if (this.LineItems != null) this.Run(); } }
            public IEnumerable<LineItem> LineItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

            public ICommand UpdateFileCommand { get; set; }

            public override void InitCommands()
            {
                base.InitCommands();

                UpdateFileCommand = new SimpleCommand(OnExecuteUpdateFileCommand);
            }

            private void OnExecuteUpdateFileCommand(object obj)
            {
                var resourceFilePath = System.IO.Path.GetDirectoryName(GetResourceFilePath());

                if (SvnHelper.Update(resourceFilePath) != true)
                {
                    MessageBox.Show("SVN 업데이트에 실패했습니다.");
                    return;
                }

                if (MessageBox.Show("Resource File, Source File의 소스코드에 반영 할까요?", "확인", MessageBoxButton.YesNo) != MessageBoxResult.Yes) 
                    return;

                UpdateResourceFile();
                UpdateSourceFile();

                MessageBox.Show("Resource File, Source File의 소스코드에 반영 되었습니다.");

                if (SvnHelper.Commit(resourceFilePath, "Localization Auto Update...") != true)
                    MessageBox.Show("SVN 커밋에 실패했습니다. 확인이 필요합니다.");

                Run();
            }

            private void UpdateResourceFile()
            {
                var resourceFile = GetResourceFilePath();

                var dic = new Dictionary<string, string>();
                foreach (var item in this.LineItems)
                {
                    if (item.Check != true
                        || string.IsNullOrEmpty(item.ResourceKey)
                        || string.IsNullOrEmpty(item.ResourceString)) continue;

                    dic.Add(item.ResourceKey, item.ResourceString);
                }

                ResourceHelper.UpdateResourceFile(dic, resourceFile);
            }

            private void UpdateSourceFile()
            {
                foreach (var item in this.LineItems)
                {
                    if (item.Check != true
                        || string.IsNullOrEmpty(item.ResourceKey)
                        || string.IsNullOrEmpty(item.ResourceString)) continue;

                    var codeOfRange = code.Substring(item.RegexMatch.Index);
                    var start = item.RegexMatch.Index + codeOfRange.IndexOf(item.ReplaceTarget);

                    code = code.Remove(start, item.ReplaceTarget.Length);
                    code = code.Insert(start, item.Replacement);
                }

                IOHelper.SaveFile(filePath, code);
            }

            private string GetResourceFilePath()
            {
                var div = this.filePath.RegexReturn(@"src\\(\w+)", 1);
                var resourceFile = string.Format(@"{0}src\Common\His3.Common.Resources\{1}\{1}MsgResourceLocalize.resx",
                    this.filePath.LeftBySearch(@"src\"), div);

                return resourceFile;
            }

            public class LineItem : PropertyNotifier
            {
                public int Line { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public string Content { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public bool Check { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public string ResourceKey { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public string ResourceString { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public string ReplaceTarget { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public string Replacement { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

                public Match RegexMatch;
            }
        }

        public class SuggestTodoList : ISuggestResult
        {
            public SuggestTodoList(string code)
            {
                var lineNumbers = Regex.Matches(code, @"\r\n").Cast<Match>().Select(p => p.Index);

                var pattern = @"(?://|/\*).*TODO.*[^\r\n]";
                var matches = Regex.Matches(code, pattern);

                TodoLines = matches.Cast<Match>().Select(p => new TodoItem()
                {
                    Line = lineNumbers.ToList().IndexOf(lineNumbers.Where(n => n > p.Index).First()) + 1,
                    Content = p.Value,
                });
            }

            public IEnumerable<TodoItem> TodoLines { get; set; }

            public class TodoItem
            {
                public int Line { get; set; }

                public string Content { get; set; }
            }
        }

        public class SuggestCodeHint : ISuggestResult
        {
            private string code;
            private string viewCode;

            public SuggestCodeHint(string code, string vmPath)
            {
                this.code = code;
                var viewPath = vmPath.Replace("ViewModel.cs", "View.xaml");
                this.viewCode = IOHelper.ReadFileToString(viewPath);

                this.HintList = new List<HintItem>();

                CollectControlSetters(); 
            }

            private void CollectControlSetters()
            {
                foreach (Match match in Regex.Matches(this.code, @"ControlSetters(?:Property)*\[""(\w+)""\]"))
                {
                    var name = match.Groups[1].Value;
                    if (this.viewCode.IndexOf(string.Format(@"ControlSetters[{0}]", name)) > 0) continue;

                    HintList.Add(new HintItem()
                        {
                            Code = match.Value,
                            Message = string.Format(@"View에 ControlSetters의 요소 '{0}'이 정의되지 않았습니다.", name),
                            Suggestion = string.Format(@"<i:Interaction.Behaviors><behavior:ControlCommonBehavior ControlSetter=""{{Binding Path=ControlSetters[{0}]}}""/></i:Interaction.Behaviors>", name),
                        });
                }

                foreach (Match match in Regex.Matches(this.code, @"ColumnSetters(?:Property)*\[""(\w+)""\]"))
                {
                    var name = match.Groups[1].Value;
                    if (this.viewCode.IndexOf(string.Format(@"Binding {0}", name)) > 0) continue;

                    HintList.Add(new HintItem()
                    {
                        Code = match.Value,
                        Message = string.Format(@"View에 ColumnSetters의 요소 '{0}'이 정의되지 않았습니다.", name),
                        Suggestion = string.Format(@"Binding {0}", name),
                    });
                }
            }

            public IList<HintItem> HintList { get; set; }

            public class HintItem
            {
                public string Message { get; set; }
                public string Code { get; set; }             
                public string Suggestion { get; set; }
            }
        }

        public class SuggestHintFromAsis : PropertyNotifier, ISuggestResult
        {
            private string code;
            private string vmPath;

            public SuggestHintFromAsis(string code, string vmPath)
            {
                this.code = code;
                this.vmPath = vmPath;

                this.HintList = new ObservableCollection<HintItem>();
            }

            public void Run()
            {
                this.HintList.Clear();

                var asisCode = IOHelper.ReadFileToString(AsisFilePath);
                var initCode = asisCode.Between("InitializeComponent()", "this.ResumeLayout(false);");

                foreach (Match match in Regex.Matches(this.code, @"private (\w+) (_(\w+)Property)(.*);"))
                {
                    var line = match.Groups[0].Value;
                    var typeName = match.Groups[1].Value;
                    var propertyName = match.Groups[2].Value;
                    var nameKey = match.Groups[3].Value;
                    var setLine = match.Groups[4].Value;
                    
                    if (typeName == "Visibility")
                    {
                        var name = nameKey.Replace("Visible", "");
                        var m = Regex.Match(initCode, string.Format(@"{0}\.Visible = (\w+)", name));
                        if (m.Success)
                        {
                            var setValue = Convert.ToBoolean(m.Groups[1].Value) ? "Visibility.Visible" : "Visibility.Collapsed";
                            var suggestion = string.Format(@"private {0} {1} = {2};", typeName, propertyName, setValue);

                            if (setLine.EndsWith(setValue)) continue;

                            this.HintList.Add(new HintItem()
                                {
                                    Apply = true,
                                    Code = line,
                                    Suggestion = suggestion,
                                    Message = string.Format(@"Asis에서 초기화코드 {0}; 를 발견 했습니다.", m.Value),
                                });
                        }
                        else if (setLine.Length < 1)
                        {
                            var setValue = "Visibility.Visible";
                            var suggestion = string.Format(@"private {0} {1} = {2};", typeName, propertyName, setValue);

                            this.HintList.Add(new HintItem()
                            {
                                Apply = false,
                                Code = line,
                                Suggestion = suggestion,
                                Message = string.Format(@"Asis에서 초기화코드를 찾을 수 없습니다.", m.Value),
                            });
                        }
                    }
                    else if (typeName == "bool" && nameKey.EndsWith("IsChecked"))
                    {
                        var name = nameKey.Replace("IsChecked", "");
                        var m = Regex.Match(initCode, string.Format(@"{0}\.Checked = (\w+)", name), RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            var setValue = Convert.ToBoolean(m.Groups[1].Value) ? "true" : "false";
                            var suggestion = string.Format(@"private {0} {1} = {2};", typeName, propertyName, setValue);

                            if (setLine.EndsWith(setValue)) continue;

                            this.HintList.Add(new HintItem()
                            {
                                Apply = true,
                                Code = line,
                                Suggestion = suggestion,
                                Message = string.Format(@"Asis에서 초기화코드 {0}; 를 발견 했습니다.", m.Value),
                            });
                        }
                        else if (setLine.Length < 1)
                        {
                            var setValue = "false";
                            var suggestion = string.Format(@"private {0} {1} = {2};", typeName, propertyName, setValue);

                            this.HintList.Add(new HintItem()
                            {
                                Apply = false,
                                Code = line,
                                Suggestion = suggestion,
                                Message = string.Format(@"Asis에서 초기화코드를 찾을 수 없습니다.", m.Value),
                            });
                        }
                    }
                    else if (typeName == "bool" && nameKey.EndsWith("Enabled"))
                    {
                        var name = nameKey.Replace("Enabled", "");
                        var m = Regex.Match(initCode, string.Format(@"{0}\.Enabled = (\w+)", name), RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            var setValue = Convert.ToBoolean(m.Groups[1].Value) ? "true" : "false";
                            var suggestion = string.Format(@"private {0} {1} = {2};", typeName, propertyName, setValue);

                            if (setLine.EndsWith(setValue)) continue;

                            this.HintList.Add(new HintItem()
                            {
                                Apply = true,
                                Code = line,
                                Suggestion = suggestion,
                                Message = string.Format(@"Asis에서 초기화코드 {0}; 를 발견 했습니다.", m.Value),
                            });
                        }
                        else if (setLine.Length < 1)
                        {
                            var setValue = "true";
                            var suggestion = string.Format(@"private {0} {1} = {2};", typeName, propertyName, setValue);

                            this.HintList.Add(new HintItem()
                            {
                                Apply = false,
                                Code = line,
                                Suggestion = suggestion,
                                Message = string.Format(@"Asis에서 초기화코드를 찾을 수 없습니다.", m.Value),
                            });
                        }
                    }
                    else if (typeName == "string" && nameKey.EndsWith("Text"))
                    {
                        var name = nameKey.Replace("Text", "");
                        var m = Regex.Match(initCode, string.Format(@"{0}\.Text = (.*);", name), RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            var setValue = m.Groups[1].Value;
                            var suggestion = string.Format(@"private {0} {1} = {2};", typeName, propertyName, setValue);

                            if (setLine.EndsWith(setValue)) continue;

                            this.HintList.Add(new HintItem()
                            {
                                Apply = true,
                                Code = line,
                                Suggestion = suggestion,
                                Message = string.Format(@"Asis에서 초기화코드 {0}; 를 발견 했습니다.", m.Value),
                            });
                        }
                        else if (setLine.Length < 1)
                        {
                            var setValue = "string.Empty";
                            var suggestion = string.Format(@"private {0} {1} = {2};", typeName, propertyName, setValue);

                            this.HintList.Add(new HintItem()
                            {
                                Apply = false,
                                Code = line,
                                Suggestion = suggestion,
                                Message = string.Format(@"Asis에서 초기화코드를 찾을 수 없습니다.", m.Value),
                            });
                        }
                    }
                }
            }

            public void Update()
            {
                bool apply = false;

                foreach (var item in this.HintList)
                {
                    if (item.Apply != true
                        || string.IsNullOrEmpty(item.Code)
                        || string.IsNullOrEmpty(item.Suggestion)) continue;

                    this.code = this.code.Replace(item.Code, item.Suggestion);
                    apply = true;
                }

                if (apply)
                {
                    IOHelper.SaveFile(this.vmPath, this.code);
                    Run();
                } 
            }

            public string AsisFilePath { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public ObservableCollection<HintItem> HintList { get; set; }

            public class HintItem : PropertyNotifier
            {
                public bool Apply { get; set; }
                public string Message { get; set; }                
                public string Code { get; set; }
                public string Suggestion { get; set; }                
            }
        }

        public class SuggestCheckService : ISuggestResult
        {
            private string viewModelPath;
            public SuggestCheckService(string code, string vmPath)
            {
                this.viewModelPath = vmPath;
                
                Results = GetResults();
            }

            private IEnumerable<Result> GetResults()
            {
                var types = SuggestHelper.AllTypes;
                var anyMethods = SuggestHelper.AnyMethods;

                //"D:\\720.YUHS\\Useverance3\\src\\Sp\\Client\\Pha\\Am\\Rcpt\\AmbOrdRcptPrtMFViewModel.cs"
                var match = Regex.Match(this.viewModelPath, @"Client\\(\w+)\\(\w+)");
                var filter = string.Format(@".Model.{0}.{1}", match.Groups[1].Value, match.Groups[2].Value);

                var modelRequestTypes = types.Where(p => p.FullName.IndexOf(filter) > 0 && p.Name.EndsWith("Request"));
                var serviceRequestTypes = anyMethods.Select(p => p.GetParameters().First().ParameterType);

                foreach (var modelRequestType in modelRequestTypes)
                {
                    if (serviceRequestTypes.Contains(modelRequestType) != true)
                    {
                        yield return new Result()
                        {
                            RequestName = modelRequestType.FullName,
                            Message = string.Format(@"'{0}'를 처리하는 Any메서드가 존재하지 않습니다.", modelRequestType.Name),
                        };
                    }
                }
            }

            public IEnumerable<Result> Results { get; set; }

            public class Result
            {
                public string RequestName { get; set; }
                public string Message { get; set; }
            }
        }
                
        #endregion

        #region TO-BE View

        public class SuggestXamlLocalization : PropertyNotifier, ISuggestResult
        {
            private string code;
            private string filePath;

            public SuggestXamlLocalization(string code, string filePath)
            {
                this.code = code;
                this.filePath = filePath;

                var resourceFile = GetResourceFilePath();
                var resource = ResourceHelper.ReadResourceFile(resourceFile);

                this.BaseKey = string.Join("_", filePath.RegexReturn(@"Client\\(\w+)\\", 1), filePath.RegexReturn(@"(\w+)View", 1), "Text");

                var items = resource.Where(p => Convert.ToString(p.Key).IndexOf(this.BaseKey) >= 0);
                this.StartIndex = items.Count() < 1
                    ? 1
                    : items.Select(p => Convert.ToInt32(Convert.ToString(p.Key).RightBySearch("_"))).OrderBy(p => p).Last() + 1;
                
                Run();
            }

            private void Run()
            {
                this.LineItems = GetLineItems(this.code).ToArray();
            }

            private IEnumerable<LineItem> GetLineItems(string code)
            {
                var lineNumbers = Regex.Matches(code, @"\r\n").Cast<Match>().Select(p => p.Index);

                string content, resourceKey, resourceString, replaceTarget, replacement;
                int keyIndex = this.StartIndex;

                var pattern = @"(?:Header|Content|Text)=""(.*?)""";
                var matches = Regex.Matches(code, pattern);
                foreach (Match match in matches)
                {
                    content = match.Value;
                    resourceKey = resourceString = replaceTarget = replacement = string.Empty;

                    int start = int.MaxValue;
                    int end = int.MinValue;

                    foreach (Match m in Regex.Matches(content, @"""(.*?)"""))
                    {
                        if (m.Value.IndexOf("{") >= 0
                            || m.Groups[1].Value == "∨"
                            ) continue;

                        resourceString += m.Groups[1].Value;

                        start = Math.Min(start, match.Index + m.Index);
                        end = Math.Max(end, match.Index + m.Index + m.Length);
                    }

                    if (string.IsNullOrEmpty(resourceString) != true)
                    {
                        resourceKey = string.Join("_", this.BaseKey, keyIndex++);

                        replaceTarget = resourceString;
                        replacement = string.Format(@"{{lex:Loc Key={0}}}", resourceKey);
                    }
                    else
                    {
                        continue;
                    }

                    yield return new LineItem()
                    {
                        Check = Regex.IsMatch(resourceString, @"[가-힣]"),
                        Line = lineNumbers.ToList().IndexOf(lineNumbers.Where(n => n > match.Index).First()) + 1,
                        Content = content,
                        ResourceKey = resourceKey,
                        ResourceString = resourceString,
                        ReplaceTarget = replaceTarget,
                        Replacement = replacement,
                        RegexMatch = match,
                    };
                }
            }

            public string BaseKey { get { return GetPropertyValue(); } set { SetPropertyValue(value); if (this.LineItems != null) this.Run(); } }
            public int StartIndex { get { return GetPropertyValue(); } set { SetPropertyValue(value); if (this.LineItems != null) this.Run(); } }
            public IEnumerable<LineItem> LineItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public ICommand UpdateFileCommand { get; set; }

            public override void InitCommands()
            {
                base.InitCommands();

                UpdateFileCommand = new SimpleCommand(OnExecuteUpdateFileCommand);
            }

            private void OnExecuteUpdateFileCommand(object obj)
            {
                var resourceFilePath = System.IO.Path.GetDirectoryName(GetResourceFilePath());

                if (SvnHelper.Update(resourceFilePath) != true)
                {
                    MessageBox.Show("SVN 업데이트에 실패했습니다.");
                    return;
                }

                if (MessageBox.Show("Resource File, Source File의 소스코드에 반영 할까요?", "확인", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;

                UpdateResourceFile();
                UpdateSourceFile();

                MessageBox.Show("Resource File, Source File의 소스코드에 반영 되었습니다.");

                if (SvnHelper.Commit(resourceFilePath, "Localization Auto Update...") != true)
                    MessageBox.Show("SVN 커밋에 실패했습니다. 확인이 필요합니다.");

                Run();
            }

            private void UpdateResourceFile()
            {
                var resourceFile = GetResourceFilePath();

                var dic = new Dictionary<string, string>();
                foreach (var item in this.LineItems)
                {
                    if (item.Check != true
                        || string.IsNullOrEmpty(item.ResourceKey)
                        || string.IsNullOrEmpty(item.ResourceString)) continue;

                    dic.Add(item.ResourceKey, item.ResourceString);
                }

                ResourceHelper.UpdateResourceFile(dic, resourceFile);
            }

            private void UpdateSourceFile()
            {
                foreach (var item in this.LineItems)
                {
                    if (item.Check != true
                        || string.IsNullOrEmpty(item.ResourceKey)
                        || string.IsNullOrEmpty(item.ResourceString)) continue;

                    var codeOfRange = code.Substring(item.RegexMatch.Index);
                    var start = item.RegexMatch.Index + codeOfRange.IndexOf(item.ReplaceTarget);

                    code = code.Remove(start, item.ReplaceTarget.Length);
                    code = code.Insert(start, item.Replacement);
                }

                IOHelper.SaveFile(filePath, code);
            }

            private string GetResourceFilePath()
            {
                var div = this.filePath.RegexReturn(@"src\\(\w+)", 1);
                var resourceFile = string.Format(@"{0}src\Common\His3.Common.Resources\{1}\{1}ResourceLocalize.resx",
                    this.filePath.LeftBySearch(@"src\"), div);

                return resourceFile;
            }

            public class LineItem : PropertyNotifier
            {
                public int Line { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public string Content { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public bool Check { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public string ResourceKey { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public string ResourceString { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public string ReplaceTarget { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
                public string Replacement { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

                public Match RegexMatch;
            }
        }

        public class SuggestBinding : PropertyNotifier, ISuggestResult
        {
            private string code;
            private string viewPath;
            private IEnumerable<Type> allTypes;
            private Type viewModelType;
            private IEnumerable<string> properties;            

            public SuggestBinding(string code, string viewPath)
            {
                this.code = code;
                this.viewPath = viewPath;

                var xdoc = new System.Xml.XmlDocument();
                xdoc.Load(viewPath);

                BindingItems = GetBindingItems(xdoc.ChildNodes.Cast<XmlNode>()).ToArray();

                this.allTypes = SuggestHelper.AllTypes;
                var viewName = code.RegexReturn(@"x:Class=""([\w\.]+)""", 1);
                var viewModelName = viewName + "Model";
                this.viewModelType = allTypes.Where(p => p.FullName == viewModelName).FirstOrDefault();

                if (viewModelType == null)
                    throw new NotSupportedException("뷰모델을 찾을 수 없습니다.");

                this.properties = GetProperties();

                foreach (var item in BindingItems)
                {
                    CheckBinding(item);
                }

                CheckBindingFromViewModel();
            }

            private void CheckBindingFromViewModel()
            {
                var bindingList = BindingItems.ToList();

                // 필수 프로퍼티점검
                CheckEssentialProperty(bindingList, properties, "LoadedCommand");
                CheckEssentialProperty(bindingList, properties, "CloseCommand");

                foreach (var property in properties)
                {
                    if (property.EndsWith("VM")
                        || property.StartsWith("Req")
                        || property.StartsWith("Rtn")
                        || property.IndexOf("DefaultViewSetting") >= 0
                        ) continue;

                    var pInfo = BindingItems.Where(p => p.BindingPath.RegexReturn(@"[\w\.]+") == property).FirstOrDefault();
                    if (pInfo != null) continue;

                    bindingList.Add(new BindingItem()
                        {
                            BindingSource = viewModelType.Name,
                            BindingPath = property,
                            State = CheckState.Failed,
                            Message = string.Format("뷰모델의 프로퍼티 {0}가 View에 바인딩 되지 않았습니다.", property),
                        });
                }

                this.BindingItems = bindingList;
            }

            private IEnumerable<string> GetProperties()
            {
                var properties = viewModelType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).Select(p => p.Name).ToList();
                foreach(var propertyName in WorkService.Current.GetPropertyNames(this.code))
                {
                    if (properties.Contains(propertyName)) continue;
                    properties.Add(propertyName);
                }

                return properties;
            }

            private void CheckEssentialProperty(List<BindingItem> bindingList, IEnumerable<string> properties, string propertyName)
            {
                var checkProperty = properties.Where(p => p.Equals(propertyName)).FirstOrDefault();
                if (checkProperty == null)
                {
                    bindingList.Add(new BindingItem()
                    {
                        BindingSource = viewModelType.Name,
                        BindingPath = propertyName,
                        State = CheckState.Failed,
                        Message = string.Format("{0}가 ViewModel에 정의되지 않았습니다.(확인)", propertyName),
                    });
                }
            }
            
            private void CheckBinding(BindingItem item)
            {
                if (string.IsNullOrEmpty(item.BindingPath)) return;

                var bindingPath = item.BindingPath.RegexReturn(@"[\w\.]+");

                if (string.IsNullOrEmpty(item.BindingSource))
                {
                    var contain = properties.Contains(bindingPath);
                    item.State = contain ? CheckState.Success : CheckState.Failed;
                    if (item.State == CheckState.Failed)
                        item.Message = string.Format(@"'{0}'에 {1} 프로퍼티가 없습니다.", "ViewModel", item.BindingPath);
                }
                else
                {
                    var pInfo = viewModelType.GetProperty(item.BindingSource);
                    if (pInfo == null)
                    {
                        item.State = CheckState.Failed;
                        item.Message = string.Format(@"'{0}'에 {1} 프로퍼티가 없습니다.", "ViewModel", item.BindingPath);
                        return;
                    }

                    var sourceType = pInfo.PropertyType.IsGenericType
                        ? pInfo.PropertyType.GetGenericArguments()[0]
                        : pInfo.PropertyType;

                    pInfo = sourceType.GetProperty(bindingPath);
                    item.State = pInfo == null ? CheckState.Failed : CheckState.Success;

                    if (item.State == CheckState.Failed)
                    {
                        var contain = properties.Contains(bindingPath);
                        item.State = contain ? CheckState.Success : CheckState.Failed;
                    }

                    if (item.State == CheckState.Failed)
                        item.Message = string.Format(@"'{0}'에 {1} 프로퍼티가 없습니다.", sourceType.Name, item.BindingPath);
                }

            }

            private IEnumerable<BindingItem> GetBindingItems(IEnumerable<XmlNode> xmlNodes, string bindingSource = null)
            {
                foreach (var node in xmlNodes)
                {   
                    if (node.Attributes != null)
                    {   
                        foreach (XmlAttribute attribute in node.Attributes.Cast<XmlAttribute>())
                        {
                            if (attribute.Value.IndexOf("Binding") > 0)
                            {
                                if (attribute.Value.IndexOf("ElementName") > 0) continue;

                                yield return new BindingItem()
                                {
                                    ElementType = node.Name,
                                    AttributeName = attribute.Name,
                                    AttributeValue = attribute.Value,
                                    BindingPath = GetBindingPath(attribute.Value),
                                    BindingSource = GetBindingSource(attribute.Value, bindingSource),
                                    Node = node,
                                };
                            }
                        }

                        if ((node.Name.IndexOf("ComboBox") > 0 && node.Name.IndexOf("ComboBox.") < 0)
                            || (node.Name.IndexOf("ListBox") > 0 && node.Name.IndexOf("ListBox.") < 0))
                        {
                            #region ItemsControl...

                            var attributeItemsSource = node.Attributes["ItemsSource"];
                            var attributeSelectedItem = node.Attributes["SelectedItem"];

                            if (attributeItemsSource == null)
                            {
                                yield return new BindingItem()
                                {
                                    ElementType = node.Name,
                                    AttributeName = "ItemsSource",
                                    State = CheckState.Failed,
                                    Message = "ItemsSource 바인딩이 필요합니다.",
                                };
                            }
                            if (attributeSelectedItem == null)
                            {
                                yield return new BindingItem()
                                {
                                    ElementType = node.Name,
                                    AttributeName = "SelectedItem",
                                    State = CheckState.Failed,
                                    Message = "SelectedItem 바인딩이 필요합니다.",
                                };
                            }

                            var attribute = node.Attributes["DisplayMemberPath"];
                            if (attribute == null)
                            {
                                yield return new BindingItem()
                                {
                                    ElementType = node.Name,
                                    AttributeName = "DisplayMemberPath",
                                    State = CheckState.Failed,
                                    Message = "DisplayMemberPath 설정이 필요합니다.",
                                };
                            }
                            else
                            {
                                yield return new BindingItem()
                                {
                                    ElementType = node.Name,
                                    AttributeName = "DisplayMemberPath",
                                    AttributeValue = attribute == null ? string.Empty : attribute.Value,
                                    BindingPath = attribute == null ? string.Empty : attribute.Value,
                                    BindingSource = attributeSelectedItem == null ? string.Empty : GetBindingPath(attributeSelectedItem.Value),
                                    Node = node,
                                };
                            }
                            attribute = node.Attributes["SelectedValuePath"];
                            if (attribute == null)
                            {
                                //! 필수요소가 아님
                                //yield return new BindingItem()
                                //{
                                //    ElementType = node.Name,
                                //    AttributeName = "SelectedValuePath",
                                //    State = CheckState.Failed,
                                //    Message = "SelectedValuePath 설정이 필요합니다.",
                                //};
                            }
                            else
                            {
                                yield return new BindingItem()
                                {
                                    ElementType = node.Name,
                                    AttributeName = "SelectedValuePath",
                                    AttributeValue = attribute == null ? string.Empty : attribute.Value,
                                    BindingPath = attribute == null ? string.Empty : attribute.Value,
                                    BindingSource = attributeSelectedItem == null ? string.Empty : GetBindingPath(attributeSelectedItem.Value),
                                    Node = node,
                                };
                            }

                            #endregion
                        }
                        else if (node.Name.EndsWith("Button") && node.Name.IndexOf("Button.") < 0)
                        {
                            #region Button

                            var attributeCommand = node.Attributes["Command"];

                            if (attributeCommand == null)
                            {
                                yield return new BindingItem()
                                {
                                    ElementType = node.Name,
                                    AttributeName = "Command",
                                    State = CheckState.Failed,
                                    Message = "Command 바인딩이 필요합니다.",
                                };
                            }

                            #endregion
                        }
                    }

                    if (node.HasChildNodes)
                    {
                        string dataContext = GetDataContext(node, bindingSource);
                        foreach (var item in GetBindingItems(node.ChildNodes.Cast<XmlNode>(), dataContext))
                        {
                            yield return item;
                        }
                    }
                }
            }

            private string GetDataContext(XmlNode node, string bindingSource = null)
            {
                if (node.Attributes == null) return bindingSource;

                var attribute = node.Attributes["ItemsSource"];
                if (attribute != null) return GetBindingPath(attribute.Value);

                attribute = node.Attributes["DataContext"];
                if (attribute != null) return GetBindingPath(attribute.Value);

                return bindingSource;
            }

            private string GetBindingPath(string value)
            {
                var path = value.RegexReturn(@"Path=(?:DataContext\.)*([\w\.\[\]]+)", 1);
                if (string.IsNullOrEmpty(path) != true) return path;

                path = value.RegexReturn(@"Command=(?:DataContext\.)*([\w\.\[\]]+)", 1);
                if (string.IsNullOrEmpty(path) != true) return path;

                path = value.RegexReturn(@"Binding (?:DataContext\.)*([\w\.\[\]]+)", 1);
                if (string.IsNullOrEmpty(path) != true) return path;

                return string.Empty;
            }

            private string GetBindingSource(string value, string defaultSource)
            {
                return defaultSource;
            }
            
            public IEnumerable<BindingItem> BindingItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

            public enum CheckState
            {
                UnChecked,
                Success,
                Failed,
            }

            public class BindingItem
            {
                public XmlNode Node { get; set; }
                public string ElementType { get; set; }
                public string AttributeName { get; set; }
                public string AttributeValue { get; set; }
                public string BindingPath { get; set; }
                public string BindingSource { get; set; }                
                public CheckState State { get; set; }
                public string Message { get; set; }
            }
        }

        public class SuggestXamlHint : PropertyNotifier, ISuggestResult
        {
            private string code;
            private string viewPath;

            public SuggestXamlHint(string code, string viewPath)
            {
                this.code = code;
                this.viewPath = viewPath;

                var xdoc = new System.Xml.XmlDocument();
                xdoc.Load(viewPath);

                HintList = GetHintList(xdoc);
            }

            private IEnumerable<HintItem> GetHintList(XmlDocument xdoc)
            {
                var nodes = xdoc.GetElementsByTagName("saf:SafTelerikGridViewCheckBoxColumn");
                foreach (XmlNode node in nodes)
                {
                    if (node.OuterXml.IndexOf("IsSelected") > 0)
                    {
                        yield return new HintItem()
                        {
                            Message = "CheckAllColumn 변경 확인!",
                            Code = node.OuterXml,
                            Suggestion = @"<hisSaf:CheckAllColumn Header=""∨"" Width=""35"" />",
                        };
                    }
                }
            }

            public IEnumerable<HintItem> HintList { get; set; }

            public class HintItem
            {
                public string Message { get; set; }
                public string Code { get; set; }
                public string Suggestion { get; set; }
            }
        }

        #endregion

        #endregion
    }

    public static class SuggestHelper
    {
        #region Fields...

        private static string binaryPath;
        private static IEnumerable<Type> _AllTypes;
        private static IEnumerable<MethodInfo> _AnyMethods;

        #endregion

        #region Properties...

        public static IEnumerable<Type> AllTypes 
        {
            get
            {
                if (_AllTypes == null)
                    _AllTypes = GetAllTypes().ToArray();

                return _AllTypes;
            }
        }
        
        public static IEnumerable<MethodInfo> AnyMethods 
        {
            get
            {
                if (_AnyMethods == null)
                    _AnyMethods = GetAnyMethods().ToArray();

                return _AnyMethods;
            }
        }

        #endregion

        #region Methods...

        private static IEnumerable<Type> GetAllTypes()
        {
            return GetAllTypes(IOHelper.GetFiles(binaryPath, "His3*.dll"));
        }

        private static IEnumerable<Type> GetAllTypes(string[] files)
        {
            foreach (var path in files)
            {
                var types = ReflectionHelper.GetTypesByAsmPath(path);
                if (types == null) continue;

                foreach (var type in types)
                    yield return type;
            }
        }

        private static IEnumerable<MethodInfo> GetAnyMethods()
        {
            foreach(var type in AllTypes)
            {
                foreach (var method in type.GetMethods().Where(p => p.Name.Equals("Any")))
                {
                    if (method.GetParameters().Count() < 1) continue;

                    yield return method;
                }
            }
        }

        internal static void ReadyTobe(string filePath)
        {
            _AllTypes = null;
            _AnyMethods = null;

            var buildPath = System.IO.Path.Combine(filePath.LeftBySearch(@"\src"), @"src\Common\artifacts\Debug\*.dll");
            var toPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "binary");
            IOHelper.Xcopy(buildPath, toPath);

            binaryPath = toPath;
        }

        #endregion
    }
}
