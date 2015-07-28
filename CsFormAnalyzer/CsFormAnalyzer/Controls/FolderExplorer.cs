using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CsFormAnalyzer.Controls
{
	[TemplatePart(Name="PART_TreeView", Type=typeof(TreeView))]

	public class FolderExplorer : Control
	{
		private TreeView PART_TreeView;
		private object dummyNode = null;

		static FolderExplorer()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FolderExplorer), new FrameworkPropertyMetadata(typeof(FolderExplorer)));
		}

		public string SelectedPath
		{
			get { return (string)GetValue(SelectedPathProperty); }
			set { SetValue(SelectedPathProperty, value); }
		}

		public static readonly DependencyProperty SelectedPathProperty =
			DependencyProperty.Register("SelectedPath", typeof(string), typeof(FolderExplorer), new FrameworkPropertyMetadata()
			{
				BindsTwoWayByDefault = true,
                PropertyChangedCallback = OnSelectedPathChanged
			}
            );

        public delegate void SelectedPathChangedHandler(object sender, DependencyPropertyChangedEventArgs e);
        public event SelectedPathChangedHandler SelectedPathChanged;

        private static void OnSelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var el = d as FolderExplorer;
            if (el.SelectedPathChanged != null)
                el.SelectedPathChanged(d, e);
        }

		public string SearchPattern
		{
			get { return (string)GetValue(SearchPatternProperty); }
			set { SetValue(SearchPatternProperty, value); }
		}

		public static readonly DependencyProperty SearchPatternProperty =
			DependencyProperty.Register("SearchPattern", typeof(string), typeof(FolderExplorer), new FrameworkPropertyMetadata("*.*"));

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PART_TreeView = this.GetTemplateChild("PART_TreeView") as TreeView;
			PART_TreeView.Loaded += PART_TreeView_Loaded;
			PART_TreeView.SelectedItemChanged += PART_TreeView_SelectedItemChanged;
		}
		
		private void PART_TreeView_Loaded(object sender, RoutedEventArgs e)
		{
			foreach (string s in Directory.GetLogicalDrives())
			{
				TreeViewItem item = new TreeViewItem();
				item.Header = s;
				item.Tag = s;
				item.FontWeight = FontWeights.Normal;
				item.Items.Add(dummyNode);
				item.Expanded += new RoutedEventHandler(folder_Expanded);
				PART_TreeView.Items.Add(item);
			}
		}

		void folder_Expanded(object sender, RoutedEventArgs e)
		{
			TreeViewItem item = (TreeViewItem)sender;
			if (item.Items.Count == 1 && item.Items[0] == dummyNode)
			{
				item.Items.Clear();
				try
				{
					foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
					{
						TreeViewItem subitem = new TreeViewItem();
						subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
						subitem.Tag = s;
						subitem.FontWeight = FontWeights.Normal;
						subitem.Items.Add(dummyNode);
						subitem.Expanded += new RoutedEventHandler(folder_Expanded);
						item.Items.Add(subitem);
					}

					foreach (string s in Directory.GetFiles(item.Tag.ToString(), SearchPattern, SearchOption.TopDirectoryOnly))
					{
						TreeViewItem subitem = new TreeViewItem();
						subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
						subitem.Tag = s;
						subitem.FontWeight = FontWeights.Normal;
						item.Items.Add(subitem);
					}
				}
				catch (Exception) { }
			}
		}

		private void PART_TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			TreeView tree = (TreeView)sender;
			TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);

			if (temp == null)
			{
				SetCurrentValue(SelectedPathProperty, null);
				return;
			}

			string path = "";
			string temp1 = "";
			string temp2 = "";
			while (true)
			{
				temp1 = temp.Header.ToString();
				if (temp1.Contains(@"\"))
				{
					temp2 = "";
				}
				path = temp1 + temp2 + path;
				if (temp.Parent.GetType().Equals(typeof(TreeView)))
				{
					break;
				}
				temp = ((TreeViewItem)temp.Parent);
				temp2 = @"\";
			}

			SetCurrentValue(SelectedPathProperty, path);            
		}		
	}
}
