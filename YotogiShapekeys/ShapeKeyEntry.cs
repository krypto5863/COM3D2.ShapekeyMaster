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

					if (animate && !enabled)
					{
						if (Animator != null)
						{
							Main.@this.StopCoroutine(Animator);
						}
					}
					else if (animate)
					{
						Animator = AnimateCoRoute(this);
						Main.@this.StartCoroutine(Animator);
					}

					RunUpdate();
				}
			}
			get => enabled;
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

					var instance = this;

					//We figured it would be much much more efficient to just have our shapekey entries themselves be individually notified when they want ExcitementChanged events instead of just updating every key.
					if (animateWithExcitement)
					{
						HarmonyPatchers.ExcitementChange += (s, e) => instance.RunUpdate(null, true);
					}
					else
					{

						HarmonyPatchers.ExcitementChange -= (s, e) => instance.RunUpdate(null, true);
					}
				}
			}
			get => animateWithExcitement;
		}
		private float excitementMax;
		public float ExcitementMax
		{
			get => excitementMax;
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
			get => excitementMin;

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
			get => false;
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
						Animator = AnimateCoRoute(this);
						Main.@this.StartCoroutine(Animator);
					}
				}
			}
			get => animate;
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
			get => animationPoll;
			set
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
			get => animationMaximum;
			set
			{
				animationMaximum = value;
			}
		}
		private float animationMinimum;
		public float AnimationMinimum
		{
			get => animationMinimum;
			set
			{
				animationMinimum = value;
			}
		}
		private float deform;
		public float Deform
		{
			get => deform;
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
			get => disableddeform;
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
			get => deformMax;
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
			get => deformMin;
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
			get => shapeKey;
			set
			{
				if (value != shapeKey)
				{
					shapeKey = value;
					RunUpdate();
					UI.SKDatabase.RefreshSubDictionaries();
				}
			}
		}
		private string maid;
		public string Maid
		{
			get => maid;
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
			get => conditionalsToggle;
			set
			{
				if (value != conditionalsToggle)
				{
					conditionalsToggle = value;
					RunUpdate();

					var instance = this;

					if (conditionalsToggle)
					{
						HarmonyPatchers.ClothingMaskChange += (s, e) => instance.RunUpdate((e as SMEventsAndArgs.ClothingMaskChangeEvent).Maid, true);
					}
					else
					{
						HarmonyPatchers.ClothingMaskChange -= (s, e) => instance.RunUpdate((e as SMEventsAndArgs.ClothingMaskChangeEvent).Maid, true);
					}
				}
			}
		}
		private bool disableWhen;
		public bool DisableWhen
		{
			get => disableWhen;
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
			get => whenAll;
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
			get => slotFlags;
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

		private readonly bool constructordone;
		public ShapeKeyEntry(Guid id, string maid = "")
		{
			this.Id = id;
			EntryName = "";
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

			animateWithOrgasm = false;
			disableddeform = 0;
			conditionalsToggle = false;
			disableWhen = false;
			whenAll = false;
			slotFlags = 0;

			Collapsed = true;

			MenuFileConditionals = new Dictionary<Guid, string>();

			constructordone = true;

			Animator = null;

			//Main.@this.StartCoroutine(Animator);
		}
		private void RunUpdate(string maid = null, bool avoidWait = false)
		{
#if (DEBUG)
			Main.logger.LogDebug($"Change was detected in the shapekeys. Calling update.");
#endif
			if (constructordone)
			{
#if (DEBUG)
				Main.logger.LogDebug($"Constructor found done. Calling.");
#endif
				if (!String.IsNullOrEmpty(maid) && !String.IsNullOrEmpty(this.maid))
				{
					if (!maid.Equals(maid))
					{
						return;
					}
				}

				ShapekeyUpdate.UpdateKeys(this, avoidWait);

#if (DEBUG)
				Main.logger.LogDebug($"Finished update.");
#endif
			}
		}
		private static IEnumerator AnimateCoRoute(ShapeKeyEntry key)
		{
			bool reverse = false;

			yield return new WaitForEndOfFrame();

			while (true)
			{
				if (key.animate == true)
				{
					if (key.enabled && HelperClasses.IsMaidActive(key.maid))
					{
						if (key.deform >= key.animationMaximum)
						{
							reverse = true;
						}
						else if (key.deform <= key.animationMinimum)
						{
							reverse = false;
						}

						if (reverse)
						{
							key.deform -= key.AnimationRateFloat;
						}
						else
						{
							key.deform += key.AnimationRateFloat;
						}

						if (key.deform > key.animationMaximum)
						{
							key.deform = key.animationMaximum;
						}
						else if (key.deform < key.animationMinimum)
						{
							key.deform = key.animationMinimum;
						}

#if (DEBUG)
						Main.logger.LogDebug($"Changed deform value for shapekey entry {EntryName}");
#endif

						ShapekeyUpdate.UpdateKeys(key, true);

						yield return new WaitForSecondsRealtime(key.AnimationPollFloat);
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
