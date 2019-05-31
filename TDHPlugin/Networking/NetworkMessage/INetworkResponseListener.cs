using JetBrains.Annotations;

namespace TDHPlugin.Networking.NetworkMessage
{
	public interface INetworkResponseListener
	{
		void OnNetworkResponse([NotNull] NetworkRequest request, [NotNull] NetworkResponse response);
	}
}