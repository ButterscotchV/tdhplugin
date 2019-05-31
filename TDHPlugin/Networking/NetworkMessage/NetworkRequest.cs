using System.Collections.Generic;
using System.Linq;

namespace TDHPlugin.Networking.NetworkMessage
{
	public class NetworkRequest : NetworkMessage
	{
		private const char Indicator = 'Q';

		public NetworkRequest(string id, string content, INetworkResponseListener responseListener = null) : base(id, content)
		{
			if (responseListener != null)
				responseListeners.Add(responseListener);
		}

		public NetworkRequest(NetworkMessage networkMessage, INetworkResponseListener responseListener = null) : this(networkMessage.id, networkMessage.content, responseListener)
		{
		}

		public readonly List<INetworkResponseListener> responseListeners = new List<INetworkResponseListener>();

		public override string ToString()
		{
			return !content.Trim().Any() ? $"{Indicator}{id}" : $"{Indicator}{id}:{content}";
		}

		public void AddResponseListener(INetworkResponseListener responseListener)
		{
			if (responseListener != null)
				responseListeners.Add(responseListener);
		}

		public void RemoveResponseListener(INetworkResponseListener responseListener)
		{
			if (responseListener != null)
				responseListeners.Remove(responseListener);
		}

		public void RemoveAllResponseListeners()
		{
			responseListeners.Clear();
		}

		public void ExecuteListeners(NetworkResponse response)
		{
			foreach (INetworkResponseListener responseListener in responseListeners)
			{
				responseListener.OnNetworkResponse(this, response);
			}
		}
	}
}