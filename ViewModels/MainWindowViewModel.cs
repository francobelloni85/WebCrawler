using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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


        public MainWindowViewModel()
        {
            UrlDone = new ObservableCollection<string>();
            UrlToDo = new ObservableCollection<string>();
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
            //string data = Models.HtmlHelper.GetHtmlCode(websitelToCrawler);
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




    }

}
