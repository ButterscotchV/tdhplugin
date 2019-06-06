using System.Collections.Generic;
using JetBrains.Annotations;
using MEC;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using TDHPlugin.Utils;

namespace TDHPlugin.EventHandlers
{
	public class ClassSetHandler : IEventHandlerSetRole
	{
		public readonly TDHPlugin plugin;

		public ClassSetHandler(TDHPlugin plugin)
		{
			this.plugin = plugin;
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			Timing.RunCoroutine(SetRoleCoroutine(ev.Player));
		}

		public static IEnumerator<float> SetRoleCoroutine([NotNull] Player player)
		{
			PlayerPerkFetcher.PlayerPerks perks = PlayerPerkFetcher.GetPlayerPerks(player);

			PlayerPerkFetcher.UpdatePlayerTag(player, perks, TDHPlugin.Singleton.tagPrefix, TDHPlugin.Singleton.tagColour);

			yield break;
		}
	}
}