using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;


namespace ChatClient.MVVM.ViewModel
{
	class MainViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<UserModel> Users { get; set; }
		public ObservableCollection<MessageModel> Messages { get; set; }
		public RelayCommand SendMessageCommand { get; set; }
		public RelayCommand ConnectToServerCommand { get; set; }
		public string Username { get; set; }

		private string _message;
		public string Message { get { return _message; } set { _message = value; OnPropertyChanged("Message"); } }

		public Random random;

		private Server _server;

		public event PropertyChangedEventHandler? PropertyChanged;

		public MainViewModel()
		{
			random = new Random();

			Users = new ObservableCollection<UserModel>();

			Messages = new ObservableCollection<MessageModel>();

			_server = new Server();

			_server.connectedEvent += UserConnected;
			_server.messageReceivedEvent += MessageReceived;
			_server.userDisconnectEvent += RemoveUser;

			ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username), o => !string.IsNullOrEmpty(Username) && !_server.isConnected());
			SendMessageCommand = new RelayCommand(o => _server.SendMessageToServer(Message, this), o => !string.IsNullOrEmpty(Message) && _server.isConnected());

		}


		private void RemoveUser()
		{
			var uid = _server.PacketReader.ReadMessage();
			var user = Users.Where(x => x.UID == uid).FirstOrDefault();
			Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
		}

		private void MessageReceived()
		{
			var msg = _server.PacketReader.ReadMessage();
			Application.Current.Dispatcher.Invoke(() => Messages.Add(new MessageModel(msg)));
		}

		private void UserConnected()
		{
			string usrnm = _server.PacketReader.ReadMessage();
			var user = new UserModel
			{
				Username = usrnm.Length < 18 ? usrnm : usrnm.Substring(0, 17) + "...",
				UID = _server.PacketReader.ReadMessage()
			};

			if (!Users.Any(x => x.UID == user.UID))
			{
				Application.Current.Dispatcher.Invoke(() => Users.Add(user));
			}
		}

		private void OnPropertyChanged(string sender)
		{
			if (PropertyChanged == null) return;
			PropertyChanged(this, new PropertyChangedEventArgs(sender));
		}
	}
}
