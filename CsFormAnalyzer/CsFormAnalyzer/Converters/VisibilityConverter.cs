using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CsFormAnalyzer.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int i = 0;
            Visibility returnValue = Visibility.Collapsed;
            if (value != null)
            {
                if (int.TryParse(value.ToString(), out i))
                {
                    if (parameter != null)
                    {
                        int p = 0;
                        if (int.TryParse(parameter.ToString(), out p))
                        {
                            returnValue = (i > p) ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        return (i > 0) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else if (value is string)
                {
                    returnValue = (value.ToString().Length > 0) ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                    returnValue = Visibility.Visible;
            }
            else
                returnValue = Visibility.Collapsed;

            if(parameter!=null&& parameter.ToString() == "I")
            {
                if (returnValue == Visibility.Visible)
                    returnValue = Visibility.Collapsed;
                else
                    returnValue = Visibility.Visible;
            }

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //if (System.Convert.ToBoolean(value))
            //    return parameter;
            //else
            return parameter;
        }
    }
}
