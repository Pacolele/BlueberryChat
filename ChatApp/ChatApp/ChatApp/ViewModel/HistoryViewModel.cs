using ChatApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChatApp.View;
using ChatApp.ViewModel.Commands;
using System.Windows;
using System.Net.Sockets;
using System.Diagnostics.Eventing.Reader;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace ChatApp.ViewModel
{
    public class HistoryViewModel : INotifyPropertyChanged
    {

        private ChattViewModel _chattViewModel;
        private ChatHistory _chatt;
        private ICommand? _openHistory;
        private ICommand? _closeHistory;
        private String userName;
        private String friend;
        private ObservableCollection<Message> allMessages;

        public HistoryViewModel(ChatHistory chatt, ChattViewModel chattViewModel)
        {
            _chattViewModel = chattViewModel;
            _chatt = chatt;
            OpenHistory = new openHistory(_chattViewModel, this);
            UserName = _chatt.Sender;
            Friend = _chatt.Receiver;
            AllMessages = new ObservableCollection<Message>(_chatt.chatHistory);

        }

        public string Display
        {
            get { return _chatt.Sender + " " + _chatt.Receiver + " " +  _chatt.Date; }
            set { _chatt.Sender = value;}
        }

        public ICommand OpenHistory { get => _openHistory; set => _openHistory = value; }

        public ICommand CloseButtonCommand
        {
            get
            {
                if(_closeHistory == null)
                {
                    _closeHistory = new closeHistory(this);
                }
                return _closeHistory;
            }
            set
            {
                _closeHistory = value;
            }
        }

       public void CloseHistory()
        {
            Console.WriteLine("prep for close of history");
            getWindow().Close();
        }

        private Window getWindow()
        {
            
            var historyWindow = Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w is History && w.Title == "History");

            return historyWindow;
        }
        public string UserName { get => userName; set => userName = value; }
        public string Friend { get => friend; set => friend = value; }
        public ObservableCollection<Message> AllMessages { get => allMessages; set => allMessages = value; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}