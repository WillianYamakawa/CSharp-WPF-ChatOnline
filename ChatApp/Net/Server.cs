using ChatClient.MVVM.ViewModel;
using ChatClient.Net.IO;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatClient.Net
{
	class Server
	{
		TcpClient _client;
		public PacketReader PacketReader;

		public event Action connectedEvent;
		public event Action messageReceivedEvent;
		public event Action userDisconnectEvent;

		public Server()
		{
			_client = new TcpClient();
		}

		public void ConnectToServer(string username)
		{
			try
			{
				if (!_client.Connected)
				{
					_client.Connect("127.0.0.1", 7891);
					PacketReader = new PacketReader(_client.GetStream());

					if (!string.IsNullOrEmpty(username))
					{
						var connectPacket = new PacketBuilder();
						connectPacket.WriteOpCode(0);
						connectPacket.WriteMessage(username);
						_client.Client.Send(connectPacket.GetPacketBytes());
					}
					ReadPackets();

				}
			}
			catch (Exception ex)
			{

			}
			
		}

		public bool isConnected()
		{
			return _client.Connected;
		}

		private void ReadPackets()
		{
			Task.Run(() =>
			{
				while (true)
				{
					var opcode = PacketReader.ReadByte();
					switch (opcode)
					{
						case 1:
							connectedEvent?.Invoke();
							break;

						case 5:
							messageReceivedEvent?.Invoke();
							break;

						case 10:
							userDisconnectEvent?.Invoke();
							break;

						default:
							Console.WriteLine("opcode not implemented");
							break;

					}
				}

			});
		}

		public void SendMessageToServer(string message, MainViewModel m)
		{
			m.Message = "";
			var messagePacket = new PacketBuilder();
			messagePacket.WriteOpCode(5);
			messagePacket.WriteMessage(message);
			_client.Client.Send(messagePacket.GetPacketBytes());
		}

	}
}
