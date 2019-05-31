using JetBrains.Annotations;
using TDHPlugin.Networking.NetworkMessage;

namespace TDHPlugin.Networking
{
	public interface IClientControllerListener
	{
		void OnClientDisconnect([NotNull] ClientController controller);

		void OnClientMessage([NotNull] ClientController controller, [NotNull] NetworkMessage.NetworkMessage message);
		NetworkResponse OnClientRequest([NotNull] ClientController controller, [NotNull] NetworkRequest request);
		void OnClientResponse([NotNull] ClientController controller, [NotNull] NetworkResponse response);
	}
}