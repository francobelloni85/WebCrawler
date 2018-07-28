using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WebCrawler.Models
{
    public enum EnumStatus { onStartup, working, onPause, onStop, finish }

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

    public class WebsiteUrlStatus
    {

        public string Ulr { get; set; }
        public EnumStatus Status { get; set; } = EnumStatus.onStartup;

    }

    public class ValidExtensionData
    {

        public List<ExtensionData> ValidExtension = new List<ExtensionData>();

        public ValidExtensionData()
        {

            ValidExtension.Add(new ExtensionData() { Name = ".jpeg", IsValid = false });
            ValidExtension.Add(new ExtensionData() { Name = ".jpg", IsValid = false });
            ValidExtension.Add(new ExtensionData() { Name = ".js", IsValid = false });
            ValidExtension.Add(new ExtensionData() { Name = ".css", IsValid = false });
            ValidExtension.Add(new ExtensionData() { Name = ".gif", IsValid = false });
            ValidExtension.Add(new ExtensionData() { Name = ".doc", IsValid = true });
            ValidExtension.Add(new ExtensionData() { Name = ".word", IsValid = true });
            ValidExtension.Add(new ExtensionData() { Name = ".zip", IsValid = false });

        }

        public ValidExtensionData(System.Collections.ObjectModel.ObservableCollection<Models.ExtensionData> UserPreferenceList) {

            foreach (ExtensionData item in UserPreferenceList) {
                ValidExtension.Add(item);
            }
                

        }



    }

    public class ExtensionData
    {

        public string Name { get; set; }
        public bool IsValid { get; set; }

    }



}
