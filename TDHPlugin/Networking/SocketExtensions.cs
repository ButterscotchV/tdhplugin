using System;
using System.Net.Sockets;

namespace TDHPlugin.Networking
{
	public static class SocketExtensions
	{
		public class IntegerReceiveException : Exception
		{
		}

		public static int ReceiveInt(this Socket socket, byte[] intBuffer, SocketFlags socketFlags = SocketFlags.None)
		{
			int numBytesReceived = socket.Receive(intBuffer, 0, 4, socketFlags);

			if (numBytesReceived < 0)
				throw new IntegerReceiveException();

			return intBuffer.ToInt();
		}

		public static int ReceiveInt(this Socket socket, SocketFlags socketFlags = SocketFlags.None)
		{
			return ReceiveInt(socket, new byte[4], socketFlags);
		}
	}
}