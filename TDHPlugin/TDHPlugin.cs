using System;
using System.Threading;
using Smod2;
using Smod2.Attributes;
using Smod2.Config;
using Smod2.EventHandlers;
using Smod2.Events;
using TDHPlugin.Networking;
using TDHPlugin.Networking.NetworkMessages;
using UnityEngine;

namespace TDHPlugin
{
	[PluginDetails(
		author = "Dankrushen",
		name = "Test",
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
		[ConfigOption] public static readonly string botIp = "127.0.0.1";
		[ConfigOption] public static readonly int botPort = 41242;

		public static TDHPlugin Singleton { get; private set; }

		public static ClientController Client { get; private set; }

		public Thread reconnectThread = new Thread(ClientReconnectThread);
		[ConfigOption] private static readonly int disconnectCheckDelayMs = 250;
		[ConfigOption] private static readonly int reconnectDelayMs = 1000;
		private static bool reconnect = true;

		[ConfigOption] private static readonly int maxRetryPrints = 2;
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
			reconnectThread.Start();

			Info(Details.name + " v" + Details.version + " has loaded.");
		}

		public override void OnDisable()
		{
			clientEnabled = false;
			reconnectThread.Abort();
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
					bool shouldPrint = retryPrintCount < maxRetryPrints;

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
					}

					unchecked
					{
						retryPrintCount++;
					}

					if (retryPrintCount == maxRetryPrints)
						Write($"Hit max retry messages ({maxRetryPrints}), no longer printing retries.");

					Thread.Sleep(reconnectDelayMs);
				}
				else
				{
					Thread.Sleep(disconnectCheckDelayMs);

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

			return new NetworkResponse(request.id, "Hello, world!");
		}

		public void OnClientResponse(ClientController controller, NetworkResponse response)
		{
			Info($"Response ({response.id}): \"{response.content}\"");
		}
	}
}