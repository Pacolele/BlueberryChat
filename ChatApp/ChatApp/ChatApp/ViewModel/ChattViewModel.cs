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
using ChatApp.Model.Interface;

namespace ChatApp.ViewModel
{
    public class ChattViewModel : INotifyPropertyChanged, IChattTopBar
    {
        private string _enteredText = string.Empty;
        private UserModel? _user;
        List<Message> allMessages = new List<Message>();
        private String? userName;
        private String? _searchtext;
        private String? _connectionStatus;

        private ICommand? _acceptRequest;
        private ICommand? _declineRequest;
        private ICommand? _sendMessage;
        private ICommand? _shakeScreen;
        private ICommand? _searchHistory;
        private ICommand? _closeButtonCommand;
        private ICommand? _minimizeButtonCommand;
        private ICommand? _maximizeButtonCommand;

        private String? _messageAlignment;
        private string? _requestSender;
        private bool? _buttonState = false;
        private bool _isServer;
        private string? friendToSave;
        private Visibility _requestVisibility = Visibility.Collapsed;
        private ObservableCollection<Message> _allMessages = new ObservableCollection<Message>();
        private ObservableCollection<HistoryViewModel> _history = new ObservableCollection<HistoryViewModel>();
        public ChattViewModel(NetworkManager networkManager, UserModel user, bool isServer)
        {
            NetworkManager = networkManager;
            NetworkManager.IsServer = isServer;
            _isServer = isServer;
            _user = user;
            userName = user.UserName;
            ConnectionStatus = "No connection";
            _requestVisibility = Visibility.Collapsed;

            List<ChatHistory> loadedChatHistories = ChatHistory.LoadChatHistories(isServer);

            foreach (var chatHistory in loadedChatHistories)
            {
               printHistory(chatHistory);
            }

            allMessages.Clear();
            AllMessages.Clear();
   
            ButtonState = NetworkManager.getConnection();
            NetworkManager.PropertyChanged += NetworkMangerHandler;
            NetworkManager.Sender += SenderHandler;
            NetworkManager.RequestSender += RequestHandler;
            NetworkManager.RecMessage += ReceivedMessageHandler;
        }

        #region DataBindings
        NetworkManager NetworkManager { get; set; }

        public UserModel? user
        {
            get
            {
                return _user;
            }
            set
            {
                _user = value;
                OnPropertyChanged(nameof(user));
            }
        }

        public string? MessageAlignment
        {
            get { return _messageAlignment; }
            set
            {
                _messageAlignment = value;
                OnPropertyChanged(nameof(MessageAlignment));
            }
        }

