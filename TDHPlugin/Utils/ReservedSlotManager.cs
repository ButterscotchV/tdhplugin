using System.Linq;
using JetBrains.Annotations;
using Smod2.API;

namespace TDHPlugin.Utils
{
	public static class ReservedSlotManager
	{
		public const string SlotCommentPrefix = "<TDHPLUGIN>";

		public static bool ContainsReservedSlot(string steamId)
		{
			return ReservedSlot.GetSlots().Any(slot => slot.SteamID == steamId.Trim());
		}

		public static ReservedSlot GetReservedSlot(string steamId)
		{
			return ReservedSlot.GetSlots().FirstOrDefault(slot => slot.SteamID == steamId.Trim());
		}

		public static ReservedSlot[] GetPluginReservedSlots()
		{
			return ReservedSlot.GetSlots().Where(slot => !string.IsNullOrEmpty(slot.Comment) && slot.Comment.Trim().StartsWith(SlotCommentPrefix)).ToArray();
		}

		public static bool ContainsPluginReservedSlot(string steamId)
		{
			return GetPluginReservedSlots().Any(slot => slot.SteamID == steamId.Trim());
		}

		public static ReservedSlot GetPluginReservedSlot(string steamId)
		{
			return GetPluginReservedSlots().FirstOrDefault(slot => slot.SteamID == steamId.Trim());
		}

		public static void AddReservedSlot([NotNull] Player player)
		{
			if (!ContainsReservedSlot(player.SteamId))
				new ReservedSlot(player.IpAddress, player.SteamId, SlotCommentPrefix + " " + player.Name).AppendToFile();
		}

		public static void RemoveReservedSlot([NotNull] Player player)
		{
			ReservedSlot slot = GetPluginReservedSlot(player.SteamId);
			slot?.RemoveSlotFromFile();
		}
	}
}