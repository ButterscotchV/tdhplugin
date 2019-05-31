using System;
using System.Linq;
using JetBrains.Annotations;

namespace TDHPlugin.Networking.NetworkMessage
{
	public class NetworkMessage
	{
		public readonly string id;
		public readonly string content;

		public NetworkMessage([NotNull] string id, [NotNull] string content)
		{
			if (!id.Trim().Any())
				throw new ArgumentException($"{nameof(id)} must not be blank");

			this.id = id;
			this.content = content;
		}

		public override string ToString()
		{
			return $"{id}:{content}";
		}
	}
}