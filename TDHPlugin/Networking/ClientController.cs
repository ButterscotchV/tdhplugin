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
		public readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		public NetworkStream networkStream;
		public StreamReader reader;

		[NotNull] private readonly byte[] intBuffer = new byte[4];

		[NotNull] public IClientControllerListener requestListener;

		private readonly Thread clientThread;

		private int networkId = int.MinValue;

		public ClientController([NotNull] IClientControllerListener requestListener)
		{
			this.requestListener = requestListener;

			clientThread = new Thread(ClientThread);
		}

		public void ClientThread()
		{
			while (socket.Connected)
			{
				try
				{
					int id = socket.ReceiveInt(intBuffer);
					int typeIdInt = socket.ReceiveInt(intBuffer);

					TDHPlugin.Write($"{nameof(typeIdInt)}: {typeIdInt}");

					NetworkMessage.NetworkMessageType typeId = NetworkMessage.TypeFromId(typeIdInt);
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

		public bool IsConnected()
		{
			try
			{
				return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
			}
			catch (SocketException)
			{
				return false;
			}
		}

		public void Connect([NotNull] string ip, int port)
		{
			socket.Connect(ip, port);

			networkStream = new NetworkStream(socket, false);
			reader = new StreamReader(networkStream, Encoding.UTF8);

			clientThread.Start();
		}

		private void OnDisconnect()
		{
			Close();
			requestListener.OnClientDisconnect(this);
		}

		public void Close()
		{
			clientThread.Abort();

			reader.Close();
			networkStream.Close();

			socket.Disconnect(true);
		}

		public void Write(int id, [NotNull] string message, NetworkMessage.NetworkMessageType type = NetworkMessage.NetworkMessageType.Message)
		{
			lock (socket)
			{
				socket.Send(id.ToBytes());
				socket.Send(((int) type).ToBytes());
				socket.Send(Encoding.UTF8.GetBytes($"{message}{'\n'}"));
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