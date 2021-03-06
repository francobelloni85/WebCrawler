﻿using HtmlAgilityPack;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;


namespace WebCrawler.ViewModels
{
    // THEME 
    // https://www.nuget.org/packages/MaterialDesignThemes/
    // https://mahapps.com/guides/quick-start.html

    // CHARS
    // https://lvcharts.net/App/examples/v1/wpf/Basic%20Line%20Chart
    // (or) https://github.com/oxyplot/oxyplot

    public class ButtonsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> executeMethod;
        private Predicate<object> CanExecuteMethod;

        public ButtonsCommand(Action<object> _executeMethod, Predicate<object> _canExecuteChange)
        {
            this.executeMethod = _executeMethod;
            this.CanExecuteMethod = _canExecuteChange;
        }

        public ButtonsCommand(Action<object> ExecuteMethod) : this(ExecuteMethod, null)
        {

        }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteMethod == null)
                return true;

            return CanExecuteMethod.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            System.Diagnostics.Debug.WriteLine("Click button");
            executeMethod.Invoke(parameter);
        }

        public void RaiseCanExecuteChange()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    public abstract class NotifyPropertyMainWindows : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string namePropertyChanged = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(namePropertyChanged));
        }

    }

    public class MainWindowViewModel : NotifyPropertyMainWindows
    {
        public ObservableCollection<string> UrlDone { get; set; }
        public ObservableCollection<string> UrlToDo { get; set; }
        public ObservableCollection<string> UrlError { get; set; }

        public ObservableCollection<Models.ExtensionData> UserPreferenceList { get; set; }

        private string fullUrlWebsite;
        private Uri fullUriWebsite;

        // Costruttore <<<<
        public MainWindowViewModel()
        {
            UrlDone = new ObservableCollection<string>();
            UrlToDo = new ObservableCollection<string>();
            UrlError = new ObservableCollection<string>();
            UserPreferenceList = new ObservableCollection<Models.ExtensionData>();

            this.StartCommand = new ButtonsCommand(StartCommandExecute, CanStartCommand);
            this.StopCommand = new ButtonsCommand(StopCommandExecute, CanStopCommand);
            this.CreateSitemapCommand = new ButtonsCommand(CreateSitemapExecute, CanCreateSitemapCommand);
            this.ValidExtensionCommand = new ButtonsCommand(ValidExtensionExecute, CanValidExtensionCommand);
            this.SavePreferenceCommand = new ButtonsCommand(SavePreferenceExecute, CanSavePreferenceCommand);

            LoadUserPreference();
            
        }

        private void LoadUserPreference()
        {
            string userPreferenceSerialize = string.Empty;

            if (String.IsNullOrEmpty(Properties.Settings.Default.FileExtension) == true)
            {
                Models.ValidExtensionData valid = new Models.ValidExtensionData();
                userPreferenceSerialize = JsonConvert.SerializeObject(valid);
                Properties.Settings.Default.FileExtension = userPreferenceSerialize;
                Properties.Settings.Default.Save();
            }
            else
            {
                userPreferenceSerialize = Properties.Settings.Default.FileExtension;
            }


            int indexStart = userPreferenceSerialize.IndexOf('[');
            int indexEnd = userPreferenceSerialize.LastIndexOf(']');
            int countCharEndString = userPreferenceSerialize.Length - indexEnd;

            string clean = userPreferenceSerialize.Substring(indexStart, userPreferenceSerialize.Length - (indexStart + countCharEndString) + 1);

            List<Models.ExtensionData> deserializedName = JsonConvert.DeserializeObject<List<Models.ExtensionData>>(clean);

            foreach (Models.ExtensionData item in deserializedName)
            {
                UserPreferenceList.Add(item);
            }

        }

        private string websitelToCrawler = string.Empty;
        public string WebsitelToCrawler {
            get { return websitelToCrawler; }
            set {
                this.websitelToCrawler = value;
                this.urlStatus.Ulr = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("UrlStatus");
            }
        }

        private Models.WebsiteUrlStatus urlStatus = new Models.WebsiteUrlStatus();
        public Models.WebsiteUrlStatus UrlStatus {
            get { return urlStatus; }
            set {
                this.urlStatus = value;
                NotifyPropertyChanged();
            }
        }

        public FontAwesome.WPF.FontAwesomeIcon FontAwesomeIcon = FontAwesome.WPF.FontAwesomeIcon.Play;


        public string currentUrl = "";
        public string CurrentUrl {
            get { return currentUrl; }
            set {
                this.currentUrl = value;
                NotifyPropertyChanged();
            }
        }

        private string httpValue = "http";
        public string HttpValue {
            get { return httpValue; }
            set { this.httpValue = value; }
        }

        public bool ValidExtensionFlyouts { get; set; } = false;

        #region StartCommand   

        public ButtonsCommand StartCommand {
            get;
            private set;
        }

        private void StartCommandExecute(object obj)
        {


            // HTTPS Problem 
            //https://stackoverflow.com/questions/24008681/request-url-getleftparturipartial-authority-returns-http-on-https-site

            switch (this.UrlStatus.Status)
            {
                case Models.EnumStatus.onStartup:
                    this.UrlStatus.Status = Models.EnumStatus.working;
                    StartWebCrawler();
                    break;

                case Models.EnumStatus.working:
                    this.UrlStatus.Status = Models.EnumStatus.onPause;
                    break;

                case Models.EnumStatus.onPause:
                    this.UrlStatus.Status = Models.EnumStatus.working;
                    StartCrawlerAsync();
                    break;
            }

            NotifyPropertyChanged("UrlStatus");
            //(Da chiedere cosa è)
            StartCommand.RaiseCanExecuteChange();

        }

        private void StartWebCrawler()
        {

            websitelToCrawler = httpValue + "://www." + websitelToCrawler;
            CurrentUrl = websitelToCrawler;

            Uri _uri = new Uri(websitelToCrawler);
            fullUriWebsite = _uri;
            fullUrlWebsite = _uri.GetLeftPart(UriPartial.Authority) + "/";

            UrlToDo.Add(fullUrlWebsite);

            StartCrawlerAsync();

        }

        private bool CanStartCommand(object obj)
        {
            return true;
        }

        #endregion

        #region Stop Command   

        public ButtonsCommand StopCommand {
            get;
            private set;
        }

        private void StopCommandExecute(object obj)
        {
            CurrentUrl = "";
            WebsitelToCrawler = "";
            this.urlStatus.Ulr = "";
            this.UrlStatus.Status = Models.EnumStatus.onStop;
            NotifyPropertyChanged("UrlStatus");
            //(Da chiedere cosa è)
            StopCommand.RaiseCanExecuteChange();
        }

        private bool CanStopCommand(object obj)
        {
            return true;
            //if (this.UrlStatus.Status == Models.EnumStatus.working)
            //{
            //    return true;
            //}
            //return false;
        }

        #endregion

        #region Create Sitemap Command   

        public ButtonsCommand CreateSitemapCommand {
            get;
            private set;
        }

        private void CreateSitemapExecute(object obj)
        {
            // https://stackoverflow.com/questions/1922204/open-directory-dialog

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                CreateSitemap(dialog.SelectedPath);


            }


        }

        private bool CanCreateSitemapCommand(object obj)
        {

            return true;
            //if (this.UrlStatus.Status == Models.EnumStatus.working)
            //{
            //    return true;
            //}
            //return false;
        }

        #endregion

        #region SavePreferenceCommand   

        public ButtonsCommand SavePreferenceCommand {
            get;
            private set;
        }

        private void SavePreferenceExecute(object obj)
        {

            Models.ValidExtensionData validExtensionData = new Models.ValidExtensionData(UserPreferenceList);
            string userPreferenceSerialize = JsonConvert.SerializeObject(validExtensionData);

            Properties.Settings.Default.FileExtension = userPreferenceSerialize;
            Properties.Settings.Default.Save();

            // Configure the message box to be displayed
            string messageBoxText = "User Preference Saved";
            string caption = "File Extension";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;

            // Display message box
            MessageBox.Show(messageBoxText, caption, button, icon);

        }


        private bool CanSavePreferenceCommand(object obj)
        {
            return true;
        }

        #endregion

        #region ValidExtensionCommand   

        public ButtonsCommand ValidExtensionCommand {
            get;
            private set;
        }

        private void ValidExtensionExecute(object obj)
        {
            ValidExtensionFlyouts = !ValidExtensionFlyouts;
            NotifyPropertyChanged("ValidExtensionFlyouts");
            NotifyPropertyChanged();
        }

        private bool CanValidExtensionCommand(object obj)
        {
            return true;
        }

        #endregion

        #region Crawler

        private async void StartCrawlerAsync()
        {


            while ((UrlToDo.Count != 0) && ((urlStatus.Status != Models.EnumStatus.onPause)) && ((urlStatus.Status != Models.EnumStatus.onStop)))
            {

                try
                {
                    CurrentUrl = UrlToDo[0];
                    NotifyPropertyChanged(CurrentUrl);

                    //HtmlWeb hw = new HtmlWeb();
                    HtmlResponseData replay = await GetHtmlDocument(CurrentUrl);

                    if (replay != null)
                    {
                        AnalizePage(replay);
                        UrlDone.Add(replay.AbsoluteUri);
                        NotifyPropertyChanged("UrlDone");
                    }
                    else
                    {
                        UrlError.Add(CurrentUrl);
                    }

                    UrlToDo.RemoveAt(0);
                    NotifyPropertyChanged("UrlToDo");
                }
                catch (Exception err)
                {
                    UrlError.Add(CurrentUrl);
                    NotifyPropertyChanged("UrlError");
                }
            }


            if (UrlToDo.Count == 0)
            {
                urlStatus.Status = Models.EnumStatus.finish;
                CurrentUrl = "";
            }
            else
            {
                UrlStatus.Status = Models.EnumStatus.onStartup;
            }

            NotifyPropertyChanged("UrlStatus");
        }

        private bool AnalizePage(HtmlResponseData replay)
        {

            try
            {
                HtmlNodeCollection _list = replay.document.DocumentNode.SelectNodes("//a[@href]");

                if (_list.Count == 0)
                    return false;

                foreach (HtmlNode link in _list)
                {
                    HtmlAttribute att = null;
                    try
                    {
                        att = link.Attributes["href"];
                    }
                    catch (Exception err2)
                    {
                        var dedug2 = "";
                    }

                    if (att != null)
                    {
                        //HtmlAttribute att = link.Attributes["href"];
                        string linkToAdd = IsValidLink(att.Value);
                        if (linkToAdd != null)
                        {
                            UrlToDo.Add(linkToAdd.ToLower().Trim());
                            NotifyPropertyChanged("UrlDone");
                        }
                    }

                }
                NotifyPropertyChanged();

            }
            catch (Exception err1)
            {
                var dedug1 = "";
                return false;
            }

            return true;

        }

        private string IsValidLink(string name)
        {
            try
            {
                name = name.Trim().ToLower();

                bool isCorrectUri = false;
                Uri uriTest = null;

                try
                {
                    uriTest = new Uri(name);
                    isCorrectUri = true;
                }
                catch
                {
                    isCorrectUri = false;
                }

                if (isCorrectUri == false)
                {
                    try
                    {
                        uriTest = new Uri(fullUrlWebsite + name);
                        name = fullUrlWebsite + name;
                        isCorrectUri = true;
                    }
                    catch
                    {
                    }
                }

                if ((isCorrectUri == true) && (uriTest != null))
                {
                    if (uriTest.GetLeftPart(UriPartial.Authority) != fullUriWebsite.GetLeftPart(UriPartial.Authority))
                    {
                        return null;
                    }

                    //NOT WORKING
                    bool alreadyDone = IsAlreadyInList(name, UrlDone);
                    if (alreadyDone == false)
                    {
                        bool alreadyInTodoList = IsAlreadyInList(name, UrlToDo);
                        if (alreadyInTodoList == false)
                        {
                            if (IsValidExtension(name) == true)
                            {
                                return name;
                            }
                        }
                    }
                }

            }
            catch
            {

            }

            return null;
        }

        private static bool IsValidExtension(string url)
        {

            //https://stackoverflow.com/questions/3784477/c-sharp-approach-for-saving-user-settings-in-a-wpf-application
            //https://weblogs.asp.net/pwelter34/444961
            //var t = WebCrawler.Properties.Settings.Default.FileExtension;

            string ext = System.IO.Path.GetExtension(url);


            switch (ext)
            {
                case ".jpg":
                case ".png":
                case ".js":
                case ".css":
                case ".jpeg":
                case ".gif":
                case "":
                    return false;
                default:
                    return true;
            }



        }

        private async Task<HtmlResponseData> GetHtmlDocument(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

            HtmlResponseData data = null;

            try
            {
                WebResponse myResponse = await request.GetResponseAsync();
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.OptionFixNestedTags = true;
                htmlDoc.Load(myResponse.GetResponseStream());
                data = new HtmlResponseData() { AbsoluteUri = myResponse.ResponseUri.AbsoluteUri.Trim().ToLower(), document = htmlDoc };
            }
            catch
            {
            }

            return data;



        }

        private class HtmlResponseData
        {
            public string AbsoluteUri;
            public HtmlDocument document;
        }

        private static bool IsAlreadyInList(string item, ObservableCollection<string> list)
        {

            item = item.ToLower().Trim();
            foreach (string t in list)
            {
                if (t.ToLower().Trim() == item)
                    return true;
            }
            return false;

        }

        #endregion

        #region Create XML

        private bool CreateSitemap(string path)
        {
            // https://msdn.microsoft.com/it-it/library/system.xml.xmldocument%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
            // https://stackoverflow.com/questions/11492705/how-to-create-an-xml-document-using-xmldocument/11492937#11492937

            bool result = false;

            XmlDocument doc = new XmlDocument();

            // the xml declaration 
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement urlsetElement = doc.CreateElement(string.Empty, "urlset", string.Empty);
            XmlAttribute attributeUrlsetElement = doc.CreateAttribute("xmlns");
            attributeUrlsetElement.Value = "http://www.sitemaps.org/schemas/sitemap/0.9";
            urlsetElement.Attributes.Append(attributeUrlsetElement);
            doc.AppendChild(urlsetElement);



            foreach (string item in UrlDone)
            {

                XmlElement elementURL = doc.CreateElement(string.Empty, "url", string.Empty);


                XmlElement elementLoc = doc.CreateElement(string.Empty, "loc", string.Empty);
                XmlText textLoc = doc.CreateTextNode(item);
                elementLoc.AppendChild(textLoc);

                elementURL.AppendChild(elementLoc);

                urlsetElement.AppendChild(elementURL);

            }

            doc.Save(path + "\\Sitemap.xml");


            return result;

        }

        #endregion



    }

    public partial class PieChartExample : UserControl
    {
        public PieChartExample()
        {
            PointLabel = chartPoint =>
                string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
            DataContext = this;
        }

        public Func<ChartPoint, string> PointLabel { get; set; }

        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }
    }

}
