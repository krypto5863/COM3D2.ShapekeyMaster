using System;

namespace ShapekeyMaster
{
	internal class SMEventsAndArgs
	{
		//Cleaner for instancing.
		public class MorphEventArgs : EventArgs
		{
			public TMorph Morph { get; private set; }
			public bool Creation { get; private set; }

			public MorphEventArgs(TMorph changedMorph, bool wasCreated = true)
			{
				Morph = changedMorph;
				Creation = wasCreated;
			}
		}

		public class ExcitementChangeEvent : EventArgs
		{
			public string Maid { get; private set; }
			public int Excitement { get; private set; }

			public ExcitementChangeEvent(string maid, int excite)
			{
				Maid = maid;
				Excitement = excite;
			}
		}

		public class ClothingMaskChangeEvent : EventArgs
		{
			public string Maid { get; private set; }

			public ClothingMaskChangeEvent(string maid)
			{
				Maid = maid;
			}
		}
	}
}