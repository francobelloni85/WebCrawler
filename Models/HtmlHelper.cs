using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Models
{
    public enum EnumStatus { working, notWorking }

    class HtmlHelper
    {

        public static string GetHtmlCode(string _url)
        {

            string data = null;

            try
            {

                string urlAddress = _url;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream);
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }

                    data = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();
                }
            }
            catch
            {

            }

            return data;


        }

    }

    public class WebsiteUrlStatus {

        public string Ulr { get; set; }
        public EnumStatus Status { get; set; } = EnumStatus.notWorking;

    }




}
