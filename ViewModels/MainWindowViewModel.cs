using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebCrawler.ViewModels
{

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

        private string fullUrlWebsite;
        private Uri fullUriWebsite;

        public MainWindowViewModel()
        {
            UrlDone = new ObservableCollection<string>();
            UrlToDo = new ObservableCollection<string>();
            UrlError = new ObservableCollection<string>();
            this.StartCommand = new ButtonsCommand(StartCommandExecute, CanStartCommand);
            this.StopCommand = new ButtonsCommand(StopCommandExecute, CanStopCommand);
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

        #region StartCommand   

        public ButtonsCommand StartCommand {
            get;
            private set;
        }

        private void StartCommandExecute(object obj)
        {
            //// Add "http://" if is missing
            //if (websitelToCrawler.IndexOf("http://") == -1)
            //{
            //    if (websitelToCrawler.IndexOf("https://") == -1)
            //    {
            //        websitelToCrawler = "http://" + websitelToCrawler;
            //    }
            //}

            websitelToCrawler = httpValue + "://www." + websitelToCrawler;

            CurrentUrl = websitelToCrawler;
            this.UrlStatus.Status = Models.EnumStatus.working;

            Uri _uri = new Uri(websitelToCrawler);
            fullUriWebsite = _uri;
            fullUrlWebsite = _uri.GetLeftPart(UriPartial.Authority) + "/";

            UrlToDo.Add(fullUrlWebsite);

            StartCrawlerAsync();
            NotifyPropertyChanged("UrlStatus");
            StartCommand.RaiseCanExecuteChange();
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
            this.UrlStatus.Status = Models.EnumStatus.notWorking;
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

        #region Crawler

        private async void StartCrawlerAsync()
        {

            while ((UrlToDo.Count != 0) && (urlStatus.Status != Models.EnumStatus.notWorking))
            {

                try
                {
                    CurrentUrl = UrlToDo[0];
                    NotifyPropertyChanged(CurrentUrl);

                    HtmlWeb hw = new HtmlWeb();
                    HtmlResponseData replay = await GetHtmlDocument(CurrentUrl);

                    UrlDone.Add(replay.AbsoluteUri);
                    UrlToDo.RemoveAt(0);

                    NotifyPropertyChanged("UrlToDo");

                    foreach (HtmlNode link in replay.document.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        HtmlAttribute att = link.Attributes["href"];
                        string linkToAdd = IsValidLink(att.Value);
                        if (linkToAdd != null)
                        {
                            UrlToDo.Add(linkToAdd.ToLower().Trim());
                            NotifyPropertyChanged("UrlDone");
                        }
                    }
                    NotifyPropertyChanged();
                }
                catch (Exception err)
                {
                    UrlError.Add(CurrentUrl);
                    NotifyPropertyChanged("UrlError");
                }

            }

            UrlStatus.Status = Models.EnumStatus.notWorking;

        }

        private string IsValidLink(string name)
        {
            try
            {
                name = name.Trim();

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
                    bool alreadyDone = IsAlreadyInList(name,UrlDone);
                    if (alreadyDone == false)
                    {
                        bool alreadyInTodoList = IsAlreadyInList(name,UrlToDo);
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
            catch(Exception err)
            {

            }

            return null;
        }

        private static bool IsValidExtension(string url)
        {

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

        private static bool IsAlreadyInList(string item, ObservableCollection<string> list) {

            item = item.ToLower().Trim();
            foreach (string t in list) {
                if (t.ToLower().Trim() == item)
                    return true;
            }
            return false;

        }

        #endregion
    }
}
