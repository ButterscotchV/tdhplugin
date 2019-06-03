using System.Threading.Tasks;

namespace TDHPlugin.Networking.NetworkMessages
{
	public class NetworkResponseFuture : TaskCompletionSource<NetworkResponse>, INetworkResponseListener
	{
		public void OnNetworkResponse(NetworkRequest request, NetworkResponse response)
		{
			SetResult(response);
		}

		public void OnNetworkRequestTimeout(NetworkRequest request)
		{
			SetCanceled();
		}
	}
}