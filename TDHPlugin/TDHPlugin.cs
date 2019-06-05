using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using JetBrains.Annotations;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;
using TDHPlugin.Commands;
using TDHPlugin.Networking;
using TDHPlugin.Networking.NetworkMessages;
using TDHPlugin.TimedObject;

namespace TDHPlugin
{
	[PluginDetails(
		author = "Dankrushen",
		name = "TDHPlugin",
		description = "A plugin for connecting SCP: SL to Discord",
		id = "dankrushen.tdh.plugin",
		configPrefix = "tdh",
		langFile = "TDHPlugin",
		version = Version,
		SmodMajor = 3,
		SmodMinor = 4,
		SmodRevision = 1
	)]
	public class TDHPlugin : Plugin, IClientControllerListener
	{
		public const string Version = "0.0.1";

		public static TDHPlugin Singleton { get; private set; }

		[ConfigOption] public readonly string botIp = "127.0.0.1";
		[ConfigOption] public readonly int botPort = 41242;
		[ConfigOption] private readonly int requestTimeoutMs = 5000;
		public static ClientController Client { get; private set; }

		[ConfigOption] private readonly int disconnectCheckDelayMs = 250;
		[ConfigOption] private readonly int reconnectDelayMs = 1000;
		private static bool reconnect = true;
		public Thread ReconnectThread { get; private set; }

		[ConfigOption] private readonly int maxRetryPrints = 5;
		private static int retryPrintCount;

		private static bool clientEnabled;

		public override void Register()
		{
			Singleton = this;

			AddEventHandlers(new PlayerConsoleCommandListener(this));
		}

		public override void OnEnable()
		{
			Client = new ClientController(this);
			clientEnabled = true;
			ReconnectThread = new Thread(ClientReconnectThread);
			reconnect = true;
			ReconnectThread.Start();

			Info(Details.name + " v" + Details.version + " has loaded.");
		}

		public override void OnDisable()
		{
			clientEnabled = false;
			ReconnectThread.Abort();
			ReconnectThread = null;
			Client?.Close();

			Info(Details.name + " v" + Details.version + " was disabled.");
		}

		public static void Write([NotNull] string message)
		{
			Singleton?.Info(message);
		}

		public static void WriteWarning([NotNull] string message)
		{
			Singleton?.Warn(message);
		}

		public static void WriteError([NotNull] string message)
		{
			Singleton?.Error(message);
		}

		public void ClientReconnectThread()
		{
			while (clientEnabled)
			{
				if (reconnect)
				{
					bool shouldPrint = maxRetryPrints <= 0 || retryPrintCount < maxRetryPrints;

					if (shouldPrint)
						Write("Attempting connection to TDH Bot...");

					if (Client.Connect(botIp, botPort))
					{
						reconnect = false;
						retryPrintCount = 0;

						Write("Successfully connected to TDH Bot!");
					}
					else
					{
						if (shouldPrint)
							Write($"Failed to connect to TDH Bot, retrying in {reconnectDelayMs} ms...");

						unchecked
						{
							retryPrintCount++;
						}

						if (retryPrintCount == maxRetryPrints)
							Write($"Hit max retry messages ({maxRetryPrints}), no longer printing retries.");

						Thread.Sleep(reconnectDelayMs);
					}
				}
				else
				{
					Thread.Sleep(disconnectCheckDelayMs);

					Client.IsConnected();
				}
			}
		}

		public void OnClientDisconnect(ClientController controller)
		{
			reconnect = true;

			Write("Disconnected from TDH Bot.");
		}

		public void OnClientMessage(ClientController controller, NetworkMessage message)
		{
			Debug($"Message ({message.id}): \"{message.content}\"");
		}

		public NetworkResponse OnClientRequest(ClientController controller, NetworkRequest request)
		{
			Debug($"Request ({request.id}): \"{request.content}\"");

			string[] args = CommandUtils.StringToArgs(request.content);

			if (args.Any())
			{
				switch (args[0].ToUpper())
				{
					case "TEST":
						return new NetworkResponse(request.id, "Testing");

					case "TIMEDOBJECTMANAGER":
					case "TIMEDOBJECTS":
					case "TIMEDOBJECT":
						Client.timedRequestManager.timedObjects.Add(new TimedObject<NetworkRequest>(new NetworkRequest(0, "TESTING"), requestTimeoutMs, TimeUnit.Millis, onFinish: OnNetworkRequestFinish, onExpire: OnNetworkRequestExpire));
						Client.timedRequestManager.timedObjects.Add(new TimedObject<NetworkRequest>(new NetworkRequest(0, "TESTING"), requestTimeoutMs * 2, TimeUnit.Millis, onFinish: OnNetworkRequestFinish, onExpire: OnNetworkRequestExpire));
						return new NetworkResponse(request.id, "Running TimedObject/TimedObjectManager test...");

					case "REQUESTBLOCKING":
						return new NetworkResponse(request.id, Client.SendRequestBlocking(new TimedObject<NetworkRequest>(new NetworkRequest(Client.GenerateMessage("Hello")), requestTimeoutMs, TimeUnit.Millis)).content);

					case "PRINT":
						if (args.Length >= 3)
						{
							List<Player> players = PluginManager?.Server?.GetPlayers();

							if (players != null)
							{
								Player player = null;

								foreach (Player smodPlayer in players)
								{
									if (smodPlayer.SteamId == args[1])
									{
										player = smodPlayer;
										break;
									}
								}

								player?.SendConsoleMessage(args[2].Replace("{%newline%}", "\n"));

								return new NetworkResponse(request.id, "Success");
							}
						}

						return new NetworkResponse(request.id, "Error");

					default:
						return new NetworkResponse(request.id, "Hello, world!");
				}
			}

			return new NetworkResponse(request.id, "Error");
		}

		public static void OnNetworkRequestFinish([NotNull] TimedObject<NetworkRequest> request)
		{
			Write("Finished NetworkRequest!");
		}

		public static void OnNetworkRequestExpire([NotNull] TimedObject<NetworkRequest> request)
		{
			Write("NetworkRequest expired...");
			lock (Client.timedRequestManager.timedObjects)
			{
				Write("Finishing remaining NetworkRequests...");
				foreach (TimedObject<NetworkRequest> timedObject in Client.timedRequestManager.timedObjects)
				{
					timedObject.finished = true;
				}
			}
		}

		public void OnClientResponse(ClientController controller, NetworkResponse response)
		{
			Debug($"Response ({response.id}): \"{response.content}\"");

			TimedObject<NetworkRequest> request = controller.GetRequest(response.id);

			if (request != null)
			{
				controller.CompleteRequest(request, response);
			}
		}

		public static NetworkResponse SendClientRequest(string message)
		{
			if (!Client.IsConnected())
				return null;

			try
			{
				return Client.SendRequestBlocking(new TimedObject<NetworkRequest>(new NetworkRequest(Client.GenerateMessage(message)), Singleton.requestTimeoutMs, TimeUnit.Millis));
			}
			catch (SocketException)
			{
			}
			catch (ObjectDisposedException)
			{
			}

			return null;
		}
	}
}