using System;
using Smod2;
using Smod2.Attributes;
using Smod2.Config;
using TDHPlugin.Networking;
using TDHPlugin.Networking.NetworkMessage;

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
		[ConfigOption] public readonly string botIp = "127.0.0.1";

		[ConfigOption] public readonly int botPort = 41242;

		public ClientController Client { get; private set; }

		public override void OnDisable()
		{
			Client?.Close();
			Info(Details.name + " v" + Details.version + " was disabled.");
		}

		public override void OnEnable()
		{
			Client = new ClientController(this);
			Client.Connect(botIp, botPort);
			Info(Details.name + " v" + Details.version + " has loaded.");
		}

		public override void Register()
		{
		}

		public void OnClientDisconnect(ClientController controller)
		{
			throw new NotImplementedException();
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