using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using TDHPlugin.Networking.NetworkMessages;
using TDHPlugin.TimedObject;

namespace TDHPlugin.Networking
{
	public class ClientController : IDisposable
	{
		public Socket Socket { get; private set; }

		public NetworkStream networkStream;

		[NotNull] public IClientControllerListener requestListener;

		private Thread clientThread;

		public readonly TimedObjectManager<NetworkRequest> timedRequestManager = new TimedObjectManager<NetworkRequest>();

		private int networkId = int.MinValue;

		private bool disposed;

		public ClientController([NotNull] IClientControllerListener requestListener)
		{
			this.requestListener = requestListener;
		}

		public void ClientThread()
		{
			byte[] intBuffer = new byte[4];

			while (!disposed && Socket != null)
			{
				try
				{
					int id = networkStream.ReadInt(intBuffer);

					NetworkMessage.NetworkMessageType typeId = NetworkMessage.TypeFromId(networkStream.ReadInt(intBuffer));

					int length = networkStream.ReadInt(intBuffer);
					if (length < 0)
					{
						TDHPlugin.WriteError("Error in received message: Invalid message length");
						continue;
					}

					string message = length == 0 ? "" : networkStream.ReadString(length);

					switch (typeId)
					{
						case NetworkMessage.NetworkMessageType.Message:
							ThreadPool.QueueUserWorkItem(state => requestListener.OnClientMessage(this, new NetworkMessage(id, message)));
							break;

						case NetworkMessage.NetworkMessageType.Request:
							ThreadPool.QueueUserWorkItem(state => SendMessage(requestListener.OnClientRequest(this, new NetworkRequest(id, message))));
							break;

						case NetworkMessage.NetworkMessageType.Response:
							ThreadPool.QueueUserWorkItem(state => requestListener.OnClientResponse(this, new NetworkResponse(id, message)));
							break;
					}
				}
				catch (AggregateException)
				{
					OnDisconnect();
				}
				catch (SocketException)
				{
					OnDisconnect();
				}
				catch (ObjectDisposedException)
				{
					OnDisconnect();
				}
				catch (NetworkStreamExtensions.IntegerReadException e)
				{
					TDHPlugin.WriteError($"Error in {nameof(ClientController)} while receiving an integer:\n{e}");
				}
				catch (NetworkStreamExtensions.StringReadException e)
				{
					TDHPlugin.WriteError($"Error in {nameof(ClientController)} while receiving a string:\n{e}");
				}
				catch (ThreadAbortException)
				{
					return;
				}
				catch (Exception e)
				{
					TDHPlugin.WriteError($"Error in {nameof(ClientController)}:\n{e}");
				}
			}
		}

		public bool Connect([NotNull] string ip, int port)
		{
			try
			{
				Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				Socket.Connect(ip, port);
			}
			catch (Exception)
			{
				Socket = null;
				return false;
			}

			lock (timedRequestManager.timedObjects)
			{
				timedRequestManager.Run();
			}

			networkStream = new NetworkStream(Socket, false);

			clientThread = new Thread(ClientThread);
			clientThread.Start();

			return true;
		}

		private void OnDisconnect()
		{
			Close();
			requestListener.OnClientDisconnect(this);
		}

		public void Close()
		{
			Dispose();
		}

		public bool IsConnected(bool executeOnDisconnect = true)
		{
			bool isConnected = false;

			try
			{
				if (Socket?.Connected ?? false)
				{
					isConnected = !(Socket.Poll(1, SelectMode.SelectRead) && Socket.Available == 0);
				}
			}
			catch (SocketException)
			{
				isConnected = false;
			}
			catch (ObjectDisposedException)
			{
				isConnected = false;
			}

			if (executeOnDisconnect && Socket != null && !isConnected)
			{
				OnDisconnect();
			}

			return isConnected;
		}

