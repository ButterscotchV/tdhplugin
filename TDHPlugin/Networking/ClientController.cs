using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using TDHPlugin.Networking.NetworkMessage;

namespace TDHPlugin.Networking
{
	public class ClientController
	{
		public readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		public NetworkStream networkStream;
		public StreamReader reader;

		[NotNull] public IClientControllerListener requestListener;

		private readonly Thread clientThread;

		private int networkId;

		public ClientController([NotNull] IClientControllerListener requestListener)
		{
			this.requestListener = requestListener;

			clientThread = new Thread(ClientThread);
		}

		public void ClientThread()
		{
			while (socket.Connected)
			{
				string message = reader.ReadLine();

				if (message == null || !message.Contains(":"))
					continue;

				string[] splitMessage = message.Split(new char[] {':'}, 2);

				if (splitMessage.Length != 2)
					continue;

				string id = splitMessage[0];

				if (!id.Trim().Any())
					continue;

				string content = splitMessage[1];

				if (id[0] == NetworkRequest.Indicator)
				{
					if (id.Length <= 1)
						continue;

					SendMessage(requestListener.OnClientRequest(this, new NetworkRequest(id.Substring(1), content)));
				}
				else if (id[0] == NetworkResponse.Indicator)
				{
					if (id.Length <= 1)
						continue;

					requestListener.OnClientResponse(this, new NetworkResponse(id.Substring(1), content));
				}
				else
				{
					requestListener.OnClientMessage(this, new NetworkMessage.NetworkMessage(id, content));
				}
			}
		}

		public void Connect([NotNull] string ip, int port)
		{
			socket.Connect(ip, port);

			networkStream = new NetworkStream(socket, false);
			reader = new StreamReader(networkStream, Encoding.UTF8);

			clientThread.Start();
		}

		public void Close()
		{
			clientThread.Abort();

			reader.Close();
			networkStream.Close();

			socket.Disconnect(true);
		}

		public void Write([NotNull] string message)
		{
			socket.Send(Encoding.UTF8.GetBytes($"{message}{'\n'}"));
		}

		public NetworkMessage.NetworkMessage GenerateMessage([NotNull] string content)
		{
			return new NetworkMessage.NetworkMessage(networkId++.ToString(), content);
		}

		public void SendMessage([NotNull] NetworkMessage.NetworkMessage message)
		{
			Write(message.ToString());
		}
	}
}