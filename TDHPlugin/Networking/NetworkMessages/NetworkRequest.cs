using System.Collections.Generic;
using JetBrains.Annotations;

namespace TDHPlugin.Networking.NetworkMessages
{
	public class NetworkRequest : NetworkMessage
	{
		public NetworkRequest(int id, [NotNull] string content, INetworkResponseListener responseListener = null) : base(id, content)
		{
			if (responseListener != null)
				responseListeners.Add(responseListener);
		}

		public NetworkRequest([NotNull] NetworkMessage networkMessage, INetworkResponseListener responseListener = null) : this(networkMessage.id, networkMessage.content, responseListener)
		{
		}

		public new const NetworkMessageType MessageType = NetworkMessageType.Request;

		public readonly List<INetworkResponseListener> responseListeners = new List<INetworkResponseListener>();

		public override NetworkMessageType GetNetworkMessageType()
		{
			return MessageType;
		}

		public void AddResponseListener([NotNull] INetworkResponseListener responseListener)
		{
			responseListeners.Add(responseListener);
		}

		public void RemoveResponseListener([NotNull] INetworkResponseListener responseListener)
		{
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