		public void Write(int id, [NotNull] string message, NetworkMessage.NetworkMessageType type = NetworkMessage.NetworkMessageType.Message)
		{
			lock (Socket)
			{
				Socket.Send(id.ToBytes());
				Socket.Send(((int) type).ToBytes());

				byte[] messageBytes = Encoding.UTF8.GetBytes(message);

				Socket.Send(messageBytes.Length.ToBytes());

				if (messageBytes.Length > 0)
					Socket.Send(messageBytes);
			}
		}

		public NetworkMessage GenerateMessage([NotNull] string content)
		{
			return new NetworkMessage(unchecked(networkId++), content);
		}


		public NetworkRequest GenerateRequest([NotNull] string content, INetworkResponseListener responseListener = null)
		{
			return new NetworkRequest(unchecked(networkId++), content, responseListener);
		}

		public void SendMessage([NotNull] NetworkMessage message)
		{
			Write(message.id, message.ToString(), message.GetNetworkMessageType());
		}

		public void SendRequest([NotNull] TimedObject<NetworkRequest> request)
		{
			lock (timedRequestManager.timedObjects)
			{
				timedRequestManager.timedObjects.Add(request);
			}

			SendMessage(request.obj);
		}

		[NotNull]
		public NetworkResponseFuture SendRequestWaitable([NotNull] TimedObject<NetworkRequest> request)
		{
			NetworkResponseFuture future = new NetworkResponseFuture();
			request.obj.AddResponseListener(future);

			SendRequest(request);

			return future;
		}

		public static NetworkResponse WaitOnRequest([NotNull] NetworkResponseFuture requestWaitable, TimeSpan? timeout = null)
		{
			try
			{
				if (timeout.HasValue)
					requestWaitable.Task.Wait(timeout.Value);
				else
					requestWaitable.Task.Wait();

				return requestWaitable.Task.IsCompleted ? requestWaitable.Task.Result : null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public NetworkResponse WaitOnRequest([NotNull] NetworkResponseFuture requestWaitable, long timeout, TimeUnit timeoutUnit)
		{
			return WaitOnRequest(requestWaitable, TimeUtils.TimeUnitToTimeSpan(timeout, timeoutUnit));
		}

		public NetworkResponse SendRequestBlocking([NotNull] TimedObject<NetworkRequest> request, TimeSpan? additionalTimeout)
		{
			if (additionalTimeout.HasValue)
			{
				TimeSpan timeout = request.Timeout + additionalTimeout.Value;
				return WaitOnRequest(SendRequestWaitable(request), timeout);
			}

			return WaitOnRequest(SendRequestWaitable(request), request.Timeout);
		}

		public NetworkResponse SendRequestBlocking([NotNull] TimedObject<NetworkRequest> request, long additionalTimeout = 1, TimeUnit additionalTimeoutUnit = TimeUnit.Seconds)
		{
			if (additionalTimeout >= 0)
			{
				return SendRequestBlocking(request, TimeUtils.TimeUnitToTimeSpan(additionalTimeout, additionalTimeoutUnit));
			}

			return SendRequestBlocking(request, null);
		}

		public TimedObject<NetworkRequest> GetRequest(int id)
		{
			lock (timedRequestManager.timedObjects)
			{
				foreach (TimedObject<NetworkRequest> request in timedRequestManager.timedObjects)
				{
					if (request.obj.id == id)
						return request;
				}
			}

			return null;
		}

		public void CompleteRequest(TimedObject<NetworkRequest> request, NetworkResponse response)
		{
			lock (timedRequestManager.timedObjects)
			{
				timedRequestManager.FinishTimedObject(request);
			}

			request.obj.ExecuteResponseHandlers(response);
		}

		public void Dispose()
		{
			if (disposed)
				return;

			disposed = true;

			clientThread = null;

			networkStream?.Close();
			networkStream = null;

			timedRequestManager.Shutdown();

			try
			{
				Socket?.Disconnect(false);
			}
			catch (SocketException)
			{
			}
			catch (ObjectDisposedException)
			{
			}

			Socket = null;
		}
	}
}