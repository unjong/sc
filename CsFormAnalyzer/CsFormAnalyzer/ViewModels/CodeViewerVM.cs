using CsFormAnalyzer.Mvvm;

namespace CsFormAnalyzer.ViewModels
{
    public class CodeViewerVM : ViewModelBase
    {
        public string Code { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string SearchText { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
    }
}
