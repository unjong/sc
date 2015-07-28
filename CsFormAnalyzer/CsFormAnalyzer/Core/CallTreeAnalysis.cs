using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Core
{
    public partial class CallTreeAnalysis
    {
        #region Fields

        private string _filePath;
        private string _namespace;
        private string _className;
        private string _baseClassName;
        private string _statics;
        private Regex _RegexFuncParam = new Regex(@"[\w]+\(([^()]*|\(([^()]*|\([^()]*\))*\))*\)", RegexOptions.Compiled);

        #endregion

        #region Initialize...

        public CallTreeAnalysis(string filePath, string statics)
        {
            this._filePath = filePath;
            this._statics = statics;

            var code = IOHelper.ReadFileToString(filePath);

            ParseClassInfo(code);

            // Form, Controller을 대상으로 합니다.
            if (string.IsNullOrEmpty(this._namespace)
                || string.IsNullOrEmpty(this._className)
                || string.IsNullOrEmpty(this._baseClassName)                
                || (this._baseClassName.IndexOf("Form", StringComparison.CurrentCultureIgnoreCase) >= 0 
                || this._baseClassName.IndexOf("Controller", StringComparison.CurrentCultureIgnoreCase) >= 0) != true
                )
            {
                return;
            }

            StartAnalysis(code);
        }

        #endregion

        #region Properties

        public List<CallTreeItem> CallTreeList { get; set; }

        #endregion

        #region Analysis

        private void StartAnalysis(string code)
        {
            var classInfoList = new List<ClassInfoItem>();
            AnalysisClassInfo(code, classInfoList);
            var list = AnalysisCallInfo(code, classInfoList);
            this.CallTreeList = list;
        }

        private void ParseClassInfo(string code)
        {
            this._namespace = code.Between("namespace ", "\r").Trim();
            if (string.IsNullOrEmpty(this._namespace)) return;

            var classLine = code.Between(@" class ", "\r").Trim();
            if (string.IsNullOrEmpty(classLine)) return;

            if (classLine.IndexOf(":") < 0)
            {
                this._className = classLine.RegexReturn(@"[\w]+");
                this._baseClassName = "Form";
            }
            else
            {
                this._className = classLine.Between("", ":").Trim().RegexReturn(@"[\w]+");
                this._baseClassName = classLine.RightBySearch(":").Trim();
                if (this._baseClassName.IndexOf('.') > 0)
                    this._baseClassName = classLine.RightBySearch(".").Trim();
                if (this._baseClassName.IndexOf(' ') > 0)
                    this._baseClassName = classLine.Substring(0, this._baseClassName.IndexOf(' '));
            }
        }

        private void AnalysisClassInfo(string code, List<ClassInfoItem> classInfoList)
        {
            //var usingLines = Regex.Matches(code, @"using HIS\.((WinUI)|(Facade))[\w.]+").Cast<Match>().Select(p => p.Value).ToList();
            var usingLines = Regex.Matches(code, @"using HIS\.[\w.]+").Cast<Match>().Select(p => p.Value).ToList();
            usingLines.Insert(0, "using " + this._namespace);
            usingLines.Insert(1, "using HIS.WinUI");

            #region Controller ClassInfo 생성
            {
                int startIndex = 0;
                string line = string.Empty;
                do
                {
                    //var regex = @"[^\s]*[\w .=]+new [\w]+Controller[^\s]*;";
                    startIndex = startIndex + "Controller".Length;
                    if (code.Length < startIndex) break;
                    startIndex = code.IndexOf("Controller", startIndex);
                    if (startIndex < 0) break;

                    line = code.ReadLine(startIndex);
                    if (line.StartsWith("//") || line.IndexOf("new ") < 0) continue;

                    var rLine = Regex.Replace(line, @"[\t ]", " ");

                    var item = new ClassInfoItem();
                    var type = rLine.LastBetween("new ", "(");
                    if (type.LastIndexOf('.') > 0)
                    {
                        item.Namespace = type.Substring(0, type.LastIndexOf('.'));
                        item.ClassName = type.RightBySearch(".").Trim();
                    }
                    else
                    {
                        item.ClassName = type.Trim();
                        var defined = rLine.LeftBySearch(" ");
                        if (defined.IndexOf(".") >= 0)
                            item.Namespace = defined.Between("", ".", false, true);
                    }

                    if (rLine.IndexOf("this.") >= 0)
                        item.Name = rLine.Between("this.", "=").Trim();
                    else
                        item.Name = rLine.Between("", "=").Trim();

                    if (item.Name.IndexOf(" ") > 0)
                        item.Name = item.Name.RightBySearch(" ").Trim();

                    item.Name = item.Name.RegexReturn(@"[\w]+");
                    if (string.IsNullOrEmpty(item.ClassName)
                        || string.IsNullOrEmpty(item.Name)
                        || item.Name.IndexOf(".") > 0) continue;

                    if (classInfoList.Where(p => p.Namespace == item.Namespace
                        && p.ClassName == item.ClassName
                        && p.Name == item.Name).Count() > 0) continue;

                    item.Namespace = GetNamespace(item, code, usingLines);

                    classInfoList.Add(item);

                } while (startIndex > 0);
            }
            #endregion

            #region Facade ClassInfo 생성
            {
                int startIndex = 0;
                string line = string.Empty;
                do
                {
                    //var regex = @"[^\s]*[\w .=]+new HIS.Facade[^\s]*;";
                    startIndex = startIndex + "Facade".Length;
                    if (code.Length < startIndex) break;
                    startIndex = code.IndexOf("Facade", startIndex);
                    if (startIndex < 0) break;

                    line = code.ReadLine(startIndex);
                    if (line.StartsWith("//") || line.IndexOf("new ") < 0) continue;

                    var rLine = Regex.Replace(line, @"[\t ]", " ");

                    var item = new ClassInfoItem();
                    var type = rLine.Between("new ", "(");
                    item.Namespace = type.IndexOf('.') >= 0 ? type.Substring(0, type.LastIndexOf('.')) : null;
                    item.ClassName = type.RightBySearch(".", true).Trim();

                    if (string.IsNullOrEmpty(item.Namespace))
                    {
                        var defined = rLine.LeftBySearch(" ");
                        if (defined.IndexOf(".") >= 0)
                            item.Namespace = defined.Between("", ".", false, true);
                    }

                    item.Name = rLine.Between(" ", "=").Trim();
                    if (string.IsNullOrEmpty(item.Name))
                        item.Name = rLine.Between("", "=").Trim();

                    if (item.Name.IndexOf(" ") > 0)
                        item.Name = item.Name.RightBySearch(" ").Trim();

                    item.Name = item.Name.RegexReturn(@"[\w]+");
                    if (string.IsNullOrEmpty(item.ClassName) || string.IsNullOrEmpty(item.Name)) continue;

                    if (classInfoList.Where(p => p.Namespace == item.Namespace
                        && p.ClassName == item.ClassName
                        && p.Name == item.Name).Count() > 0) continue;

                    item.Namespace = GetNamespace(item, code, usingLines);

                    classInfoList.Add(item);

                } while (startIndex > 0);
            }
            #endregion

            #region 스태틱 라이브러리 콜러 ClassInfo 생성
            {
                string[] fullNames = this._statics.Split(',');

                foreach (var fullName in fullNames)
                {
                    int startIndex = 0;
                    string line = string.Empty;
                    var ns = fullName.Trim().Left(fullName.LastIndexOf("."));
                    var className = fullName.RightBySearch(".");

                    do
                    {
                        startIndex = startIndex + line.Length;
                        if (code.Length < startIndex) break;
                        startIndex = code.IndexOf(className, startIndex);
                        if (startIndex < 0) break;

                        line = code.ReadLine(startIndex);
                        if (line.StartsWith("//")) continue;

                        var rLine = Regex.Replace(line, @"[\t ]", " ");

                        var item = new ClassInfoItem();
                        item.Namespace = ns;
                        item.ClassName = className;
                        item.Name = className;

                        if (classInfoList.Where(p => p.Namespace == item.Namespace
                            && p.ClassName == item.ClassName
                            && p.Name == item.Name).Count() > 0) continue;

                        classInfoList.Add(item);

                    } while (startIndex > 0);
                }
            }
            #endregion
        }

        private List<CallTreeItem> AnalysisCallInfo(string code, List<ClassInfoItem> classInfoList)
        {
            var list = new List<CallTreeItem>();
            if (classInfoList.Count < 1) return list;

            //var sb = new StringBuilder();
            //foreach (var classInfo in classInfoList)
            //{
            //    if (sb.Length > 0) sb.Append("|");
            //    sb.Append(string.Format("({0})", classInfo.Name));
            //}
            //var regex = string.Format(@"({0})\.[^\n]*", sb.ToString());
            foreach (var classInfo in classInfoList)
            {
                int startIndex = 0;
                string line = string.Empty;
                do
                {
                    startIndex = startIndex + classInfo.Name.Length;
                    if (code.Length < startIndex) break;
                    startIndex = code.IndexOf(string.Format("{0}.", classInfo.Name), startIndex);
                    if (startIndex < 0) break;

                    line = code.ReadLine(startIndex);
                    if (line.StartsWith("//") || line.StartsWith("using ")) continue;
                    var rLine = ReadEndLine(code, line, startIndex);
                    rLine = Regex.Replace(rLine, @"[\t ]", " ");

                    var methodStart = startIndex;
                    string methodLine = string.Empty;
                    do
                    {
                        if (methodStart < 0) break;
                        methodStart = Math.Max(code.LastIndexOf("public ", methodStart), code.LastIndexOf("private ", methodStart));
                        methodLine = code.ReadLine(methodStart).Trim();
                        if (methodLine.StartsWith("//"))
                        {
                            methodLine = string.Empty;
                            continue;
                        }   
                        else if (methodLine.EndsWith(")") != true)
                            methodLine = ReadEndLine(code, methodLine, methodStart);

                    } while (string.IsNullOrEmpty(methodLine));

                    if (string.IsNullOrEmpty(methodLine) || methodLine.IndexOf("(") < 0) continue;

                    var methodName = methodLine.Substring(0, methodLine.IndexOf("(")).RightBySearch(" ");
                    var methodRetunType = methodLine.IndexOf("public") > 0
                        ? methodLine.Between("public ", " ")
                        : methodLine.Between("private ", " ");
                    var methodParams = methodLine.Between("(", ")");

                    //var callFuncionName = rLine.Between(string.Format(@"{0}.", classInfo.Name), "(").Trim();
                    var callFuncionName = rLine.RegexReturn(classInfo.Name + @"\.[\w]+").Replace(classInfo.Name + ".", "");
                    if (string.IsNullOrEmpty(callFuncionName) || callFuncionName.IndexOf("=") >= 0) continue;

                    // 괄호찾기 123(aaa(bbb)ccc)ddd => (aaa(bbb)ccc)                    
                    //var regex = string.Format("{0}{1}", callFuncionName, @"\(([^()]*|\(([^()]*|\([^()]*\))*\))*\)");
                    //var callFuncionParams = rLine.RegexReturn(regex);
                    //callFuncionParams = callFuncionParams.Between("(", ")", false, true).Trim();
                    //string callFuncionParams = rLine.RightBySearch(callFuncionName).Between("(", ")", false, true);
                    var blocks = rLine.RightBySearch(callFuncionName).GetBlocks(false);
                    if (blocks.Length < 1) continue;
                    string callFuncionParams = blocks.ElementAt(0);
                    if (callFuncionParams.IndexOf("(") < 0 && callFuncionParams.IndexOf(")") > 0)
                    {
                        callFuncionParams = callFuncionParams.LeftBySearch(")");
                    }
                    //if (callFuncionParams.IndexOf("{") >= 0)
                    //{                        
                    //    var m = _RegexFuncParam.Match(rLine.RightBySearch(callFuncionName));
                    //    if (m.Success)
                    //        callFuncionParams = m.Value;
                    //}

                    var item = new CallTreeItem();
                    item.Namespace = this._namespace;
                    item.ClassName = this._className;
                    item.MethodName = methodName;
                    item.MethodParams = methodParams;
                    item.ReturnValue = methodRetunType;
                    var layer = _baseClassName.ConcatDiv("/", _className);
                    item.Layer = layer.IndexOf("Form", StringComparison.CurrentCultureIgnoreCase) >= 0
                        ? "WinUI"
                        : layer.IndexOf("Controller", StringComparison.CurrentCultureIgnoreCase) >= 0
                        ? "Controller"
                        : layer.IndexOf("Facade", StringComparison.CurrentCultureIgnoreCase) >= 0
                        ? "Facade"
                        : null;
                    if (item.Layer == null) Debugger.Break();

                    item.CallObjectNamespace = classInfo.Namespace;
                    item.CallObjectName = classInfo.ClassName;
                    item.CallFunctionName = callFuncionName;
                    item.CallFunctionParams = callFuncionParams;

                    if (string.IsNullOrEmpty(item.CallFunctionName)) continue;

                    list.Add(item);

                } while (startIndex > 0);
            }

            return list;
        }

        #endregion

        #region Methods

        private string GetNamespace(ClassInfoItem item, string code, List<string> usingLines)
        {
            string ns = string.Empty;

            if (string.IsNullOrEmpty(item.Namespace) || item.Namespace.StartsWith("this") || item.Namespace.StartsWith("HIS") != true)
            {
                string defined = string.Empty;
                foreach (var match in code.RegexMatches(@"[^\n]+" + item.Name))
                {
                    var line = match.Trim();
                    if (line.StartsWith("//")) continue;
                    if (line.LastIndexOf(" ") >= 0) line = line.Substring(0, line.LastIndexOf(" "));
                    if (line.LastIndexOf(" ") >= 0) defined = line.Substring(line.LastIndexOf(" "));
                    break;
                }
                if (defined.IndexOf(".") >= 0)
                    ns = defined.Between("", ".", false, true);

                if (string.IsNullOrEmpty(ns) || ns.StartsWith("this") || ns.StartsWith("HIS") != true)
                {
                    ns = string.Empty;
                    var basePath = _filePath.LeftBySearch(@"\Source", false, true);
                    foreach (var usingLine in usingLines)
                    {
                        var usingNS = usingLine.RightBySearch(" ").RegexReplace(@"[^\w.]+", string.Empty);
                        var usingNSArray = usingNS.Split('.');
                        string midPath = string.Empty, path;
                        for (int i = 1; i < usingNSArray.Length; i++)
                        {
                            midPath = midPath.ConcatDiv(@"\", usingNSArray.ElementAt(i));
                            path = System.IO.Path.Combine(basePath, string.Format(@"{0}\{1}\{2}.cs", midPath, usingNS, item.ClassName));
                            if (System.IO.File.Exists(path))
                            {
                                ns = usingNS;
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(ns) != true) break;
                    }
                }
            }

            return string.IsNullOrEmpty(ns)
                ? item.Namespace
                : ns;
        }

        private string ReadEndLine(string code, string line, int startIndex)
        {
            if (startIndex < 0 || line.EndsWith(";")) return line;

            var start = code.LastIndexOf("\n", startIndex);
            var end = code.IndexOf(";", startIndex);
            var blockStart = code.IndexOf("{", startIndex);
            if (blockStart >= 0
                && (blockStart < end && code.IndexOf("}", startIndex) > end)) end = blockStart;

            line = code.Substring(start, end - start).Trim();

            var sb = new StringBuilder();
            foreach (var s1 in line.Split('\n'))
            {
                var s2 = s1.Trim();
                if (s2.StartsWith("//")) continue;
                if (s2.IndexOf("//") > 0)
                    s2 = s2.LeftBySearch("//").Trim();
                sb.Append(s2);
            }
            return sb.ToString().RegexReplace(@"([\r\n]+|( ){2,})", "");
        }

        #endregion
    }

    public partial class CallTreeAnalysis
    {
        public static List<CallTreeItem> GetCallTree(string path, string statics)
        {
            var analysis = new CallTreeAnalysis(path, statics);
            return analysis.CallTreeList;
        }

        public class ClassInfoItem
        {
            public string Namespace { get; set; }
            public string ClassName { get; set; }
            public string Name { get; set; }
        }

        public class CallTreeItem
        {
            public string Namespace { get; set; }
            public string ClassName { get; set; }
            public string MethodName { get; set; }
            public string MethodParams { get; set; }

            public string Layer { get; set; }

            public string ReturnValue { get; set; }

            public string CallObjectNamespace { get; set; }
            public string CallObjectName { get; set; }
            public string CallFunctionName { get; set; }
            public string CallFunctionParams { get; set; }
        }
    }
}
