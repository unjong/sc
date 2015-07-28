using CsFormAnalyzer.Core;
using CsFormAnalyzer.Utils;
using CsFormAnalyzer.Mvvm;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Data;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Text;

namespace CsFormAnalyzer.ViewModels
{
	partial class ProjectConterVM : ViewModelBase
    {
        #region Initialize...

        public ProjectConterVM()
        {
            Result = new List<CsFileInfo>();
            Files = new List<ProjectConterVM.FileInfo>();

            this.FindOptionIsText = true;
        }

        #endregion

        #region Properties

        public string TargetPath { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public int WorkIndex { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public IEnumerable<CsFileInfo> Result { get { return _Result; } set { _Result = value; OnPropertyChanged(); } }
        private IEnumerable<CsFileInfo> _Result;

        public IEnumerable<ProjectConterVM.FileInfo> Files { get { return _Files; } set { _Files = value; OnPropertyChanged(); } }
        private IEnumerable<ProjectConterVM.FileInfo> _Files;

        public bool FindOptionIsText { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public bool FindOptionIsRegex { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string SearchText { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public IEnumerable<FindItem> FindResult { get { return _FindResult; } set { _FindResult = value; OnPropertyChanged(); } }
        private IEnumerable<FindItem> _FindResult;

        public List<ClassInfoItem> ClassListDetails { get { return _ClassListDetails; } set { _ClassListDetails = value; OnPropertyChanged(); } }
        private List<ClassInfoItem> _ClassListDetails;

        #endregion

        #region Commands

        public ICommand RunCommand { get; private set; }

        public override void InitCommands()
        {
            RunCommand = base.CreateCommand(OnExecuteRunCommand);
        }

        private void OnExecuteRunCommand(object param)
        {
            base.InvokeAsyncAction(Run);
        }

        #endregion

        #region Methods

        public void Run()
        {
            if (WorkIndex == 0)
                RunCsFileInfoList();

            else if (WorkIndex == 1)
                RunFileInfoList();

            else if (WorkIndex == 2)
                RunFindFiles();

            else if (WorkIndex == 3)
                RunClassListDetails();
        }

        #endregion

        #region RunCsFileInfoList

        private void RunCsFileInfoList()
        {
            var files = Directory.GetFiles(this.TargetPath, "*.cs", System.IO.SearchOption.AllDirectories);

            var menuFiles = new List<string>();
            var list = new List<CsFileInfo>();
            foreach (var file in files)
            {
                AnalysisCsFile(list, file);

                if (Path.GetFileName(file).IndexOf("Menu") >= 0)
                {
                    menuFiles.Add(file);
                }
            }

            foreach (var file in menuFiles)
            {
                AnalysisMenu(list, file);
            }

            #region 보정

            { // Designer.cs 파일의 Title을 Form의 Title과 맞춰준다.
                foreach (var item in list.Where(p => p.FileName.EndsWith(".Designer.cs")))
                {
                    foreach (var targetItem in list.Where(p => p.FileName.Equals(item.FileName.Replace(".Designer.cs", ".cs"))))
                    {
                        targetItem.Title = targetItem.Title;
                    }
                }
            }
            {
                var group = list.GroupBy(p => new { p.Directory, p.FileName });
                foreach (var item in group.Where(p => p.Count() > 1))
                {
                    CsFileInfo firstItem = null;
                    foreach (var targetItem in
                        list.Where(p => p.Directory.Equals(item.Key.Directory)
                        && p.FileName.Equals(item.Key.FileName)).OrderByDescending(p => p.Title)
                        .ThenBy(p => p.Menu))
                    {
                        if (firstItem == null)
                            firstItem = targetItem;
                        else
                        {
                            targetItem.Title = firstItem.Title;
                            targetItem.Menu = firstItem.Menu;
                            targetItem.MenuFile = firstItem.MenuFile;
                        }
                    }
                }
            }

            #endregion


            this.Result = list.OrderBy(p => p.Directory).ThenBy(p => p.FileName);
        }

        private void AnalysisCsFile(List<CsFileInfo> list, string filePath)
        {
            var code = IOHelper.ReadFileToString(filePath);

            var directory = Path.GetDirectoryName(filePath);
            if (directory.IndexOf("WinUI") > 0)
                directory = directory.Substring(directory.IndexOf("WinUI"));
            else if (directory.IndexOf("COM") > 0)
                directory = directory.Substring(directory.IndexOf("COM"));

            var fileName = Path.GetFileName(filePath);
            if (fileName.EndsWith("_re.cs") || fileName.EndsWith("_re.Designer.cs")) return;

            string title = null;
            var titleLine = code.RegexReturn(@"[^\s\n]*this\.Text[^\n]*;");
            if (string.IsNullOrEmpty(titleLine) != true)
                title = titleLine.Between(@"""", @"""");

            var regex = @"(class [^\n]*)";
            foreach (var match in Regex.Matches(code, regex))
            {
                var line = match.ToString().Trim();
                if (line.StartsWith("//")) continue;

                var item = new CsFileInfo();
                item.Directory = directory;
                item.FileName = fileName;
                item.Class = line.RegexReturn(@"class[ ]+[\S]+").RightBySearch(" ");
                item.BaseClass = line.RightBySearch(":").RegexReturn(@"[\w\.:]+").RightBySearch(".", true);
                item.Title = title;
                //item.Line = line;
                list.Add(item);
            }
        }

        private void AnalysisMenu(List<CsFileInfo> list, string filePath)
        {
            var code = IOHelper.ReadFileToString(filePath);

            #region MenuNames
            var menuNames = new List<string>();
            {
                { // mainMenu
                    var line = code.RegexReturn(@"this\.[\w]+ = new System\.Windows\.Forms\.MainMenu\(");
                    if (line == null) return;

                    var name = line.Between("this.", " ");
                    menuNames.Add(name);
                }

                var regex = @"this\.[\w]+ = new System\.Windows\.Forms\.MenuItem\(\);";
                foreach (var match in Regex.Matches(code, regex))
                {
                    var line = match.ToString().Trim();
                    if (line.StartsWith("//")) continue;

                    var name = line.Between("this.", " ");
                    menuNames.Add(name);
                }

                if (menuNames.Count < 1) return;
            }
            #endregion

            #region MenuItemInfo
            var menuPropertyList = new List<MenuItemProperty>();
            {
                var regex = @"this\.[\w]+\.[\w]+ = [^\n]+";
                foreach (var match in Regex.Matches(code, regex))
                {
                    var line = match.ToString().Trim();
                    if (line.StartsWith("//")) continue;

                    var name = line.Between("this.", ".");
                    var property = line.Between(name + ".", " =");
                    var value = line.Between(" = ", ";");

                    if (menuNames.Contains(name) != true) continue;

                    var item = new MenuItemProperty();
                    item.Name = name;
                    item.Property = property;
                    item.Value = value;

                    menuPropertyList.Add(item);
                }

                if (menuPropertyList.Count < 1) return;
            }
            #endregion

            #region menu parent
            var menuList = new List<MenuItemInfo>();
            {
                var regex = @"this\.[\w]+\.MenuItems.AddRange\(new System\.Windows\.Forms\.MenuItem\[\] {[\n\w\s\.,]+}\);";
                foreach (var match in Regex.Matches(code, regex))
                {
                    var line = match.ToString().Trim();

                    var parent = line.Between("this.", ".MenuItems");
                    var parameters = line.Between("{", "}");
                    foreach (var param in parameters.Split(','))
                    {
                        var parameter = param.Trim();
                        var name = parameter.Substring("this.".Length);

                        var item = new MenuItemInfo();
                        item.Name = name;
                        item.Text = menuPropertyList.Where(p => p.Name.Equals(name) && p.Property.Equals("Text")).FirstOrDefault().Value;
                        item.Parent = parent;
                        menuList.Add(item);
                    }
                }

                foreach (var item in menuList)
                {
                    MenuItemInfo parent = item;
                    item.Depth = item.Text;
                    do
                    {
                        parent = menuList.Where(p => p.Name.Equals(parent.Parent)).FirstOrDefault();
                        if (parent == null) break;
                        item.Depth = parent.Text.ConcatDiv(@"\", item.Depth);
                    } while (parent != null);

                    item.Depth = Regex.Replace(item.Depth, "\"", "");
                }
            }
            #endregion

            #region call menu

            var arrayPath = Regex.Split(filePath, @"\\");
            var dir = arrayPath.ElementAt(arrayPath.Count() - 2);

            foreach (var item in list.Where(p => p.BaseClass != null && p.BaseClass.IndexOf("Form") >= 0))
            {
                if (item.Directory.IndexOf(dir.Substring(0, dir.LastIndexOf('.'))) < 0) continue;

                string callMethodLine = string.Empty;
                int startIndex = 0;
                do
                {
                    callMethodLine = string.Empty;
                    var callLine = code.IndexOf(string.Format(".{0}(", item.Class), startIndex);
                    if (callLine < 0)
                    {
                        callLine = code.IndexOf(string.Format(" {0}(", item.Class), startIndex);
                        if (callLine < 0) break;
                    }

                    var start = code.LastIndexOf(" void ", callLine);
                    if (start < 0) break;
                    var end = code.IndexOf("_Click", start);
                    if (end < 0) break;
                    callMethodLine = code.Substring(start, end - start);

                    startIndex = callLine + 100;

                } while (callMethodLine.IndexOf('\n') >= 0);
                if (string.IsNullOrEmpty(callMethodLine)) continue;

                var name = callMethodLine.Substring(" void ".Length);
                var menu = menuList.Where(p => p.Name.Equals(name)).FirstOrDefault();
                if (menu == null) continue;

                item.MenuFile = filePath.Substring(filePath.IndexOf("WinUI"));
                item.Menu = menu.Depth;
            }

            #endregion
        }

        #endregion

        #region RunFileInfoList

        private void RunFileInfoList()
        {
            var files = Directory.GetFiles(this.TargetPath, "*.*", System.IO.SearchOption.AllDirectories);

            var list = new List<FileInfo>();
            foreach (var file in files)
            {
                var item = new FileInfo();
                //item.Path = Path.GetDirectoryName(file);
                //if (item.Path.IndexOf("81.전체화면분석") > 0)
                //    item.Path = item.Path.Substring(item.Path.IndexOf("81.전체화면분석"));
                //item.FileName = Path.GetFileName(file);
                var info = new System.IO.FileInfo(file);
                item.Directory = info.Directory.ToString();
                item.Name = info.Name;
                item.Size = info.Length;
                item.CreateTime = info.CreationTime.ToString("yyyy-MM-dd hh:mm");
                item.LastWriteTime = info.LastWriteTime.ToString("yyyy-MM-dd hh:mm");

                list.Add(item);
            }
            this.Files = list;

            //var sb = new StringBuilder();
            //foreach (var item in list)
            //{
            //    sb.AppendLine(item.FullPath);
            //}
            //var savePath = @"D:\aa.txt";
            //System.IO.File.WriteAllText(savePath, sb.ToString(), Encoding.Default);
        }

        #endregion

        #region RunFindFiles
        
        private void RunFindFiles()
        {
            if (string.IsNullOrEmpty(this.SearchText)) return;

            var list = new List<FindItem>();

            var files = Directory.GetFiles(this.TargetPath, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var code = IOHelper.ReadFileToString(file);
                var lines = FindLines(code);
                if (lines.Count > 0)
                {
                    var item = new FindItem();
                    item.FilePath = file;
                    item.Lines = string.Join(", ", lines.ToArray());
                    list.Add(item);
                }
            }

            FindResult = list;
        }

                private List<int> FindLines(string code)
        {
            var list = new List<int>();
            var searchText = this.SearchText;
            var lines = code.Split('\n');

            for (int i = 0; i < lines.Count(); i++)
            {
                var line = lines[i];
                if (FindOptionIsText)
                {
                    if (line.IndexOf(searchText) >= 0)
                    {
                        list.Add(i);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(line.RegexReturn(searchText)) != true)
                    {
                        list.Add(i);
                    }
                }
            }

            return list;
        }


        #endregion
        
        #region RunClassListDetails

                private void RunClassListDetails()
                {
                    var list = new List<ClassInfoItem>();

                    var files = Directory.GetFiles(this.TargetPath, "*.*", System.IO.SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        ParseClassListDetails(list, file);
                    }

                    ClassListDetails = list;

                    IOHelper.ListToFile(list, @"d:\aaa.txt", "§");
                }

                private void ParseClassListDetails(List<ClassInfoItem> list, string filePath)
                {
                    var fileName = Path.GetFileName(filePath);
                    if (fileName.EndsWith("_re.cs") || fileName.EndsWith("_re.Designer.cs")) return;

                    string subFilePath = filePath;
                    if (filePath.IndexOf("WinUI") > 0)
                        subFilePath = filePath.Substring(filePath.IndexOf("WinUI"));
                    else if (filePath.IndexOf("Facade") > 0)
                        subFilePath = filePath.Substring(filePath.IndexOf("Facade"));
                    else if (filePath.IndexOf("COM") > 0)
                        subFilePath = filePath.Substring(filePath.IndexOf("COM"));

                    var fileSize = new System.IO.FileInfo(filePath).Length;
                    var code = IOHelper.ReadFileToString(filePath);
                    var lines = new List<string>();
                    foreach (var line in code.Split('\n'))
                    {
                        lines.Add(line.Trim());
                    }

                    string className = null, baseClass = null;
                    var regex = @"(class|public|private)([^{^;]+)";
                    foreach (var match in Regex.Matches(code, regex))
                    {
                        var line = match.ToString().Trim();
                        if (line.StartsWith("//")) continue;

                        if (line.IndexOf(" class ") > 0)
                        {
                            className = line.RegexReturn(@"class[ ]+[\S]+").RightBySearch(" ");
                            baseClass = line.RightBySearch(":").RegexReturn(@"[\w\.:]+").RightBySearch(".", true);
                            continue;
                        }
                        if (string.IsNullOrEmpty(className)) continue;
                        if ((line.IndexOf("(") > 0 && line.IndexOf(")") > 0) != true) continue;

                        var methodName = line.LeftBySearch(")", true) + ")";
                        string description = null;

                        string methodLine;
                        var methodLineNdx = lines.IndexOf(line);
                        if (methodLineNdx > 0)
                            methodLine = lines.ElementAt(methodLineNdx);
                        else
                            methodLine = lines.Where(p => p.IndexOf(line) > 0).FirstOrDefault();

                        if (methodLine != null)
                        {
                            int lineIndex = lines.IndexOf(methodLine) - 1;
                            int summaryStart = 0, summaryEnd = 0;
                            while (true)
                            {
                                var targetLine = lines.ElementAt(lineIndex).Trim();
                                if (targetLine.StartsWith("/// <summary>"))
                                    summaryStart = lineIndex;

                                else if (targetLine.StartsWith("/// </summary>"))
                                    summaryEnd = lineIndex;

                                else if (targetLine.StartsWith("//") != true
                                    || lineIndex < 1
                                    || (summaryStart > 0 && summaryEnd > 0)
                                    )
                                    break;

                                lineIndex -= 1;
                            }

                            if (summaryStart > 0 && summaryEnd > 0)
                            {
                                for (int i = summaryStart + 1; i < summaryEnd; i++)
                                {
                                    description = description.ConcatDiv("$newline$", lines.ElementAt(i).Trim());
                                }
                            }
                        }

                        var item = new ClassInfoItem();
                        item.FilePath = subFilePath;
                        item.FileSize = fileSize;
                        item.BaseClass = baseClass;
                        item.ClassName = className;
                        item.Method = methodName.RegexReplace(@"[\r\n\s]+", " ");
                        item.Description = description;
                        list.Add(item);
                    }
                }

        #endregion
    }

    partial class ProjectConterVM : ViewModelBase
    {
        public class CsFileInfo
        {
            public string Directory { get; set; }
            public string FileName { get; set; }
            public string Class { get; set; }
            public string BaseClass { get; set; }
            //public string Line { get; set; }
            public string Title { get; set; }
            public string Menu { get; set; }
            public string MenuFile { get; set; }            
        }

        class MenuItemProperty
        {
            public string Name { get; set; }
            public string Property { get; set; }
            public string Value { get; set; }
        }

        class MenuItemInfo
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public string Parent { get; set; }
            public string Depth { get; set; }
        }

        public class FileInfo
        {
            //public string Path { get; set; }
            //public string FileName { get; set; }
            //public string FullPath { get; set; }

            public string Directory { get; set; }
            public string Name { get; set; }
            public double Size { get; set; }

            public string CreateTime { get; set; }

            public string LastWriteTime { get; set; }
        }

        public class FindItem
        {
            public string FilePath { get; set; }

            //public List<int> Lines { get; set; }
            public string Lines { get; set; }
        }

        public class ClassInfoItem
        {
            public string FilePath { get; set; }
            public long FileSize { get; set; }
            public string BaseClass { get; set; }
            public string ClassName { get; set; }
            public string Method { get; set; }
            public string Description { get; set; }
        }        
    }
}
