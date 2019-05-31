using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using MEC;

namespace TDHPlugin.Networking
{
	public class ClientController
	{
		public readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		public IClientControllerListener requestListener;

		private CoroutineHandle clientCoroutine;

		public ClientController(IClientControllerListener requestListener)
		{
			this.requestListener = requestListener;
		}

		public IEnumerator<float> ClientCoroutine()
		{
			while (socket.Connected)
			{
				using (NetworkStream networkStream = new NetworkStream(socket, false))
				using (StreamReader reader = new StreamReader(networkStream))
				{
					string message = reader.ReadLine();

					requestListener.OnClientMessage(this, new NetworkMessage.NetworkMessage("TEST", message));
				}

				yield return Timing.WaitForOneFrame;
			}
		}

		public void Connect(string ip, int port)
		{
			socket.Connect(ip, port);
			clientCoroutine = Timing.RunCoroutine(ClientCoroutine());
		}

		public void Close()
		{
			Timing.KillCoroutines(clientCoroutine);
			socket.Disconnect(true);
		}
	}
}