using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WebCrawler.Assets.Convert
{


    class WebsiteUrlStatusToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null) {
                var t = (Models.WebsiteUrlStatus)value;

                if (t.Status == Models.EnumStatus.working)
                    return true;

                if (string.IsNullOrWhiteSpace(t.Ulr) == false) {
                    return true;
                }
            }

            return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
