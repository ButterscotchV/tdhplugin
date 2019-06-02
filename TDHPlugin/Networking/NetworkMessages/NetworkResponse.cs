using JetBrains.Annotations;

namespace TDHPlugin.Networking.NetworkMessages
{
	public class NetworkResponse : NetworkMessage
	{
		public NetworkResponse(int id, [NotNull] string content) : base(id, content)
		{
		}

		public NetworkResponse([NotNull] NetworkMessage networkMessage) : this(networkMessage.id, networkMessage.content)
		{
		}

		public new const NetworkMessageType MessageType = NetworkMessageType.Response;

		public override NetworkMessageType GetNetworkMessageType()
		{
			return MessageType;
		}
	}
}