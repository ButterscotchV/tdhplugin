using JetBrains.Annotations;
using Smod2.API;

namespace TDHPlugin.Utils
{
	public class PlayerPerkFetcher
	{
		public class PlayerPerks
		{
			[NotNull] public readonly string steamId;

			public bool tag;
			public bool customTag;
			public bool reservedSlot;

			public PlayerPerks([NotNull] string steamId)
			{
				this.steamId = steamId;
			}

			public static PlayerPerks FromRoleIds([NotNull] string steamId, [NotNull] string[] roleIds)
			{
				PlayerPerks perks = new PlayerPerks(steamId);

				foreach (string roleId in roleIds)
				{
					if (!TDHPlugin.Singleton.rolePerks.TryGetValue(roleId, out string rolePerks)) continue;

					foreach (char rolePerk in rolePerks)
					{
						switch (rolePerk)
						{
							case 'T':
								perks.tag = true;
								break;
							case 'C':
								perks.customTag = true;
								break;
							case 'R':
								perks.reservedSlot = true;
								break;
						}
					}
				}

				return perks;
			}
		}

		public static PlayerPerks GetPlayerPerks([NotNull] string steamId)
		{
			string[] userRoles = TDHPlugin.GetUserRoleIds(steamId);
			return PlayerPerks.FromRoleIds(steamId, userRoles);
		}

		public static PlayerPerks GetPlayerPerks([NotNull] Player player)
		{
			return GetPlayerPerks(player.SteamId);
		}

		public static void UpdatePlayerTag([NotNull] Player player, [NotNull] PlayerPerks perks, string tagPrefix, string tagColour)
		{
			if (!perks.tag) return;

			if (perks.customTag)
			{
				string customTag = TDHPlugin.GetUserCustomTag(player.SteamId);

				if (!string.IsNullOrEmpty(customTag))
				{
					player.SetRank(color: tagColour, text: string.IsNullOrEmpty(tagPrefix) ? customTag : $"{tagPrefix}{(tagPrefix.EndsWith(" ") ? "" : " ")}{customTag}");
				}
				else if (!string.IsNullOrEmpty(tagPrefix))
				{
					player.SetRank(color: tagColour, text: tagPrefix.Trim());
				}
			}
			else if (!string.IsNullOrEmpty(tagPrefix))
			{
				player.SetRank(color: tagColour, text: tagPrefix.Trim());
			}
		}

		public static void UpdatePlayerSlot([NotNull] Player player, [NotNull] PlayerPerks perks)
		{
			if (perks.reservedSlot)
				ReservedSlotManager.AddReservedSlot(player);
			else
				ReservedSlotManager.RemoveReservedSlot(player);
		}
	}
}