using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace TDHPlugin.Networking
{
	public static class NetworkStreamExtensions
	{
		private static int WaitOnRead([NotNull] Task<int> readTask)
		{
			readTask.Wait();
			return readTask.IsCompleted ? readTask.Result : -1;
		}

		public class IntegerReadException : IOException
		{
		}

		public static int ReadInt([NotNull] this NetworkStream networkStream, [NotNull] byte[] intBuffer)
		{
			int numBytesReceived = WaitOnRead(networkStream.ReadAsync(intBuffer, 0, 4));

			if (numBytesReceived != 4)
				throw new IntegerReadException();

			return intBuffer.ToInt();
		}

		public static int ReadInt([NotNull] this NetworkStream networkStream, [NotNull] byte[] intBuffer, int offset)
		{
			int numBytesReceived = WaitOnRead(networkStream.ReadAsync(intBuffer, offset, 4));

			if (numBytesReceived != 4)
				throw new IntegerReadException();

			return intBuffer.ToInt(offset);
		}

		public static int ReadInt([NotNull] this NetworkStream networkStream)
		{
			byte[] intBuffer = new byte[4];
			int numBytesReceived = WaitOnRead(networkStream.ReadAsync(intBuffer, 0, 4));

			if (numBytesReceived != 4)
				throw new IntegerReadException();

			return intBuffer.ToInt();
		}

		public class StringReadException : IOException
		{
		}

		[NotNull]
		public static string ReadString([NotNull] this NetworkStream networkStream, [NotNull] byte[] stringBuffer, int offset, int size, [NotNull] Encoding encoding)
		{
			int numBytesReceived = WaitOnRead(networkStream.ReadAsync(stringBuffer, offset, size));

			if (numBytesReceived < 0)
				throw new StringReadException();

			return encoding.GetString(stringBuffer, offset, numBytesReceived);
		}

		public static string ReadString([NotNull] this NetworkStream networkStream, [NotNull] byte[] stringBuffer, int offset, int size)
		{
			return ReadString(networkStream, stringBuffer, offset, size, Encoding.UTF8);
		}

		public static string ReadString([NotNull] this NetworkStream networkStream, [NotNull] byte[] stringBuffer, int size)
		{
			return ReadString(networkStream, stringBuffer, 0, size);
		}

		public static string ReadString([NotNull] this NetworkStream networkStream, [NotNull] int size)
		{
			return ReadString(networkStream, new byte[size], size);
		}
	}
}