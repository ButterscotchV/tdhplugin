using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TDHPlugin.Networking
{
	public class ClientController
	{
		public readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		public IClientControllerListener requestListener;

		private Thread clientThread;

		public ClientController(IClientControllerListener requestListener)
		{
			this.requestListener = requestListener;
			clientThread = new Thread(ClientThread);
		}

		public void ClientThread()
		{
			byte[] bytes = new byte[1024];

			while (socket.Connected)
			{
				int length = socket.Receive(bytes);

				string message = Encoding.UTF8.GetString(bytes, 0, length);

				requestListener.OnClientMessage(this, new NetworkMessage.NetworkMessage("TEST", message));
			}
		}

		public void Connect(string ip, int port)
		{
			socket.Connect(ip, port);
			clientThread.Start();
		}

		public void Close()
		{
			clientThread?.Abort();
			socket.Disconnect(true);
		}
	}
}
