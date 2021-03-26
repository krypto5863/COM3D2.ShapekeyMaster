using System.Collections;
using UnityEngine;

namespace ShapekeyMaster
{
	internal class ShapeKeyEntry
	{

		private IEnumerator Animator;

		public int Id { get; set; }
		public string EntryName { get; set; }
		public bool Enabled { get; set; }

		public bool SetEnabled(bool value)
		{
			if (value != Enabled)
			{
				Enabled = value;
				RunUpdate();
			}

			return value;
		}

		public bool IsProp { get; set; }
		public bool AnimateWithExcitement { get; set; }
		public bool SetAnimateWithExcitement(bool value)
		{
			if (value != AnimateWithExcitement)
			{
				AnimateWithExcitement = value;
				RunUpdate();
			}
			return value;
		}
		public float ExcitementMax { get; set; }
		public void SetExcitementMax(float value)
		{
			if (value != ExcitementMax)
			{
				ExcitementMax = value;
				RunUpdate();
			}
		}
		public float ExcitementMin { get; set; }
		public void SetExcitementMin(float value)
		{
			if (value != ExcitementMin)
			{
				ExcitementMin = value;
				RunUpdate();
			}
		}
		public bool AnimateWithOrgasm { get; set; }
		public bool SetAnimateWithOrgasm(bool value)
		{
			if (value != AnimateWithOrgasm)
			{
				AnimateWithOrgasm = value;
				RunUpdate();
			}
			return value;
		}
		public bool GetAnimateWithOrgasm()
		{
			//return AnimateWithOrgasm;
			return false;
		}
		public bool Animate { get; set; }
		public bool SetAnimate(bool value)
		{

			if (value != Animate)
			{

				Animate = value;

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

			return value;
		}
		public string AnimationRate { get; set; }
		public float GetAnimationRate()
		{

			if (float.TryParse(AnimationRate, out float val))
			{
				return val;
			}

			return 0;
		}
		public string SetAnimationRate(string value)
		{
			if (value != AnimationRate)
			{
				AnimationRate = value;
			}
			return value;
		}
		public string AnimationPoll { get; set; }
		public float GetAnimationPoll()
		{

			if (float.TryParse(AnimationPoll, out float val))
			{
				return val;
			}

			return 0.01633f;
		}
		public string SetAnimationPoll(string value)
		{
			if (value != AnimationPoll)
			{
				AnimationPoll = value;
			}
			return value;
		}
		public float AnimationMaximum { get; set; }
		public float SetAnimationMaximum(float value)
		{
			if (value != AnimationMaximum)
			{
				AnimationMaximum = value;
			}
			return value;
		}
		public float AnimationMinimum { get; set; }
		public float SetAnimationMinimum(float value)
		{
			if (value != AnimationMinimum)
			{
				AnimationMinimum = value;
			}
			return value;
		}
		public float Deform { get; set; }
		public void SetDeform(float value)
		{
			if (value != Deform)
			{
				Deform = value;
				RunUpdate();
			}
		}
		public float DeformMax { get; set; }
		public void SetDeformMax(float value)
		{
			if (value != DeformMax)
			{
				DeformMax = value;
				RunUpdate();
			}
		}
		public float DeformMin { get; set; }
		public void SetDeformMin(float value)
		{
			if (value != DeformMin)
			{
				DeformMin = value;
				RunUpdate();
			}
		}
		public string ShapeKey { get; set; }
		public void SetShapeKey(string value)
		{
			if (value != ShapeKey)
			{
				ShapeKey = value;
				RunUpdate();
			}
		}
		public string Maid { get; set; }
		public void SetMaid(string value)
		{
			if (value != Maid)
			{
				Maid = value;
				RunUpdate();
			}
		}
		public bool Collapsed { get; set; }

		private readonly bool constructordone = false;
		public ShapeKeyEntry(int id, bool Prop = false)
		{
			this.Id = id;
			EntryName = "New Entry #" + id.ToString();
			Enabled = true;
			Deform = 0;
			ShapeKey = "";
			IsProp = Prop;

			if (Prop == false)
			{
				AnimateWithExcitement = false;
				ExcitementMax = 300.0F;
				ExcitementMin = 0.0F;
				DeformMax = 100;
				DeformMin = 0;
				Maid = "";

				Animate = false;
				AnimationMaximum = 100;
				AnimationMinimum = 0;
				AnimationRate = "1";
				AnimationPoll = "0.01633";
			}
			Collapsed = true;

			Animator = AnimateCoRoute();

			Main.@this.StartCoroutine(Animator);

			constructordone = true;
		}
		private void RunUpdate()
		{
#if (DEBUG)
			Debug.Log($"Change was detected in the shapekeys. Calling update.");
#endif
			if (constructordone)
			{
#if (DEBUG)
				Debug.Log($"Constructor found done. Calling.");
#endif
				ShapekeyFetcherSetter.RunSingleEntry(this);
				//ShapekeyFetcherSetter.RunSingleShapekey(this);

#if (DEBUG)
				Debug.Log($"Finished update.");
#endif
			}
		}
		private IEnumerator AnimateCoRoute()
		{
			bool reverse = false;

			yield return new WaitForEndOfFrame();

			while (true)
			{
				if (Animate == true)
				{
					if (Enabled && HelperClasses.IsMaidActive(Maid))
					{
						if (Deform >= AnimationMaximum)
						{
							reverse = true;
						}
						else if (Deform <= AnimationMinimum)
						{
							reverse = false;
						}

						if (reverse)
						{
							Deform -= GetAnimationRate();
						}
						else
						{
							Deform += GetAnimationRate();
						}

						if (Deform > AnimationMaximum)
						{
							Deform = AnimationMaximum;
						}
						else if (Deform < AnimationMinimum)
						{
							Deform = AnimationMinimum;
						}

#if (DEBUG)
						Debug.Log($"Changed deform value for shapekey entry {EntryName}");
#endif

						ShapekeyFetcherSetter.RunSingleEntry(this);

						yield return new WaitForSecondsRealtime(GetAnimationPoll());
					}
					else
					{
#if (DEBUG)
						Debug.Log($"Maid isn't active or shapekey entry disabled. Waiting two seconds and rechecking...");
#endif

						yield return new WaitForSecondsRealtime(2);
						//SpinWait.SpinUntil(new Func<bool>(() => HelperClasses.IsMaidActive(Maid)));
					}
				}
				else
				{

#if (DEBUG)
					Debug.Log($"Animate was false. Exiting the coroutine.");
#endif

					break;

					/*SpinWait.SpinUntil(new Func<bool>(() => {
						if (Animate == true)
						{
							return true;
						}
						return false;
					}));*/
				}
			}
		}
	}
}
