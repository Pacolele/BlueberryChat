using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;

namespace ChatApp.Model
{
    public class NetworkManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        TcpClient? tcp;
        TcpListener listener;
        string? message;
        Stream? stream;

        public event Action<String>? SocketError;
        public event Action<String>? Sender;
        public event Action<String>? RequestSender;
        public event Action<Message>? RecMessage;
        public event Action? ConnectionClosed;
        private bool _ConnectionState = false;
        private IPEndPoint endPoint;
        private bool handshake = true;
        private bool isListening = false;
        private bool canListen= true;
        private bool isServer;
        public bool ConnectionState
        
        {
            get
            {
                return _ConnectionState;
            }
            set
            {
                _ConnectionState = value;
                OnPropertyChanged(nameof(ConnectionState));
            }
        }

        public bool IsServer { get => isServer; set => isServer = value; }

        //SERVER SIDE
        async public void startConnection(UserModel model)
        {
           
            Console.WriteLine("=====================SERVER SIDE=====================");
            endPoint = new IPEndPoint(IPAddress.Parse(model.IpAddress), model.Port);
            listener = new TcpListener(endPoint);
            try
            {
                listener.Start();
            }
            catch (SocketException err)
            {
                Console.WriteLine("Could not start server, socket Already in use: " + err);
                OnPropertyChanged("ServerAlreadyInUse");
                SocketError?.Invoke(err.Message);
                return;
            }
            Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnPropertyChanged("ServerStarted");
                        canListen = false;
                        Console.WriteLine("Chatt window closed server side, bool: ");
                    });
                });
            Console.WriteLine("Server started");

            try
            {
                tcp = await listener.AcceptTcpClientAsync();
                Console.WriteLine("tjena");
                stream = tcp.GetStream();
                listenForMessage();
                
            }
            catch(SocketException err)
            {
                Console.WriteLine("Window closed ): " + err);
            }
        }

        //CLIENT SIDE
        public async void joinConnection(UserModel model)
        {
               endPoint = new IPEndPoint(IPAddress.Parse(model.IpAddress), model.Port);
            try
            {
                tcp = new TcpClient();
                tcp.Connect(endPoint);
                stream = tcp.GetStream();
                OnPropertyChanged("SendingJoinRequest");
                listenForMessage();
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to connect. Retrying in 0.5 seconds...");
                OnPropertyChanged("ConnectionError");
                await Task.Delay(500); // Wait for 0.5 seconds before the next attempt
                }
        }

       

        async Task listenForMessage()
        {
            isListening = true;
            try
            {
                while (isListening)
                {
                    if (stream == null)
                    {
                        return;
                    }

                    var buffer = new byte[1024];
                    var received = stream!.Read(buffer, 0, 1024);
                    var message = Encoding.UTF8.GetString(buffer, 0, received);

                    if (message == null)
                    {
                        return;
                    }

                    if (received > 0)
                    {
                        string jsonString = Encoding.UTF8.GetString(buffer, 0, received);

                        // Deserialize the JSON string to a Messages object
                        Message? receivedMessage = JsonConvert.DeserializeObject<Message>(jsonString);
                        if (receivedMessage != null)
                        {                            
                            // Process the received message as needed
                            switch (receivedMessage.RequestType)
                            {
                                case "joinRequest":
                                    RequestSender?.Invoke(receivedMessage.ServerName);

                                    OnPropertyChanged("ReceivedJoinRequest");
                                    break;

                                case "sendMesssage": 
                                    RecMessage?.Invoke(receivedMessage);
                                    OnPropertyChanged("ReceivedMessage");
                                    break;

                                case "acceptJoinRequest": //Client har fått svar
                                    ConnectionState = true;
                                    
                                    Task.Run(() =>
                                    {
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            OnPropertyChanged("OpenChattWindow");
                                            
                                        });
                                    });
                                       

                                    break;

                                case "declineJoinRequest":
                                    OnPropertyChanged("RequestDeclined");

                                    break;
                                case "connectionEstablished":
                                    if(handshake)
                                    {
                                        handshake = false;

                                        sendMessage(receivedMessage);
                                    }
                                    else
                                    {
                                        await Application.Current.Dispatcher.InvokeAsync(() =>
                                        {
                                            Sender?.Invoke(receivedMessage.ServerName);
                                        });
                                        handshake = true;
                                        break;
                                    }
                                    break;

                                case "closingConnection":
                                    Console.WriteLine("Closing Connection......");

                                    FriendClosingConnection();
                                    break;
                                case "ShakeScreen":
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        OnPropertyChanged("ShakeScreen");
                                    });

                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IOException while reading from the stream: {ex.Message}");
            }
        }
        public bool getConnection()
        {
            return ConnectionState;
        }

        public async Task sendMessage(Message message)
        {
            try
            {
                // Serialize the Messages object to a JSON string
                string jsonString = JsonConvert.SerializeObject(message);

                // Convert the JSON string to bytes
                var buffer = Encoding.UTF8.GetBytes(jsonString);

                // Write the bytes to the stream
                await stream!.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception)
            {
                Console.WriteLine($"Error sending message");
            }
        }
        public async void CloseConnection()
        {
            try
            {
                if(ConnectionState)
                {
                    Message message = new Message
                    {
                        RequestType = "closingConnection"
                    };

                   await sendMessage(message);
                }
                
                isListening = false;
                listener?.Server.Close();
                listener?.Stop();
                stream?.Flush();
                stream?.Close();
                tcp?.Close();
             
                OnConnectionClosed();
            }
            catch (Exception)
            {
                Console.WriteLine($"Error closing connection");
            }
        }

        public void FriendClosingConnection()
        {
            try
            {
                ConnectionState = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged("ConnectionClosed");
                });

                if (IsServer)
                {
                    Task.Run(() => RestartConnection());
                }
                
            }
            catch (Exception)
            {
                Console.WriteLine($"Error closing connection");
            }
        }

        private void OnConnectionClosed()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged("ClosingChatt");
            });
        }

        public void RestartConnection()
        {
            isListening = false;
            listener?.Stop();
            stream?.Flush();
            stream?.Close();
            tcp?.Close();
            try
            {
                Console.WriteLine("===================== Restaring the server =====================");
                listener = new TcpListener(endPoint);
                listener.Start();

                Console.WriteLine("Server started");
                
                    tcp = listener.AcceptTcpClient();
                    stream = tcp.GetStream();

                    listenForMessage();

            }
            catch (SocketException err)
            {
                Console.WriteLine("Window closed ): " + err);
               
            }
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
}