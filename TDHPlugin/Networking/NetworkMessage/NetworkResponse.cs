using System.Linq;

namespace TDHPlugin.Networking.NetworkMessage
{
	public class NetworkResponse : NetworkMessage
	{
		private const char Indicator = 'R';

		public NetworkResponse(string id, string content) : base(id, content)
		{
		}

		public NetworkResponse(NetworkMessage networkMessage) : this(networkMessage.id, networkMessage.content)
		{
		}

		public override string ToString()
		{
			return !content.Trim().Any() ? $"{Indicator}{id}" : $"{Indicator}{id}:{content}";
		}
	}
}