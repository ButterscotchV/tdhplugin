using System.Collections.Generic;
using JetBrains.Annotations;
using MEC;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using TDHPlugin.Utils;

namespace TDHPlugin.EventHandlers
{
	public class PlayerJoinHandler : IEventHandlerPlayerJoin
	{
		public readonly TDHPlugin plugin;

		public PlayerJoinHandler(TDHPlugin plugin)
		{
			this.plugin = plugin;
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			Timing.RunCoroutine(PlayerJoinCoroutine(ev.Player));
		}

		public static IEnumerator<float> PlayerJoinCoroutine([NotNull] Player player)
		{
			PlayerPerkFetcher.PlayerPerks perks = PlayerPerkFetcher.GetPlayerPerks(player);

			PlayerPerkFetcher.UpdatePlayerSlot(player, perks);
			PlayerPerkFetcher.UpdatePlayerTag(player, perks, TDHPlugin.Singleton.tagPrefix, TDHPlugin.Singleton.tagColour);

			yield break;
		}
	}
}