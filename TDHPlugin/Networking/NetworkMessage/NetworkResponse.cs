using JetBrains.Annotations;

namespace TDHPlugin.Networking.NetworkMessage
{
	public class NetworkResponse : NetworkMessage
	{
		public const char Indicator = 'R';

		public NetworkResponse([NotNull] string id, [NotNull] string content) : base(id, content)
		{
		}

		public NetworkResponse([NotNull] NetworkMessage networkMessage) : this(networkMessage.id, networkMessage.content)
		{
		}

		public override string ToString()
		{
			return $"{Indicator}{id}:{content}";
		}
	}
}