using TDHPlugin.Networking.NetworkMessage;

namespace TDHPlugin.Networking
{
	public interface IClientControllerListener
	{
		void OnClientMessage(ClientController controller, NetworkMessage.NetworkMessage message);
		NetworkResponse OnClientRequest(ClientController controller, NetworkRequest request);
		void OnClientResponse(ClientController controller, NetworkResponse response);

		void OnClientDisconnect(ClientController controller);
	}
}