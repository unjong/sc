using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CsFormAnalyzer.Converters
{
	[ValueConversion(typeof(string), typeof(bool))]
	public class FolderIconConverter : IValueConverter
	{
		public static FolderIconConverter Instance = new FolderIconConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var tvi = value as TreeViewItem;
			var path = tvi.Header.ToString();
			if (path.Contains(@"\"))
			{
				//Uri uri = new Uri("pack://application:,,,/lib/images/diskdrive.png");
				Uri uri = new Uri("pack://application:,,,/CsFormAnalyzer;component/lib/images/diskdrive.png");
				BitmapImage source = new BitmapImage(uri);
				return source;
			}
			else
			{
				while (tvi.Parent is TreeViewItem)
				{
					tvi = tvi.Parent as TreeViewItem;
					path = string.Join(@"\", tvi.Header.ToString(), path);
				}

				if (Directory.Exists(path))
				{
					//Uri uri = new Uri("pack://application:,,,/lib/images/folder.png");
					Uri uri = new Uri("pack://application:,,,/CsFormAnalyzer;component/lib/images/folder.png");
					BitmapImage source = new BitmapImage(uri);
					return source;
				}
				else
					return null;
				//var attr = File.GetAttributes(path);
				//if (attr == FileAttributes.Directory)
				//{
				//	Uri uri = new Uri("pack://application:,,,/lib/images/folder.png");
				//	BitmapImage source = new BitmapImage(uri);
				//	return source;
				//}
				//else
				//{
				//	return null;
				//}
				//Uri uri = new Uri("pack://application:,,,/lib/images/folder.png");
				//BitmapImage source = new BitmapImage(uri);
				//return source;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("Cannot convert back");
		}
	}
}
