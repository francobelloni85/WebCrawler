using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WebCrawler.Assets.Convert
{
    
    class WebsiteUrlStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var t = (Models.WebsiteUrlStatus)value;


                switch (t.Status) {

                    case Models.EnumStatus.onStartup:
                        return FontAwesome.WPF.FontAwesomeIcon.HourglassStart.ToString(); //"hourglass-start";

                    case Models.EnumStatus.working:
                        return FontAwesome.WPF.FontAwesomeIcon.Spinner.ToString(); //"Spinner";

                    case Models.EnumStatus.onPause:
                        return FontAwesome.WPF.FontAwesomeIcon.Pause.ToString(); // "pause";

                    case Models.EnumStatus.onStop:
                        return FontAwesome.WPF.FontAwesomeIcon.Stop.ToString(); // "stop";

                    case Models.EnumStatus.finish:
                        return FontAwesome.WPF.FontAwesomeIcon.CalendarCheckOutline.ToString();  //"calendar-check";
                        
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
