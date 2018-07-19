using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace WebCrawler.Assets.Convert
{


    class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                ObservableCollection<string> t = (ObservableCollection<string>)value;
                if (t.Count >= 1) {
                    return "[" + t.Count + "]";
                }               
            }

            return "";

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
