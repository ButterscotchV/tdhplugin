using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TDHPlugin.Networking
{
	public class ScannerSimulator
	{
		private readonly Socket socket;
		private readonly StringBuilder stringBuilder = new StringBuilder();

		private readonly byte[] bytes;

		public readonly Queue<string> lineBuffer = new Queue<string>();

		public ScannerSimulator(Socket socket, int bufferSize = 1024)
		{
			this.socket = socket ?? throw new NullReferenceException($"{nameof(socket)} must not be null");
			bytes = new byte[bufferSize];
		}

		public string NextLine()
		{
			if (lineBuffer.Any())
				return lineBuffer.Dequeue();

			while (true)
			{
				string message = Encoding.UTF8.GetString(bytes, 0, socket.Receive(bytes));

				if (message.Contains(Environment.NewLine))
				{
					string[] finalMessages = message.Split(new string[] {Environment.NewLine}, StringSplitOptions.None);

					stringBuilder.Append(finalMessages[0]);
					string line = stringBuilder.ToString();
					stringBuilder.Clear();

					if (finalMessages.Length <= 1)
						return line;

					for (int i = 1; i < stringBuilder.Length - 1; i++)
					{
						lineBuffer.Enqueue(message);
					}

					stringBuilder.Append(stringBuilder[stringBuilder.Length - 1]);

					return line;
				}

				stringBuilder.Append(message);
			}
		}
	}
}