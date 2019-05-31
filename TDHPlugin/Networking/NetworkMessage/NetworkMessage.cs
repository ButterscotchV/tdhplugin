using System;
using System.Linq;

namespace TDHPlugin.Networking.NetworkMessage
{
	public class NetworkMessage
	{
		public readonly string id;
		public readonly string content;

		public NetworkMessage(string id, string content)
		{
			if (!id.Trim().Any())
				throw new ArgumentException($"{nameof(id)} must not be blank");

			this.id = id;
			this.content = content;
		}

		public override string ToString()
		{
			return !content.Trim().Any() ? id : $"{id}:{content}";
		}
	}
}