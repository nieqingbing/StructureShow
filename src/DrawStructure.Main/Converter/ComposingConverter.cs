using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DrawStructure.Main
{
    public class ComposingConverter : IValueConverter
    {
        private List<IValueConverter> converters = new List<IValueConverter>();

        public Collection<IValueConverter> Converters
        {
            get { return new Collection<IValueConverter>(this.converters); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            for (int i = 0; i < this.converters.Count; i++)
            {
                value = converters[i].Convert(value, targetType, parameter, culture);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            for (int i = this.converters.Count - 1; i >= 0; i--)
            {
                value = converters[i].ConvertBack(value, targetType, parameter, culture);
            }
            return value;
        }
    }
}
