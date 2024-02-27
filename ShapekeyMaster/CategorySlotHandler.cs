using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeKeyMaster
{
	[Flags]
	internal enum DisableWhenEquipped
	{
		Wear = 1,
		Skirt = 2,
		OnePiece = 4,
		SwimSuit = 8,
		Underwear = 16,
		Bra = 32,
		Stockings = 64,
		Shoes = 128,
		Headset = 256,
		Hat = 512,
		Glove = 1024,
		Arm = 2048,
		Back = 4096
	}

	internal class SlotChecker
	{
		public static readonly Dictionary<DisableWhenEquipped, TBody.SlotID> SlotToSlotList = new Dictionary<DisableWhenEquipped, TBody.SlotID>
		{
			//Why do you parse the enum?? Because it's way more update proof as the enum number won't slide if they add or subtract values.
			{ DisableWhenEquipped.Wear,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"wear") },
			{ DisableWhenEquipped.Skirt,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"skirt") },
			{ DisableWhenEquipped.OnePiece,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"onepiece") },
			{ DisableWhenEquipped.SwimSuit,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"mizugi") },
			{ DisableWhenEquipped.Underwear,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"panz") },
			{ DisableWhenEquipped.Bra,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"bra") },
			{ DisableWhenEquipped.Stockings,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"stkg") },
			{ DisableWhenEquipped.Shoes,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"shoes") },
			{ DisableWhenEquipped.Headset,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"headset") },
			{ DisableWhenEquipped.Hat,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"accHat") },
			{ DisableWhenEquipped.Glove,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"glove") },
			{ DisableWhenEquipped.Arm,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"accUde") },
			{ DisableWhenEquipped.Back,(TBody.SlotID)Enum.Parse(typeof(TBody.SlotID),"accSenaka") }
		};

		internal static bool CheckIfSlotDisableApplies(ShapeKeyEntry entry, Maid maid)
		{
#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug("Running slot check...");
#endif

			if (entry.WhenAll)
			{
#if (DEBUG)
				ShapeKeyMaster.pluginLogger.LogDebug("WhenAll");
#endif
				//When all is equipped disable key
				foreach (var slot in Enum.GetValues(typeof(DisableWhenEquipped)).Cast<DisableWhenEquipped>())
				{
#if (DEBUG)
					ShapeKeyMaster.pluginLogger.LogDebug($"Checking {slot}");
#endif

					//If a maid has any clothing item of a type unequipped, we return false to disable.
					if (!entry.SlotFlags.HasFlag(slot)
						|| maid.body0.GetMask(SlotToSlotList[slot])
						|| !maid.body0.GetSlotLoaded(SlotToSlotList[slot]))
					{
						continue;
					}

					if (entry.IgnoreCategoriesWithShapekey && maid.DoesCategoryContainKey(SlotToSlotList[slot], entry.ShapeKey))
					{
						continue;
					}

					return entry.DisableWhen;
				}
#if (DEBUG)
				ShapeKeyMaster.pluginLogger.LogDebug("Checking menus");
#endif
				foreach (var menu in entry.MenuFileConditionals.Values)
				{
#if (DEBUG)
					ShapeKeyMaster.pluginLogger.LogDebug($"Checking {menu}");
#endif
					var bodySkins = maid.body0
						.FetchGoSlot()
						.Select(tbody => tbody)
						.Where(str => str.m_mp != null && str.m_mp.strFileName.Contains(menu, StringComparison.OrdinalIgnoreCase))
						.ToArray();

					if (!bodySkins.Any())
					{
						return entry.DisableWhen;
					}

					if (bodySkins.Any(skin => !maid.body0.GetMask(skin.SlotId)))
					{
						return entry.DisableWhen;
					}
				}

				return !entry.DisableWhen;
			}
#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug("WhenAny");
#endif

			//When any is equipped disable key
			foreach (var slot in Enum.GetValues(typeof(DisableWhenEquipped)).Cast<DisableWhenEquipped>())
			{
#if (DEBUG)
				ShapeKeyMaster.pluginLogger.LogDebug($"Checking {slot}");
#endif
				//If a maid has any clothing item of a type equipped, we return true to disable.
				if (entry.SlotFlags.HasFlag(slot)
					&& maid.body0.GetMask(SlotToSlotList[slot])
					&& maid.body0.GetSlotLoaded(SlotToSlotList[slot])
					&& (entry.IgnoreCategoriesWithShapekey == false || maid.DoesCategoryContainKey(SlotToSlotList[slot], entry.ShapeKey) == false))
				{
					return !entry.DisableWhen;
				}
			}

			foreach (var menu in entry.MenuFileConditionals.Values)
			{
#if (DEBUG)
				ShapeKeyMaster.pluginLogger.LogDebug($"Checking {menu}");
#endif
				var bodySkins = maid.body0
					.FetchGoSlot()
					.Select(tbody => tbody)
					.Where(str => str.m_mp != null && str.m_mp.strFileName.Contains(menu, StringComparison.OrdinalIgnoreCase));

				if (bodySkins.Any(skin => maid.body0.GetMask(skin.SlotId)))
				{
					return !entry.DisableWhen;
				}
			}

			return entry.DisableWhen;
		}
	}
}