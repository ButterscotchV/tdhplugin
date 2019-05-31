namespace TDHPlugin.Networking.NetworkMessage
{
	public interface INetworkResponseListener
	{
		void OnNetworkResponse(NetworkRequest request, NetworkResponse response);
	}
}
