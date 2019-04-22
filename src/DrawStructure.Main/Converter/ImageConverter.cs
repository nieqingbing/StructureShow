﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DrawStructure.Main
{
    public class ImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(value.ToString()));

                // Use BitmapCacheOption.OnLoad to prevent binding the source holding on to the photo file.
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                return bitmap;
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("NotImplementedException");
        }

        #endregion
    }
}
