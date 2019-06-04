using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TDHPlugin.Networking.NetworkMessages;
using TDHPlugin.TimedObject;

namespace TDHPlugin.Networking
{
	public class ClientController
	{
		public Socket Socket { get; private set; }

		public NetworkStream networkStream;
		public StreamReader reader;

		[NotNull] private readonly byte[] intBuffer = new byte[4];

		[NotNull] public IClientControllerListener requestListener;

		private Thread clientThread;

		public readonly TimedObjectManager<NetworkRequest> timedRequestManager = new TimedObjectManager<NetworkRequest>();

		private int networkId = int.MinValue;

		public bool IsConnected
		{
			get
			{
				try
				{
					if (!Socket?.Connected ?? true)
						return false;

					return !(Socket.Poll(1, SelectMode.SelectRead) && Socket.Available == 0);
				}
				catch (SocketException)
				{
					return false;
				}
				catch (ObjectDisposedException)
				{
					return false;
				}
			}
		}

		public ClientController([NotNull] IClientControllerListener requestListener)
		{
			this.requestListener = requestListener;
		}

		public void ClientThread()
		{
			while (Socket != null)
			{
				try
				{
					int id = Socket.ReceiveInt(intBuffer);

					NetworkMessage.NetworkMessageType typeId = NetworkMessage.TypeFromId(Socket.ReceiveInt(intBuffer));

					string message = reader.ReadLine();

					if (message == null)
						continue;

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
				catch (SocketException)
				{
					OnDisconnect();
				}
				catch (ObjectDisposedException)
				{
					OnDisconnect();
				}
				catch (SocketExtensions.IntegerReceiveException)
				{
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
			catch (SocketException)
			{
				Socket = null;
				return false;
			}
			catch (ObjectDisposedException)
			{
				Socket = null;
				return false;
			}

			timedRequestManager.Run();

			networkStream = new NetworkStream(Socket, false);
			reader = new StreamReader(networkStream, Encoding.UTF8);

			clientThread = new Thread(ClientThread);
			clientThread.Start();

			return true;
		}

		internal bool DisconnectedCheck()
		{
			if (IsConnected) return false;

			OnDisconnect();
			return true;
		}

		private void OnDisconnect()
		{
			Close();
			requestListener.OnClientDisconnect(this);
		}

		public void Close()
		{
			clientThread.Abort();
			clientThread = null;

			reader.Close();
			networkStream.Close();

			timedRequestManager.Shutdown();

			try
			{
				Socket?.Disconnect(false);
				Socket = null;
			}
			catch (SocketException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
		}

		public void Write(int id, [NotNull] string message, NetworkMessage.NetworkMessageType type = NetworkMessage.NetworkMessageType.Message)
		{
			lock (Socket)
			{
				Socket.Send(id.ToBytes());
				Socket.Send(((int) type).ToBytes());
				Socket.Send(Encoding.UTF8.GetBytes($"{message}{'\n'}"));
			}
		}

		public NetworkMessage GenerateMessage([NotNull] string content)
		{
			return new NetworkMessage(unchecked(networkId++), content);
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

		public NetworkResponse WaitOnRequest([NotNull] NetworkResponseFuture requestWaitable)
		{
			try
			{
				requestWaitable.Task.Wait();

				return requestWaitable.Task.IsCompleted ? requestWaitable.Task.Result : null;
			}
			catch (AggregateException)
			{
				return null;
			}
		}

		public NetworkResponse SendRequestBlocking([NotNull] TimedObject<NetworkRequest> request)
		{
			return WaitOnRequest(SendRequestWaitable(request));
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
			timedRequestManager.FinishTimedObject(request);

			request.obj.ExecuteResponseHandlers(response);
		}
	}
}