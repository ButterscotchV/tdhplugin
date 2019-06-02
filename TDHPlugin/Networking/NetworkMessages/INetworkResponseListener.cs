using JetBrains.Annotations;

namespace TDHPlugin.Networking.NetworkMessages
{
	public interface INetworkResponseListener
	{
		void OnNetworkResponse([NotNull] NetworkRequest request, [NotNull] NetworkResponse response);
		void OnNetworkRequestTimeout([NotNull] NetworkRequest request);
	}
}