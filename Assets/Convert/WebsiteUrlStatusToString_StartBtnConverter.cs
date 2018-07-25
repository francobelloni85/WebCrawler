using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WebCrawler.Assets.Convert
{
   
    class WebsiteUrlStatusToString_StartBtnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var t = (Models.WebsiteUrlStatus)value;

                switch (t.Status) {
                    case Models.EnumStatus.working:
                        return "Pause";
                    case Models.EnumStatus.onPause:
                        return "Resume";
                }

                if (string.IsNullOrWhiteSpace(t.Ulr) == false)
                {
                    return "Start";
                }
            }

            return "Start 1";

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
