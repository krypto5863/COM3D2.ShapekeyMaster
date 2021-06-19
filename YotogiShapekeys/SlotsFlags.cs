using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShapekeyMaster
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
		private static readonly Dictionary<DisableWhenEquipped, TBody.SlotID> SlotToSlotList = new Dictionary<DisableWhenEquipped, TBody.SlotID>()
		{
			{ DisableWhenEquipped.Wear,TBody.SlotID.wear },
			{ DisableWhenEquipped.Skirt,TBody.SlotID.skirt },
			{ DisableWhenEquipped.OnePiece,TBody.SlotID.onepiece },
			{ DisableWhenEquipped.SwimSuit,TBody.SlotID.mizugi },
			{ DisableWhenEquipped.Underwear,TBody.SlotID.panz },
			{ DisableWhenEquipped.Bra,TBody.SlotID.bra },
			{ DisableWhenEquipped.Stockings,TBody.SlotID.stkg },
			{ DisableWhenEquipped.Shoes,TBody.SlotID.shoes },
			{ DisableWhenEquipped.Headset,TBody.SlotID.headset },
			{ DisableWhenEquipped.Hat,TBody.SlotID.accHead },
			{ DisableWhenEquipped.Glove,TBody.SlotID.glove },
			{ DisableWhenEquipped.Arm,TBody.SlotID.accUde },
			{ DisableWhenEquipped.Back,TBody.SlotID.accSenaka }
		};

		internal static bool CheckIfSlotDisableApplies(ShapeKeyEntry entry, Maid maid) 
		{
			if (entry.WhenAll)
			{
			//When all is equipped disable key
				foreach (DisableWhenEquipped slot in Enum.GetValues(typeof(DisableWhenEquipped)).Cast<DisableWhenEquipped>())
				{
					//If a maid has any clothing item of a type unequipped, we return false to disable.
					if (entry.SlotFlags.HasFlag(slot) && !maid.body0.GetMask(SlotToSlotList[slot]) && maid.body0.GetSlotLoaded(SlotToSlotList[slot]))
					{
						return entry.DisableWhen;
					}
				}

				foreach (string menu in entry.MenuFileConditionals.Values)
				{
					var TBodySkins = maid.body0.goSlot.Select(tbody => tbody).Where(str => str.m_mp != null && str.m_mp.strFileName.ToLower().Equals(menu.ToLower()));

					if (TBodySkins.Count() == 0) 
					{
						return entry.DisableWhen;
					}

					foreach(TBodySkin skin in TBodySkins) 
					{
						if (!maid.body0.GetMask(skin.SlotId))
						{
							return entry.DisableWhen;
						}
					}
				}

				return !entry.DisableWhen;
			}
			else 
			{
			//When any is equipped disable key
				foreach (DisableWhenEquipped slot in Enum.GetValues(typeof(DisableWhenEquipped)).Cast<DisableWhenEquipped>())
				{
					//If a maid has any clothing item of a type equipped, we return true to disable.
					if (entry.SlotFlags.HasFlag(slot) && maid.body0.GetMask(SlotToSlotList[slot]) && maid.body0.GetSlotLoaded(SlotToSlotList[slot]))
					{
						return !entry.DisableWhen;
					}
				}

				foreach (string menu in entry.MenuFileConditionals.Values)
				{
					var TBodySkins = maid.body0.goSlot.Select(tbody => tbody).Where(str => str.m_mp != null && str.m_mp.strFileName.ToLower().Equals(menu.ToLower()));

					foreach (TBodySkin skin in TBodySkins) 
					{
						if (maid.body0.GetMask(skin.SlotId))
						{
							return !entry.DisableWhen;
						}
					}
				}

				return entry.DisableWhen;
			}
		}
	}
}
