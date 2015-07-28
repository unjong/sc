using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CsFormAnalyzer.Mvvm
{
    public interface IConditionalTemplateItem
    {
        string MatchingKey { get; set; }
    }

    public class ConditionalTemplateItem
    {
        public Type TargetType { get; set; }
        public DataTemplate DataTemplate { get; set; }
        public string MatchingKey { get; set; }
        public bool IsDefault { get; set; }

        public bool IsMatching(object item)
        {
            if (item is IConditionalTemplateItem)
            {
                return ((IConditionalTemplateItem)item).MatchingKey == MatchingKey;
            }
            else
            {
                return this.TargetType == item.GetType();
            }
        }
    }

    [ContentProperty("Templates")]
    public class ConditionalTemplateSelector : DataTemplateSelector
    {
        private ConditionalTemplateItem defaultItem;
        public ObservableCollection<ConditionalTemplateItem> Templates { get; set; }

        public ConditionalTemplateSelector()
        {
            Templates = new ObservableCollection<ConditionalTemplateItem>();
        }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var matchingItem = Templates.Where(p => p.IsMatching(item)).FirstOrDefault();
            if (matchingItem != null) return matchingItem.DataTemplate;

            if (defaultItem == null) defaultItem = Templates.Where(p => p.IsDefault).FirstOrDefault();
            if (defaultItem != null) return defaultItem.DataTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
