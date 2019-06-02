using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using TDHPlugin.Networking.NetworkMessages;

namespace TDHPlugin.Networking
{
	public class ClientController
	{
		public Socket Socket { get; private set; }

		public NetworkStream networkStream;
		public StreamReader reader;

		[NotNull] private readonly byte[] intBuffer = new byte[4];

		[NotNull] public IClientControllerListener requestListener;

		private Thread clientThread;

		private int networkId = int.MinValue;

		public bool IsConnected
		{
			get
			{
				try
				{
					if (!Socket?.Connected ?? true)
						return false;

					return !(Socket.Poll(1, SelectMode.SelectRead) && Socket.Available == 0);
				}
				catch (SocketException)
				{
					return false;
				}
				catch (ObjectDisposedException)
				{
					return false;
				}
			}
		}

		public ClientController([NotNull] IClientControllerListener requestListener)
		{
			this.requestListener = requestListener;
		}

		public void ClientThread()
		{
			while (Socket != null)
			{
				try
				{
					int id = Socket.ReceiveInt(intBuffer);

					NetworkMessage.NetworkMessageType typeId = NetworkMessage.TypeFromId(Socket.ReceiveInt(intBuffer));

					string message = reader.ReadLine();

					if (message == null)
						continue;

					switch (typeId)
					{
						case NetworkMessage.NetworkMessageType.Message:
							requestListener.OnClientMessage(this, new NetworkMessage(id, message));
							break;

						case NetworkMessage.NetworkMessageType.Request:
							SendMessage(requestListener.OnClientRequest(this, new NetworkRequest(id, message)));
							break;

						case NetworkMessage.NetworkMessageType.Response:
							requestListener.OnClientResponse(this, new NetworkResponse(id, message));
							break;
					}
				}
				catch (SocketException)
				{
					OnDisconnect();
				}
				catch (ObjectDisposedException)
				{
					OnDisconnect();
				}
				catch (SocketExtensions.IntegerReceiveException)
				{
				}
				catch (Exception e)
				{
					TDHPlugin.WriteError($"Error in ClientController:\n{e}");
				}
			}
		}

		public bool Connect([NotNull] string ip, int port)
		{
			try
			{
				Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				Socket.Connect(ip, port);
			}
			catch (SocketException)
			{
				return false;
			}
			catch (ObjectDisposedException)
			{
				return false;
			}

			networkStream = new NetworkStream(Socket, false);
			reader = new StreamReader(networkStream, Encoding.UTF8);

			clientThread = new Thread(ClientThread);
			clientThread.Start();

			return true;
		}

		internal bool DisconnectedCheck()
		{
			if (IsConnected) return false;

			OnDisconnect();
			return true;
		}

		private void OnDisconnect()
		{
			Close();
			requestListener.OnClientDisconnect(this);
		}

		public void Close()
		{
			clientThread.Abort();
			clientThread = null;

			reader.Close();
			networkStream.Close();

			try
			{
				Socket?.Disconnect(false);
				Socket = null;
			}
			catch (SocketException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
		}

		public void Write(int id, [NotNull] string message, NetworkMessage.NetworkMessageType type = NetworkMessage.NetworkMessageType.Message)
		{
			lock (Socket)
			{
				Socket.Send(id.ToBytes());
				Socket.Send(((int) type).ToBytes());
				Socket.Send(Encoding.UTF8.GetBytes($"{message}{'\n'}"));
			}
		}

		public NetworkMessage GenerateMessage([NotNull] string content)
		{
			return new NetworkMessage(unchecked(networkId++), content);
		}

		public void SendMessage([NotNull] NetworkMessage message)
		{
			Write(message.id, message.ToString(), message.GetNetworkMessageType());
		}
	}
}