using Newtonsoft.Json;
using ShapeKeyMaster.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ShapeKeyMaster
{
	internal class ShapeKeyEntry : ICloneable
	{
		private IEnumerator animator;
		public Guid Id { get; }
		public string EntryName { get; set; }
		public DateTime CreationDate { get; set; }
		[JsonIgnore]
		private int? orderNum;
		public int? OrderNum {
			get => orderNum;
			set 
			{
				if (value == null)
				{
					orderNum = null;
					return;
				}

				if (orderNum == null)
				{
					orderNum = value;
					return;
				}

				orderNum = Ui.SkDatabase.Reorder_Insert(this, value.Value);
			} 
		}
		[JsonIgnore]
		public int? OrderNumTmp { get; set; }

		private int enabled;
		/// <summary>
		/// 0 is ignored, 1 is zeroed, 2 is on.
		/// </summary>
		public int Enabled
		{
			set
			{
				if (value == enabled)
				{
					return;
				}

				if (value > 2)
				{
					value = 0;
				}
				else if (value < 0)
				{
					value = 2;
				}

				enabled = value;

				if (animate)
				{
					switch (enabled)
					{
						case 0:
						case 1 when animator != null:
							ShapeKeyMaster.Instance.StopCoroutine(animator);
							animator = null;
							break;
						case 2 when animator == null:
							animator = AnimateCoRoute(this);
							ShapeKeyMaster.Instance.StartCoroutine(animator);
							break;
					}
				}

				RunUpdate();
			}
			get => enabled;
		}

		private bool animateWithExcitement;

		public bool AnimateWithExcitement
		{
			set
			{
				if (value == animateWithExcitement)
				{
					return;
				}

				animateWithExcitement = value;
				RunUpdate();

				//We figured it would be much much more efficient to just have our shapekey entries themselves be individually notified when they want ExcitementChanged events instead of just updating every key.
				if (animateWithExcitement)
				{
					HarmonyPatchers.ExcitementChange += HarmonyPatchersOnExcitementChange;
				}
				else
				{
					HarmonyPatchers.ExcitementChange -= HarmonyPatchersOnExcitementChange;
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
				if (value == excitementMax)
				{
					return;
				}

				excitementMax = value;
				RunUpdate();
			}
		}

		private float excitementMin;

		public float ExcitementMin
		{
			get => excitementMin;

			set
			{
				if (value == excitementMin)
				{
					return;
				}

				excitementMin = value;
				RunUpdate();
			}
		}

		private bool animateWithOrgasm;

		public bool AnimateWithOrgasm
		{
			get => false;
			set
			{
				if (value == animateWithOrgasm)
				{
					return;
				}

				animateWithOrgasm = value;
				RunUpdate();
			}
		}

		private bool animate;

		public bool Animate
		{
			set
			{
				if (value == animate)
				{
					return;
				}

				animate = value;

				if (value == false)
				{
					ShapeKeyMaster.Instance.StopCoroutine(animator);
					animator = null;
				}
				else
				{
					animator = AnimateCoRoute(this);
					ShapeKeyMaster.Instance.StartCoroutine(animator);
				}
			}
			get => animate;
		}

		private string animationRate;

		public string AnimationRate
		{
			get => float.TryParse(animationRate, out _) ? animationRate : "0";
			set => animationRate = value;
		}

		public float AnimationRateFloat
		{
			get => float.TryParse(animationRate, out var val) ? val : 0;
			set => animationRate = value.ToString(CultureInfo.InvariantCulture);
		}

		public string AnimationPoll { get; set; }

		public float AnimationPollFloat
		{
			get => float.TryParse(AnimationPoll, out var val) ? val : 0.01633f;
			set => AnimationPoll = value.ToString(CultureInfo.InvariantCulture);
		}

		public float AnimationMaximum { get; set; }
		public float AnimationMinimum { get; set; }

		private float deform;

		public float Deform
		{
			get => deform;
			set
			{
				if (value == deform)
				{
					return;
				}

				deform = value;
#if DEBUG
				ShapeKeyMaster.pluginLogger.LogDebug($"{EntryName}: Deform changed, running update!");
#endif
				RunUpdate();
			}
		}

		private float disableddeform;

		public float DisabledDeform
		{
			get => disableddeform;
			set
			{
				if (value == disableddeform)
				{
					return;
				}

				disableddeform = value;
				RunUpdate();
			}
		}

		private float deformMax;

		public float DeformMax
		{
			get => deformMax;
			set
			{
				if (Math.Abs(value - deformMax) < 0.001f)
				{
					return;
				}

				deformMax = value;
				RunUpdate();
			}
		}

		private float deformMin;

		public float DeformMin
		{
			get => deformMin;
			set
			{
				if (Math.Abs(value - deformMin) < 0.001f)
				{
					return;
				}

				deformMin = value;
				RunUpdate();
			}
		}

		private string shapeKey;

		public string ShapeKey
		{
			get => shapeKey;
			set
			{
				if (value == shapeKey)
				{
					return;
				}

				shapeKey = value;
				RunUpdate();
				Ui.SkDatabase.RefreshSubDictionaries();
			}
		}

		private string maid;

		public string Maid
		{
			get => maid;
			set
			{
				if (value == maid)
				{
					return;
				}

				maid = value;
				RunUpdate();
				Ui.SkDatabase.RefreshSubDictionaries();
			}
		}

		private bool conditionalsToggle;

		public bool ConditionalsToggle
		{
			get => conditionalsToggle;
			set
			{
				if (value == conditionalsToggle)
				{
					return;
				}

				conditionalsToggle = value;
				RunUpdate();

				if (conditionalsToggle)
				{
					HarmonyPatchers.ClothingMaskChange += HarmonyPatchersOnClothingMaskChange;
				}
				else
				{
					HarmonyPatchers.ClothingMaskChange -= HarmonyPatchersOnClothingMaskChange;
				}
			}
		}

		private bool ignoreCategoriesWithShapekey;

		public bool IgnoreCategoriesWithShapekey
		{
			get => ignoreCategoriesWithShapekey;
			set
			{
				if (value == ignoreCategoriesWithShapekey)
				{
					return;
				}

				ignoreCategoriesWithShapekey = value;
				RunUpdate();
			}
		}

		private bool disableWhen;

		public bool DisableWhen
		{
			get => disableWhen;
			set
			{
				if (value == disableWhen)
				{
					return;
				}

				disableWhen = value;
				RunUpdate();
			}
		}

		private bool whenAll;

		public bool WhenAll
		{
			get => whenAll;
			set
			{
				if (value == whenAll)
				{
					return;
				}

				whenAll = value;
				RunUpdate();
			}
		}

		private DisableWhenEquipped slotFlags;

		public DisableWhenEquipped SlotFlags
		{
			get => slotFlags;
			set
			{
				if (value == slotFlags)
				{
					return;
				}

				slotFlags = value;
				RunUpdate();
			}
		}

		public Dictionary<Guid, string> MenuFileConditionals { get; set; }
		public bool Collapsed { get; set; }

		private readonly bool constructorDone;

		public ShapeKeyEntry(Guid id, string maid = "")
		{
			Id = id;
			EntryName = "";
			CreationDate = DateTime.Now;
			enabled = 2;
			deform = 0;
			shapeKey = "";

			animateWithExcitement = false;
			excitementMax = 300.0F;
			excitementMin = 0.0F;
			deformMax = 100;
			deformMin = 0;
			this.maid = maid;

			animate = false;
			AnimationMaximum = 100;
			AnimationMinimum = 0;
			animationRate = "1";
			AnimationPoll = "0.01633";

			animateWithOrgasm = false;
			disableddeform = 0;
			conditionalsToggle = false;
			disableWhen = false;
			whenAll = false;
			slotFlags = 0;

			Collapsed = true;

			MenuFileConditionals = new Dictionary<Guid, string>();

			constructorDone = true;

			animator = null;

			//Main.@this.StartCoroutine(Animator);
		}

		public object Clone()
		{
			var newClone = new ShapeKeyEntry(Guid.NewGuid(), maid)
			{
				enabled = enabled,
				EntryName = EntryName.Clone() as string,
				OrderNum = OrderNum,
				deform = deform,
				shapeKey = shapeKey,
				animateWithExcitement = animateWithExcitement,
				excitementMax = excitementMax,
				excitementMin = excitementMin,
				deformMax = deformMax,
				deformMin = deformMin,
				animate = animate,
				AnimationMaximum = AnimationMaximum,
				AnimationMinimum = AnimationMinimum,
				animationRate = animationRate.Clone() as string,
				AnimationPoll = AnimationPoll.Clone() as string,
				animateWithOrgasm = animateWithOrgasm,
				disableddeform = disableddeform,
				conditionalsToggle = conditionalsToggle,
				disableWhen = disableWhen,
				whenAll = whenAll,
				slotFlags = (DisableWhenEquipped)((int)slotFlags),
				Collapsed = true,
				MenuFileConditionals = new Dictionary<Guid, string>(MenuFileConditionals)
			};

			return newClone;
		}

		private void HarmonyPatchersOnClothingMaskChange(object s, EventArgs e) => RunUpdate((e as SmEventsAndArgs.ClothingMaskChangeEvent)?.Maid, true);

		private void HarmonyPatchersOnExcitementChange(object s, EventArgs e) => RunUpdate(null, true);

		private void RunUpdate(string maidPar = null, bool avoidWait = false)
		{
#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug("Change was detected in the shapekeys. Calling update.");
#endif

			if (!constructorDone)
			{
				return;
			}

#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug("Constructor found done. Calling.");
#endif

			if (!string.IsNullOrEmpty(maidPar) && !string.IsNullOrEmpty(maid))
			{
				if (!maidPar.Equals(maid))
				{
					return;
				}
			}

			ShapeKeyUpdate.UpdateKeys(this, avoidWait);

#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug("Finished update.");
#endif
		}

		private static IEnumerator AnimateCoRoute(ShapeKeyEntry key)
		{
			var reverse = false;

			yield return new WaitForEndOfFrame();

			while (true)
			{
				if (key.animate)
				{
					if (key.enabled == 2 && Extensions.IsMaidActive(key.maid))
					{
						if (key.deform >= key.AnimationMaximum)
						{
							reverse = true;
						}
						else if (key.deform <= key.AnimationMinimum)
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

						if (key.deform > key.AnimationMaximum)
						{
							key.deform = key.AnimationMaximum;
						}
						else if (key.deform < key.AnimationMinimum)
						{
							key.deform = key.AnimationMinimum;
						}

#if (DEBUG)
						ShapeKeyMaster.pluginLogger.LogDebug($"Changed deform value for shapekey entry {key.EntryName}");
#endif

						ShapeKeyUpdate.UpdateKeys(key, true);

						yield return new WaitForSecondsRealtime(key.AnimationPollFloat);
					}
					else
					{
#if (DEBUG)
						ShapeKeyMaster.pluginLogger.LogDebug($"Maid isn't active or shapekey entry disabled. Waiting two seconds and rechecking...");
#endif

						yield return new WaitForSecondsRealtime(2);
						//SpinWait.SpinUntil(new Func<bool>(() => HelperClasses.IsMaidActive(Maid)));
					}
				}
				else
				{
#if (DEBUG)
					ShapeKeyMaster.pluginLogger.LogDebug($"Animate was false. Exiting the coroutine.");
#endif

					break;
				}
			}
		}
	}
}