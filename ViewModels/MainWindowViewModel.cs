﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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


        //public Models.Person EditModePersonSelected {
        //    get { return editModePersonSelected; }

        //    set {
        //        if (editModePersonSelected == value)
        //            return;
        //        editModePersonSelected = value;
        //        NotifyPropertyChanged();

        //        //SaveCommand.RaiseCanCanExecuteChange();
        //    }

        //}




        #region StartCommand   

        public ButtonsCommand StartCommand {
            get;
            private set;
        }

        private void StartCommandExecute(object obj)
        {
            CurrentUrl = websitelToCrawler;
            this.UrlStatus.Status = Models.EnumStatus.working;

            Uri _uri = new Uri(websitelToCrawler);
            fullUriWebsite = _uri;
            fullUrlWebsite = _uri.GetLeftPart(UriPartial.Authority) + "/";

            UrlToDo.Add(fullUrlWebsite);

            StartCrawlerAsync();
            NotifyPropertyChanged("UrlStatus");
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
        }

        #endregion



        private async void StartCrawlerAsync()
        {

            while ((UrlToDo.Count != 0)&&(urlStatus.Status!=Models.EnumStatus.notWorking))
            {

                try
                {
                    CurrentUrl = UrlToDo[0];
                    NotifyPropertyChanged(CurrentUrl);

                    HtmlWeb hw = new HtmlWeb();
                    //HtmlDocument doc = hw.Load(CurrentUrl);
                    
                    HtmlDocument doc = await GetHtmlDocument(CurrentUrl);

                    UrlDone.Add(UrlToDo[0]);
                    UrlToDo.RemoveAt(0);
                    NotifyPropertyChanged("UrlToDo");

                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        HtmlAttribute att = link.Attributes["href"];
                        string linkToAdd = IsValidLink(att.Value);
                        if (linkToAdd != null)
                        {
                            UrlToDo.Add(linkToAdd);
                            NotifyPropertyChanged("UrlDone");
                        }
                    }
                    NotifyPropertyChanged();
                }
                catch
                {
                    UrlError.Add(CurrentUrl);
                    NotifyPropertyChanged("CurrentUrl");
                }


            }

        }


        private string IsValidLink(string name)
        {

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

                bool done = false;

                // Check the url is alredy done
                foreach (string t in UrlDone)
                {
                    if (name == t)
                    {
                        done = true;
                    }
                }

                if (done == false)
                {
                    bool alreadyInList = false;

                    // Check if the url is in the todo list
                    foreach (string t in UrlToDo)
                    {
                        if (name == t)
                        {
                            alreadyInList = true;
                        }
                    }

                    if (alreadyInList == false)
                    {
                        return name;
                    }
                }
            }

            return null;
        }


        private async Task<HtmlDocument> GetHtmlDocument(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

            HtmlDocument htmlDoc = null;

            try
            {
                WebResponse myResponse = await request.GetResponseAsync();
                htmlDoc = new HtmlDocument();
                htmlDoc.OptionFixNestedTags = true;
                htmlDoc.Load(myResponse.GetResponseStream());
            }
            catch (Exception err)
            {


            }

            return htmlDoc;



        }
    }
}
