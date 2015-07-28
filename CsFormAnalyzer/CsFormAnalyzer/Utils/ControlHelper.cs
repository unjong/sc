using CsFormAnalyzer.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CsFormAnalyzer.Utils
{
    public static class ControlHelper
    {        
        public static T FindVisualParent<T>(DependencyObject child)
             where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                // use recursion to proceed with next level
                return FindVisualParent<T>(parentObject);
            }
        }

        public static void RemoveSelectedItem(this Selector selector)
        {
            var selectedItem = selector.SelectedItem;
            if (selectedItem == null) return;

            RemoveItem(selector, selectedItem);
        }

        public static void AddNewItem(ItemsControl itemsControl)
        {
            if (itemsControl.ItemsSource == null)
            {
                //itemsControl.Items.Add(newItem);
            }
            else
            {
                var coll = itemsControl.ItemsSource as IList;
                var type = coll.GetType().GetGenericArguments().FirstOrDefault();
                if (type == null) return;

                var newItem = Activator.CreateInstance(type);

                if (newItem is IConditionalTemplateItem)
                {
                    var last = coll[coll.Count - 1];
                    if ("IsNew".Equals(((IConditionalTemplateItem)last).MatchingKey))
                    {
                        coll.Insert(coll.Count - 1, newItem);
                        return;
                    }
                }

                coll.Add(newItem);
            }
        }

        public static void RemoveItem(ItemsControl itemsControl, object item)
        {
            if (itemsControl.ItemsSource == null)
            {
                itemsControl.Items.Remove(item);
            }
            else
            {
                var collection = itemsControl.ItemsSource as IList;
                if (collection == null) return;

                if (collection.Contains(item))
                    collection.Remove(item);
                else
                {
                    var el = item as System.Windows.FrameworkElement;
                    var targetItem = itemsControl.ItemsSource.Cast<object>().Where(p => p == item || (el != null && el.DataContext == item)).FirstOrDefault();
                    if (targetItem == null) return;

                    collection.Remove(targetItem);
                }               
            }
        }
    }
}
