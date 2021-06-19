using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShapekeyMaster
{
	internal class ShapeKeyEntry
	{
		private IEnumerator Animator;
		public Guid Id { get; set; }
		public string EntryName { get; set; }

		private bool enabled;
		public bool Enabled
		{
			set
			{
				if (value != enabled)
				{
					enabled = value;
					RunUpdate();
				}
			}
			get 
			{
				return enabled;
			}
		}

		private bool animateWithExcitement;
		public bool AnimateWithExcitement
		{
			set
			{
				if (value != animateWithExcitement)
				{
					animateWithExcitement = value;
					RunUpdate();
				}
			} get 
			{
				return animateWithExcitement;
			}
		}
		private float excitementMax;
		public float ExcitementMax
		{
			get { return excitementMax; }
			set
			{
				if (value != excitementMax)
				{
					excitementMax = value;
					RunUpdate();
				}
			}
		}
		private float excitementMin;
		public float ExcitementMin
		{
			get { return excitementMin; }

			set
			{
				if (value != excitementMin)
				{
					excitementMin = value;
					RunUpdate();
				}
			}
		}
		private bool animateWithOrgasm;
		public bool AnimateWithOrgasm
		{
			get 
			{
				return false;
			}
			set
			{
				if (value != animateWithOrgasm)
				{
					animateWithOrgasm = value;
					RunUpdate();
				}
			}
		}
		private bool animate;
		public bool Animate
		{
			set
			{
				if (value != animate)
				{
					animate = value;

					if (value == false)
					{
						Main.@this.StopCoroutine(Animator);
					}
					else
					{
						Animator = AnimateCoRoute();
						Main.@this.StartCoroutine(Animator);
					}
				}
			} get 
			{
				return animate;
			}
		}
		private string animationRate;
		public string AnimationRate
		{
			get
			{
				if (float.TryParse(animationRate, out float val))
				{
					return animationRate;
				}

				return "0";
			}
			set 
			{
				animationRate = value.ToString();
			}
		}
		public float AnimationRateFloat
		{
			get
			{
				if (float.TryParse(animationRate, out float val))
				{
					return val;
				}

				return 0;
			}
			set
			{
				animationRate = value.ToString();
			}
		}
		private string animationPoll;
		public string AnimationPoll
		{
			get
			{
				return animationPoll;
			} set 
			{
				animationPoll = value;
			}
		}
		public float AnimationPollFloat
		{
			get
			{
				if (float.TryParse(animationPoll, out float val))
				{
					return val;
				}

				return 0.01633f;
			}
			set
			{
				animationPoll = value.ToString();
			}
		}
		private float animationMaximum;
		public float AnimationMaximum
		{
			get { return animationMaximum; }
			set
			{
				animationMaximum = value;
			}
		}
		private float animationMinimum;
		public float AnimationMinimum
		{
			get { return animationMinimum; }
			set
			{
				animationMinimum = value;
			}
		}
		private float deform;
		public float Deform
		{
			get
			{
				return deform;
			}
			set
			{
				if (value != deform)
				{
					deform = value;
					RunUpdate();
				}
			}
		}
		private float disableddeform;
		public float DisabledDeform
		{
			get
			{
				return disableddeform;
			}
			set
			{
				if (value != disableddeform)
				{
					disableddeform = value;
					RunUpdate();
				}
			}
		}
		private float deformMax;
		public float DeformMax
		{
			get { return deformMax;}
			set
			{
				if (value != deformMax)
				{
					deformMax = value;
					RunUpdate();
				}
			}
		}
		private float deformMin;
		public float DeformMin
		{
			get { return deformMin; }
			set
			{
				if (value != deformMin)
				{
					deformMin = value;
					RunUpdate();
				}

			}		
		}

		private string shapeKey;
		public string ShapeKey
		{
			get { return shapeKey; }
			set
			{
				if (value != shapeKey)
				{
					shapeKey = value;
					RunUpdate();
				}
			}
		}
		private string maid;
		public string Maid
		{
			get
			{ return maid; }
			set
			{
				if (value != maid)
				{
					maid = value;
					RunUpdate();
					UI.SKDatabase.RefreshSubDictionaries();
				}
			}
		}


		private bool conditionalsToggle;
		public bool ConditionalsToggle 
		{
			get 
			{ return conditionalsToggle; } 
			set 
			{
				if (value != conditionalsToggle) 
				{
					conditionalsToggle = value;
					RunUpdate();
				}
			} 
		}
		private bool disableWhen;
		public bool DisableWhen
		{
			get
			{ return disableWhen; }
			set
			{
				if (value != disableWhen)
				{
					disableWhen = value;
					RunUpdate();
				}
			}
		}
		private bool whenAll;
		public bool WhenAll
		{
			get
			{ return whenAll; }
			set
			{
				if (value != whenAll)
				{
					whenAll = value;
					RunUpdate();
				}
			}
		}
		private DisableWhenEquipped slotFlags;
		public DisableWhenEquipped SlotFlags
		{
			get
			{ return slotFlags; }
			set
			{
				if (value != slotFlags)
				{
					slotFlags = value;
					RunUpdate();
				}
			}
		}
		public Dictionary<Guid, string> MenuFileConditionals { get; set; }
		public bool Collapsed { get; set; }

		private readonly bool constructordone = false;
		public ShapeKeyEntry(Guid id, string maid = "")
		{
			this.Id = id;
			EntryName = "New Entry #" + Id.ToString();
			enabled = true;
			deform = 0;
			shapeKey = "";

			animateWithExcitement = false;
			excitementMax = 300.0F;
			excitementMin = 0.0F;
			deformMax = 100;
			deformMin = 0;
			this.maid = maid;

			animate = false;
			animationMaximum = 100;
			animationMinimum = 0;
			animationRate = "1";
			animationPoll = "0.01633";

			Collapsed = true;

			MenuFileConditionals = new Dictionary<Guid, string>();

			Animator = AnimateCoRoute();

			Main.@this.StartCoroutine(Animator);

			constructordone = true;
		}
		private void RunUpdate()
		{
#if (DEBUG)
			Main.logger.LogDebug($"Change was detected in the shapekeys. Calling update.");
#endif
			if (constructordone)
			{
#if (DEBUG)
				Main.logger.LogDebug($"Constructor found done. Calling.");
#endif
				//ShapekeyFetcherSetter.RunSingleEntry(this);
				//ShapekeyFetcherSetter.RunSingleShapekey(this);

				ShapekeyUpdate.UpdateKeys(maid);

#if (DEBUG)
				Main.logger.LogDebug($"Finished update.");
#endif
			}
		}
		private IEnumerator AnimateCoRoute()
		{
			bool reverse = false;

			yield return new WaitForEndOfFrame();

			while (true)
			{
				if (animate == true)
				{
					if (enabled && HelperClasses.IsMaidActive(maid))
					{
						if (deform >= animationMaximum)
						{
							reverse = true;
						}
						else if (deform <= animationMinimum)
						{
							reverse = false;
						}

						if (reverse)
						{
							deform -= AnimationRateFloat;
						}
						else
						{
							deform += AnimationRateFloat;
						}

						if (deform > animationMaximum)
						{
							deform = animationMaximum;
						}
						else if (deform < animationMinimum)
						{
							deform = animationMinimum;
						}

#if (DEBUG)
						Main.logger.LogDebug($"Changed deform value for shapekey entry {EntryName}");
#endif

						ShapekeyUpdate.UpdateKeys(maid, true);

						yield return new WaitForSecondsRealtime(AnimationPollFloat);
					}
					else
					{
#if (DEBUG)
						Main.logger.LogDebug($"Maid isn't active or shapekey entry disabled. Waiting two seconds and rechecking...");
#endif

						yield return new WaitForSecondsRealtime(2);
						//SpinWait.SpinUntil(new Func<bool>(() => HelperClasses.IsMaidActive(Maid)));
					}
				}
				else
				{

#if (DEBUG)
					Main.logger.LogDebug($"Animate was false. Exiting the coroutine.");
#endif

					break;
				}
			}
		}
	}
}
