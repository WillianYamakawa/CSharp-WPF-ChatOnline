using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
	internal class Program
	{
		static TcpListener _listener;
		static List<Client> _users;
		static void Main(string[] args)
		{
			_users = new List<Client>();
			_listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
			_listener.Start();

			while (true)
			{
				var client = new Client(_listener.AcceptTcpClient());
				_users.Add(client);

				/* Broadcast to everyone */
				BroadcastConnection();
			}
		}

		static void BroadcastConnection()
		{
			foreach (var user in _users)
			{
				foreach(var usr in _users)
				{
					var BroadcastPacket = new PacketBuilder();
					BroadcastPacket.WriteOpCode(1);
					BroadcastPacket.WriteMessage(usr.Username);
					BroadcastPacket.WriteMessage(usr.UID.ToString());
					user.ClientSocket.Client.Send(BroadcastPacket.GetPacketBytes());
				}
			}
		}

		public static void BroadcastDisconnect(string UID)
		{
			var disconnectedUser = _users.Where(x => x.UID.ToString() == UID).FirstOrDefault();
			_users.Remove(disconnectedUser);
			foreach (var user in _users)
			{
				var BroadcastPacket = new PacketBuilder();
				BroadcastPacket.WriteOpCode(10);
				BroadcastPacket.WriteMessage(UID);
				user.ClientSocket.Client.Send(BroadcastPacket.GetPacketBytes());
			}

			BroadcastMessage($"[{disconnectedUser.Username}] Disconnected!");
		}

		public static void BroadcastMessage(string message)
		{
			foreach (var user in _users)
			{
				var msgPacket = new PacketBuilder();
				msgPacket.WriteOpCode(5);
				msgPacket.WriteMessage(message);
				user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
			}
		}

	}
}
