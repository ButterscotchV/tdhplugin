using System.Net.Sockets;
using System.Threading;

namespace TDHPlugin.Networking
{
	public class ClientController
	{
		public readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		public IClientControllerListener requestListener;

		private readonly ScannerSimulator scanner;
		private readonly Thread clientThread;

		public ClientController(IClientControllerListener requestListener)
		{
			this.requestListener = requestListener;

			scanner = new ScannerSimulator(socket);
			clientThread = new Thread(ClientThread);
		}

		public void ClientThread()
		{
			while (socket.Connected)
			{
				string message = scanner.NextLine();

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