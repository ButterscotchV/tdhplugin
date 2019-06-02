using System.Threading;
using Smod2;
using Smod2.Attributes;
using Smod2.Config;
using TDHPlugin.Networking;
using TDHPlugin.Networking.NetworkMessages;

namespace TDHPlugin
{
	[PluginDetails(
		author = "Dankrushen",
		name = "TDHPlugin",
		description = "A plugin for connecting SCP: SL to Discord",
		id = "dankrushen.tdh.plugin",
		configPrefix = "tdh",
		langFile = "TDHPlugin",
		version = "0.0.1",
		SmodMajor = 3,
		SmodMinor = 4,
		SmodRevision = 1
	)]
	public class TDHPlugin : Plugin, IClientControllerListener
	{
		[ConfigOption] public readonly string botIp = "127.0.0.1";
		[ConfigOption] public readonly int botPort = 41242;

		public static TDHPlugin Singleton { get; private set; }

		public static ClientController Client { get; private set; }

		public Thread reconnectThread { get; private set; }
		[ConfigOption] private readonly int disconnectCheckDelayMs = 250;
		[ConfigOption] private readonly int reconnectDelayMs = 1000;
		private static bool reconnect = true;

		[ConfigOption] private readonly int maxRetryPrints = 5;
		private static int retryPrintCount;

		private static bool clientEnabled;

		public override void Register()
		{
			Singleton = this;
		}

		public override void OnEnable()
		{
			Client = new ClientController(this);
			clientEnabled = true;
			reconnectThread = new Thread(ClientReconnectThread);
			reconnectThread.Start();

			Info(Details.name + " v" + Details.version + " has loaded.");
		}

		public override void OnDisable()
		{
			clientEnabled = false;
			reconnectThread.Abort();
			reconnectThread = null;
			Client?.Close();

			Info(Details.name + " v" + Details.version + " was disabled.");
		}

		public static void Write(string message)
		{
			Singleton?.Info(message);
		}

		public static void WriteWarning(string message)
		{
			Singleton?.Warn(message);
		}

		public static void WriteError(string message)
		{
			Singleton?.Error(message);
		}

		public static void ClientReconnectThread()
		{
			while (clientEnabled)
			{
				if (reconnect)
				{
					bool shouldPrint = Singleton.maxRetryPrints <= 0 || retryPrintCount < Singleton.maxRetryPrints;

					if (shouldPrint)
						Write("Attempting connection to TDH Bot...");

					if (Client.Connect(Singleton.botIp, Singleton.botPort))
					{
						reconnect = false;
						retryPrintCount = 0;

						Write("Successfully connected to TDH Bot!");
					}
					else
					{
						if (shouldPrint)
							Write($"Failed to connect to TDH Bot, retrying in {Singleton.reconnectDelayMs} ms...");

						unchecked
						{
							retryPrintCount++;
						}

						if (retryPrintCount == Singleton.maxRetryPrints)
							Write($"Hit max retry messages ({Singleton.maxRetryPrints}), no longer printing retries.");

						Thread.Sleep(Singleton.reconnectDelayMs);
					}
				}
				else
				{
					Thread.Sleep(Singleton.disconnectCheckDelayMs);

					Client.DisconnectedCheck();
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
			Info($"Message ({message.id}): \"{message.content}\"");
		}

		public NetworkResponse OnClientRequest(ClientController controller, NetworkRequest request)
		{
			Info($"Request ({request.id}): \"{request.content}\"");

			switch (request.content.ToLower())
			{
				case "test":
					return new NetworkResponse(request.id, "Testing");

				default:
					return new NetworkResponse(request.id, "Hello, world!");
			}
		}

		public void OnClientResponse(ClientController controller, NetworkResponse response)
		{
			Info($"Response ({response.id}): \"{response.content}\"");
		}
	}
}