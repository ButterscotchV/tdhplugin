using System;
using JetBrains.Annotations;

namespace TDHPlugin.Networking.NetworkMessages
{
	public class NetworkMessage
	{
		public enum NetworkMessageType
		{
			Message = 0,
			Request = 1,
			Response = 2
		}

		public static NetworkMessageType TypeFromId(int id)
		{
			foreach (NetworkMessageType type in Enum.GetValues(typeof(NetworkMessageType)))
			{
				if ((int) type == id)
					return type;
			}

			throw new ArgumentException($"NetworkMessageType with provided {nameof(id)} was not found");
		}

		public const NetworkMessageType MessageType = NetworkMessageType.Message;

		public readonly int id;
		[NotNull] public readonly string content;

		public NetworkMessage(int id, [NotNull] string content)
		{
			this.id = id;
			this.content = content;
		}

		public virtual NetworkMessageType GetNetworkMessageType()
		{
			return MessageType;
		}

		public override string ToString()
		{
			return content;
		}
	}
}