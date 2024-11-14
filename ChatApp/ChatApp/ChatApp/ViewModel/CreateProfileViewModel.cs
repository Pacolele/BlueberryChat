using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChatApp.Model;
using ChatApp.View;
using ChatApp.ViewModel.Commands;
using System.Windows;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using ChatApp.Model.Interface;

namespace ChatApp.ViewModel
{
    public class CreateProfileViewModel : INotifyPropertyChanged, IChattTopBar
    {
        private NetworkManager? NetworkManager { get; set; }
        private ICommand? startServer;
        private ICommand joinServer;
        private ICommand? _closeButtonCommand;
        private ICommand? _minimizeButtonCommand;
        private ICommand? _maximizeButtonCommand;
        private UserModel? _model;
        private string _errorMessage;
        private Visibility _errorVisibility;

        public CreateProfileViewModel(NetworkManager networkManager)
        {
            NetworkManager = networkManager;
            _model = new UserModel();
            UserName = "default";
            Port = 8080;
            IpAddress = "127.0.0.1";

            ErrorVisibility = Visibility.Collapsed;

            NetworkManager.PropertyChanged += NetworkMangerHandler;
        }

        #region DataBindings
        public string UserName
        {
            get { return _model?.UserName; }
            set
            {
                if (_model?.UserName != value)
                {
                    _model!.UserName = value;
                    OnPropertyChanged(UserName);
                }
            }
        }

        public int Port
        {
            get { return _model!.Port; }
            set
            {
                if (_model != null)
                {
                    _model.Port = value;
                    OnPropertyChanged(nameof(Port));
                }
            }
        }

        public string IpAddress
        {
            get { return _model!.IpAddress; }
            set
            {
                if (_model != null)
                {
                    _model.IpAddress = value;
                    OnPropertyChanged(nameof(IpAddress));
                }
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        public Visibility ErrorVisibility
        {
            get
            {
                return _errorVisibility;
            }
            set
            {
                _errorVisibility = value;
                OnPropertyChanged(nameof(ErrorVisibility));
            }
        }

        public ICommand StartServer { 
            get
            {
                if(startServer == null)
                    startServer = new StartChatt(this);
                return startServer;
            } 
            set
            {
                startServer = value;
            }
                
        }
        public ICommand? JoinServer
        {
            get
            {
                if (joinServer == null)
                    joinServer = new JoinChat(this);
                return joinServer;
            }
            set
            {
                joinServer = value;
            }
        }

        public ICommand CloseButtonCommand
        {
            get
            {
                if (_closeButtonCommand == null)
                    _closeButtonCommand = new chattTopButton<CreateProfileViewModel>(this, "close");
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
                    _minimizeButtonCommand = new chattTopButton<CreateProfileViewModel>(this, "minimize");
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
                    _maximizeButtonCommand = new chattTopButton<CreateProfileViewModel>(this, "maximize");
                return _maximizeButtonCommand;
            }
            set
            {
                _maximizeButtonCommand = value;
            }
        }
        public Chatt Chatt { get; private set; }
        #endregion

        #region NetworkFunctions

        private void NetworkMangerHandler(object? sender, PropertyChangedEventArgs e)
        {


            if (e.PropertyName == "ServerAlreadyInUse")
            {
                ErrorMessage = "You cannot start a server that is already in use";
                ErrorVisibility = Visibility.Visible;
            }
            else if (e.PropertyName == "ServerStarted")
            {
                showChatt(true);
            }
            else if (e.PropertyName == "SendingJoinRequest")
            {
                Message message = new Message
                {
                    RequestType = "joinRequest",
                    ServerName = UserName!
                };
                NetworkManager!.sendMessage(message);
            }
            else if (e.PropertyName == "OpenChattWindow")
            {
                showChatt(false);
            }

            else if (e.PropertyName == "ConnectionError")
            {
                ErrorMessage = "No available server to connect to";
                ErrorVisibility = Visibility.Visible;
            }

            else if (e.PropertyName == "NoServerToConnectTo")
            {
                ErrorMessage = "You cannot request to join a server that is not in use";
                ErrorVisibility = Visibility.Visible;
            }
            else if (e.PropertyName == "RequestDeclined")
            {
                ErrorMessage = "Your request to join was declined";
                ErrorVisibility = Visibility.Visible;
            }
        }

        public void startConnection()
        {
            ErrorVisibility = Visibility.Collapsed;
            Task.Run(() =>
            {

                try
                {
                    if (_model != null)
                    {
                        NetworkManager?.startConnection(_model);
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions appropriately
                    Console.WriteLine($"Error in startConnection: {ex}");
                }
            });
        }

        public void joinConnection()
        {   
            ErrorVisibility = Visibility.Collapsed;
            
            Task.Run(() =>
            {   
                try
                {
                    if (_model != null)
                    {
                        NetworkManager?.joinConnection(_model);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in startConnection: {ex}");
                }
            });
             
        }

        #endregion

        #region ChattWindowHandling

        private void showChatt(bool isServer)
        {
            Console.WriteLine("USER STARTING: " + UserName);

            Chatt chatt = new Chatt(NetworkManager, _model, isServer);

            chatt.Closing += ChattWindowClosing!;
            
            chatt.ShowDialog();
        }

        private void ChattWindowClosing(object sender, CancelEventArgs e)
        {

            NetworkManager?.CloseConnection();
        }

        public void ChattWindowHandler(string action)
        {

            Window window = getWindow();
            if (action == "close")
            {
                Application.Current.Shutdown();
            }
            else if (action == "minimize")
            {
              window.WindowState = WindowState.Minimized;
            }
            else if (action == "maximize")
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                }
                else
                {
                    window.WindowState = WindowState.Maximized;
                }
            }

            
        }

        private Window getWindow()
        {
            var window = Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w is CreateProfile && w.Title == "CreateProfile");

            return window!;
        }

        #endregion


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}