        public string? SearchText
        {
            get
            {
                return _searchtext; 
            }
            set
            {
                _searchtext = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public string? ConnectionStatus
        {
            get
            {
                return _connectionStatus;
            }
            set
            {
                _connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        public string? RequestSender
        {
            get
            {
                return _requestSender; 
            }
            set
            {
                _requestSender = value;
                OnPropertyChanged(nameof(RequestSender));
            }
        }

        public Visibility requestVisibility
        {
            get { return _requestVisibility; }
            set
            {
                _requestVisibility = value;
                OnPropertyChanged(nameof(requestVisibility));
            }
        }
        public ObservableCollection<Message> AllMessages
        {
            get { return _allMessages; }
            set
            {
                _allMessages = value;
                OnPropertyChanged(nameof(AllMessages));
            }
        }

        public ObservableCollection<HistoryViewModel> ChatHistories
        {
            get { return _history; }
            set
            {
                _history = value;
                OnPropertyChanged(nameof(ChatHistories));
            }
        }

        public string? UserName
        {
            get { return userName; }
            set
            {
                if (user != null)
                {
                    userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public string? Friend
        {
            get { return user?.Friend; }
            set
            {
                if (user != null)
                {
                    user.Friend = value!;
                    OnPropertyChanged(nameof(Friend));

                }
            }
        }
        public bool? ButtonState
        {
            get
            {
                return _buttonState;
            }
            set
            {
                _buttonState = value;
                OnPropertyChanged(nameof(ButtonState));
            }
        }

        public string? EnteredText
        {
            get { return _enteredText; }
            set
            {
                if (user != null)
                {
                    _enteredText = value!;
                    OnPropertyChanged(nameof(EnteredText));
                }
            }
        }

        public ICommand acceptRequest
        {
            get
            {
                if (_acceptRequest == null)
                    _acceptRequest = new acceptRequest(this);
                return _acceptRequest;
            }
            set
            {
                _acceptRequest = value;
            }
        }

        public ICommand declineRequest
        {
            get
            {
                if (_declineRequest == null)
                    _declineRequest = new declineRequest(this);
                return _declineRequest;
            }
            set
            {
                _declineRequest = value;
            }
        }

        public ICommand SendMessage
        {
            get
            {
                if (_sendMessage == null)
                    _sendMessage = new SendMessages(this);
                return _sendMessage;
            }
            set
            {
                _sendMessage = value;
            }
        }

        public ICommand ShakeScreen
        {
            get
            {
                if (_shakeScreen == null)
                    _shakeScreen = new shakeScreen(this);
                return _shakeScreen;
            }
            set
            {
                _shakeScreen = value;
            }
        }

        public ICommand SearchHistory
        {
            get
            {
                if (_searchHistory == null)
                    _searchHistory = new searchHistory(this);
                return _searchHistory;
            }
            set
            {
                _searchHistory = value;
            }
        }

        public ICommand CloseButtonCommand
        {
            get
            {
                if (_closeButtonCommand == null)
                    _closeButtonCommand = new chattTopButton<ChattViewModel>(this, "close");
                return _closeButtonCommand;
            }
            set
            {
                _closeButtonCommand = value;
            }
        }

        public ICommand MinimizeButtonCommand
        {
            get
            {
                if (_minimizeButtonCommand == null)
                    _minimizeButtonCommand = new chattTopButton<ChattViewModel>(this, "minimize");
                return _minimizeButtonCommand;
            }
            set
            {
                _minimizeButtonCommand = value;
            }
        }

        public ICommand MaximizeButtonCommand
        {
            get
            {
                if (_maximizeButtonCommand == null)
                    _maximizeButtonCommand = new chattTopButton<ChattViewModel>(this, "maximize");
                return _maximizeButtonCommand;
            }
            set
            {
                _maximizeButtonCommand = value;
            }
        }

        #endregion

        #region DataHandlers
        public async Task handleRequest(bool answer)
        {
            if (answer)
            {
                Message message = new Message
                {
                    RequestType = "acceptJoinRequest",
                    ServerName = userName!,
                    ClientName = RequestSender
                };
                await NetworkManager.sendMessage(message);

                Message message2 = new Message
                {
                    RequestType = "connectionEstablished",
                    ServerName = userName!,
                    ClientName = RequestSender
                };
                await NetworkManager.sendMessage(message2);
                Friend = RequestSender;
                friendToSave = RequestSender;
                ConnectionStatus = "Connected";
                NetworkManager.ConnectionState = true;
                ButtonState = true; // server kam skicka medelanden
            }
            else
            {
                Message message = new Message
                {
                    RequestType = "declineJoinRequest",
                    ServerName = userName!
                };
                await NetworkManager.sendMessage(message);
                Console.WriteLine("Declined request restarting");

                Task.Run(() => {
                    NetworkManager.RestartConnection();
                });
            }
            HideRequest();
        }

        public void handleSendMessage()
        {

            if (EnteredText != null)
            {
                if (EnteredText.Length > 0)
                {
                    Message message = new Message
                    {
                        RequestType = "sendMesssage",
                        MessageContent = EnteredText!,
                        ServerName = userName!,
                        ClientName = Friend,
                        IsReceiver = false,
                        SenderColor = "#1D2570"
                    };

                    Message historyMessage = new Message
                    {
                        RequestType = "sendMesssage",
                        MessageContent = EnteredText!,
                        ServerName = userName!,
                        ClientName = Friend,
                        IsReceiver = false,
                        SenderColor = "#1D2570"
                    };

                    EnteredText = "";
                    allMessages.Add(historyMessage);
                    MessageAlignment = "Left";
                    NetworkManager.sendMessage(message);
                    printMessage(message); // Your own message
                    message.ServerName = "You";
                    
                }
            }
        }
        private async void NetworkMangerHandler(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ReceivedJoinRequest")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowRequest();
                });
            }
            else if (e.PropertyName == "ClosingChatt")
            {
                SaveChatHistory();
                ConnectionStatus = "No connection";
                Friend = null;
            }
            else if(e.PropertyName == "Connected")
            {
                Console.WriteLine("Client preparing to send message to server...");
                Message message = new Message
                {
                    RequestType = "connectionEstablished"
                };
                await NetworkManager.sendMessage(message);
            }

            else if (e.PropertyName == "ShakeScreen")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var allWindows = Application.Current.Windows.OfType<Window>().ToList();
                    if (allWindows.Count > 1)
                    {
                        var secondWindow = allWindows[1];
                        if (secondWindow != null)
                        { 
                            ShakeTheScreen(secondWindow);
                        }
                    }
                });
            }
            else if(e.PropertyName == "ConnectionClosed") {
                ButtonState = NetworkManager.getConnection();
                ConnectionStatus = "No Connection";
                Friend = null;

            }
        }

        private void SenderHandler(string sender)
        {
            Friend = sender;
            friendToSave = sender;
            ConnectionStatus = "Connected";
        }

        private void RequestHandler(string sender)
        {
            RequestSender = sender;
        }

        private void ReceivedMessageHandler(Message message)
        {
            if (message.RequestType == "sendMesssage")
            {
                MessageAlignment = "Right";
                message.SenderColor = "#5AB0C7";
                allMessages.Add(message);
                printMessage(message);
            }
        }

        public void ChattWindowHandler(string action)
        {
    
            Window chatt = getChattWindow();
            if (action == "close")
            {
                chatt.Close();
            }
            else if (action == "minimize")
            {
                chatt.WindowState = WindowState.Minimized;
            }
            else if (action == "maximize")
            {
                if (chatt.WindowState == WindowState.Maximized)
                {
                    chatt.WindowState = WindowState.Normal;
                }
                else
                {
                    chatt.WindowState = WindowState.Maximized;
                }
            }
        }

        #endregion

        #region HistoryHandling
        private void SaveChatHistory()
        {
            // Create a new ChatHistory instance
            // Friend != null && 
            if (allMessages.Count > 0) {
                Console.WriteLine("Username to save: " + userName + "items in list " + allMessages.Count);
            
                ChatHistory chatHistory = new ChatHistory
                {
                Sender = userName,
                Receiver = friendToSave,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                chatHistory = new List<Message>(allMessages)
                };
                chatHistory.SaveChatHistory(_isServer);
                allMessages.Clear();
            }
        }

        public void SearchChatHistory(string query)
        {
            query = query.ToLower();

            var result = ChatHistory.LoadChatHistories(_isServer)
                .Where(chatHistory =>
                    chatHistory.Sender.ToLower().Contains(query) ||
                    chatHistory.Date.Contains(query) ||
                    chatHistory.Sender.ToLower().IndexOf(query) != -1 ||
                    chatHistory.Date.IndexOf(query) != -1)
                .ToList();

            if (result.Count > 0)
            {
                ChatHistories.Clear();

                foreach (var chatHistory in result)
                {
                    Console.WriteLine("FOUND:   " + chatHistory.Sender);
                    ChatHistories.Add(new HistoryViewModel(chatHistory,this));
                }
            }
            else
            {
                ChatHistories.Clear();

            }
        }

        #endregion

        #region Prints
        public void printHistory(ChatHistory history)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatHistories.Add(new HistoryViewModel(history, this)); //for the print window
            });
        }

        public void printMessage(Message message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AllMessages.Add(message); //for the print window
            });
        }

        #endregion

        #region WindowHandling
        public void ShakeTheScreen(Window thisWindow)
        {
          
            var storyboard = new Storyboard();

            var animation = new DoubleAnimation
            {
                From = thisWindow.Left,
                To = thisWindow.Left + 30, 
                
                Duration = TimeSpan.FromMilliseconds(100),
                RepeatBehavior = new RepeatBehavior(4),
                AutoReverse = true
            };

            Storyboard.SetTarget(animation, thisWindow);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Window.Left)"));

            storyboard.Children.Add(animation);
            storyboard.Begin();   
        }

        public void handleShakeScreen()
        {
            Console.WriteLine("SENDING TO : " + Friend);
            Message message = new Message
            {
                RequestType = "ShakeScreen"
            };
            NetworkManager.sendMessage(message);
        }

        private Window getChattWindow()
        {
            var chattWindow = Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w is Chatt && w.Title == "ChattWindow");

            return chattWindow;
        }

        public void OpenHistoryWindow(HistoryViewModel hvm)
        {
            if (_user != null)
            {
                History historyWindow = new History(hvm);

                historyWindow.Show();
            }
        }

        private void ShowRequest()
        {
            AllMessages.Clear();
            requestVisibility = Visibility.Visible;
        }

        private void HideRequest()
        {
            requestVisibility = Visibility.Collapsed;
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}