using System.Linq;
using Smod2.EventHandlers;
using Smod2.Events;
using TDHPlugin.Networking.NetworkMessages;

namespace TDHPlugin.Commands
{
	public class PlayerConsoleCommandListener : IEventHandlerCallCommand
	{
		public readonly TDHPlugin plugin;

		public PlayerConsoleCommandListener(TDHPlugin plugin)
		{
			this.plugin = plugin;
		}

		public void OnCallCommand(PlayerCallCommandEvent ev)
		{
			string[] args = CommandUtils.StringToArgs(ev.Command);

			if (args.Any())
			{
				NetworkResponse response;

				switch (args[0].ToUpper())
				{
					case "CONNECT":
						if (args.Length >= 2)
						{
							response = TDHPlugin.SendClientRequest($"FINISHCONNECT {ev.Player.SteamId} Console {args[1]}");
						}
						else
						{
							response = TDHPlugin.SendClientRequest($"CONNECT {ev.Player.SteamId} Console");
						}

						ev.ReturnMessage = response?.content ?? "Unable to send request";

						break;

					case "DISCONNECT":
						response = TDHPlugin.SendClientRequest($"DISCONNECT {ev.Player.SteamId} Console");

						ev.ReturnMessage = response?.content ?? "Unable to send request";

						break;
				}
			}
		}
	}
}