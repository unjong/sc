using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CsFormAnalyzer.Converters
{
    public class StringToBoolByParamConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var returnValue = parameter.ToString().Equals(System.Convert.ToString(value));
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
