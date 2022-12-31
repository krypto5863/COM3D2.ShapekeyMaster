using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
//using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ShapekeyMaster.HelperClasses;
using Debug = UnityEngine.Debug;

namespace ShapekeyMaster
{
	internal class ShapekeyFetcherSetter
	{
		private static List<Maid> maidslist = new List<Maid>();
		private static readonly List<string> YotogiLvls = new List<string>() { "SceneYotogi", "SceneYotogiOld" };
		private static readonly int OrgasmDeform = 1;
		private static readonly bool IsOrgasming = false;
		private static readonly List<ShapekeyChangeForm> ChangeList = new List<ShapekeyChangeForm>();
		private static bool CorouteRunning = false;
		public static void RegisterMaid(Maid maid)
		{
			if (maidslist.Contains(maid) == false)
			{
				maidslist.Add(maid);
			}

			maidslist = maidslist
			.Where(m => m != null)
			.Distinct()
			.ToList();

			RunSingleMaid(maid);
		}
		//Called so external functions can get the maid list in a controlled fashion.
		public static List<Maid> GetMaidsList()
		{
			return maidslist;
		}
		public static void RunAll()
		{
#if (DEBUG)
			Main.logger.LogDebug($"Running on all!");
#endif

			maidslist
			.Where(m => m != null)
			.ToList()
			.ForEach(m => RunSingleMaid(m));

			Main.logger.LogDebug("Finished RunAll");
		}
		//This simple func just fetches all morphs from a maid and sends it to the single morph func
		public static void RunSingleMaid(Maid maid)
		{
#if (DEBUG)
			Stopwatch stop = new Stopwatch();
			stop.Start();
#endif

#if (DEBUG)
			Main.logger.LogDebug($"Working on maid: {maid.status.fullNameJpStyle} with {GetAllMorphsFromMaid(maid).Count()} TMorphs");
#endif
			var changeForms = GetAllMorphsFromMaid(maid)
			 .Select(tm => ShapekeyFetcherSetter.GetTMorphChangeForm(tm))
			 .SelectMany(r => r);

			ProcessChanges(changeForms);

#if (DEBUG)
			stop.Stop();
			Main.logger.LogDebug($"MissionControlSingleEntry finished processing in {stop.Elapsed} ms");
#endif
		}
		public static void RunSingleEntry(ShapeKeyEntry sk)
		{
#if (DEBUG)
			Stopwatch stop = new Stopwatch();
			stop.Start();
			Main.logger.LogDebug($"Calculating single entry...");
#endif

			List<Maid> maids = new List<Maid>();

			if (sk.Maid == "")
			{
				maids = maidslist
					.Where(m => m != null)
					.ToList();
			}
			else
			{
				maids = maidslist
					.Where(m => m != null && m.status.fullNameJpStyle == sk.Maid)
					.ToList();
			}

#if (DEBUG)
			Main.logger.LogDebug($"Fetched {maids.Count} Maids");
#endif

			if (maids.Count <= 0)
			{
				return;
			}

#if (DEBUG)
			Main.logger.LogDebug($"Maids were not zero...");
#endif

			var morphs = maids
			.Select(m => GetAllMorphsFromMaid(m))
			.SelectMany(r => r)
			.Where(m => m.Contains(sk.ShapeKey));

#if (DEBUG)
			Main.logger.LogDebug($"Fetched Morphs");
#endif

			if (morphs.ToList().Count <= 0)
			{
				return;
			}

#if (DEBUG)
			Main.logger.LogDebug($"Morphs were not zero");
#endif

			var changes = morphs
			.Select(m => GetTMorphChangeForm(m))
			.SelectMany(r => r);

#if (DEBUG)
			Main.logger.LogDebug($"Turned morphs into changes");
#endif

			ProcessChanges(changes);

#if (DEBUG)
			Main.logger.LogDebug($"Processed changes...");

			stop.Stop();
			Main.logger.LogDebug($"MissionControlSingleEntry finished processing in {stop.Elapsed} ms");
#endif
		}
		//This function takes a morph and checks every single shapekey entry for one that edits the held morph. If one is found, pushes to modify morph.
		public static List<ShapekeyChangeForm> GetTMorphChangeForm(TMorph m)
		{
#if (DEBUG)
			Main.logger.LogDebug($"Working on TMorph holding this many morphs: {m.MorphCount}");
#endif

			//Filters the list and only returns viable shapekey entries to reduce processing times.
			List<ShapeKeyEntry> templist = UI.SKDatabase.AllShapekeyDictionary.Values
			.Where(s => (s.Maid.Equals("") || m.bodyskin.body.maid.status.fullNameJpStyle.Equals(s.Maid))
			&& m.Contains(s.ShapeKey))
			.ToList();

			List<ShapekeyChangeForm> resultList = new List<ShapekeyChangeForm>();

			//Checks every held shapekeyentry and works as required.
			foreach (ShapeKeyEntry s in templist)
			{
#if (DEBUG)
				Main.logger.LogDebug($"Checking that it isn't a face morph...");
#endif

				if (m.bodyskin.body.Face != null && m.bodyskin.body.Face.morph != null && m.bodyskin.body.Face.morph.Contains(s.ShapeKey))
				{
#if (DEBUG)
					Main.logger.LogDebug($"We are processing a facial key of {s.ShapeKey}");
#endif
					m.bodyskin.body.maid.boMabataki = false;
				}

				int shapekeyIndex = (int)m.hash[s.ShapeKey];

				//If maid is orgasming and orgasms animation is on, we just ask for the orgasm info. Currently unused...
				if (s.GetAnimateWithOrgasm() && IsOrgasming)
				{
					resultList.Add(new ShapekeyChangeForm
					{
						Morph = m,
						Index = shapekeyIndex,
						Deform = OrgasmDeform,
						ShapekeyName = s.ShapeKey
					});

					continue;
				}

				//If the maid is supposed to be animated with excitement and we are in yotogi, we set values according to an excitement deformation calculation.
				if (s.AnimateWithExcitement && YotogiLvls.Contains(SceneManager.GetActiveScene().name))
				{
					float deform = CalculateExcitementDeformation(s, maidslist);

					resultList.Add(new ShapekeyChangeForm
					{
						Morph = m,
						Index = shapekeyIndex,
						Deform = deform,
						ShapekeyName = s.ShapeKey
					});

					continue;
				}

				resultList.Add(new ShapekeyChangeForm
				{
					Morph = m,
					Index = shapekeyIndex,
					Deform = s.Deform,
					ShapekeyName = s.ShapeKey
				});
#if (DEBUG)
				Main.logger.LogDebug($"Creating change form of {shapekeyIndex} {s.Deform} {s.ShapeKey}");
#endif
			}
			return resultList;
		}
		//Even though this goes against my mission control's idea. I wanted to save myself the trouble of rewriting the same code 3 times As such, I decided to go for it. This will be run sync though.
		public static void ProcessChanges(IEnumerable<ShapekeyChangeForm> changes)
		{
#if (DEBUG)
			Main.logger.LogDebug($"Checking if changes are not zero and running coroutine if needed...");
#endif
			if (changes.Count() > 0)
			{
				ChangeList.AddRange(changes.ToList());

				if (CorouteRunning == false)
				{
					Main.@this.StartCoroutine(RunShapekeyChange());
					//RunShapekeyChangeNormal();
				}
			}
		}
		public static IEnumerator RunShapekeyChange()
		{
			CorouteRunning = true;

#if (DEBUG)
			Main.logger.LogDebug($"Started Changer Coroute...");
#endif

			yield return null;

			while (ChangeList.Count > 0)
			{
#if (DEBUG)
				Main.logger.LogDebug($"Running the while...");
#endif

				ShapekeyChangeForm nextKey =
				ChangeList
				.FirstOrDefault(t => t.Morph != null && t.Morph.bodyskin != null && t.Morph.bodyskin.body != null && t.Morph.bodyskin.body.maid.isActiveAndEnabled);

				if (nextKey == null)
				{
#if (DEBUG)
					Main.logger.LogDebug($"It seems no keys were ready to be changed. Waiting for next frame...");
#endif
					yield return null;
					continue;
				}

				if (nextKey.Morph.GetBlendValues(nextKey.Index) != nextKey.Deform)
				{
#if (DEBUG)
					Main.logger.LogDebug($"{nextKey.ShapekeyName} could be changed.");
#endif

					if (nextKey.Deform < 0)
					{
						nextKey.Deform = 0;
					}

#if (DEBUG)
					Main.logger.LogDebug($"Value needs setting! Setting value of {nextKey.Deform} to {nextKey.ShapekeyName}");
#endif
					Main.SetBlendValues(nextKey.Index, (nextKey.Deform / 100f), nextKey.Morph);

					try
					{
						nextKey.Morph.FixBlendValues();
					}
					catch
					{
#if (DEBUG)
						Main.logger.LogWarning($"Had some issues fixing blend values on {nextKey.ShapekeyName}. Be wary.");
#endif
					}

#if (DEBUG)
					Main.logger.LogDebug($"Checking if {nextKey.ShapekeyName} was a face morph and needs updating");
#endif
					if (nextKey.Morph.bodyskin.body.Face != null && nextKey.Morph.bodyskin.body.Face.morph != null && nextKey.Morph.bodyskin.body.Face.morph.Contains(nextKey.ShapekeyName))
					{
						nextKey.Morph.bodyskin.body.Face.morph.FixBlendValues_Face();
					}

					ChangeList.Remove(nextKey);

#if (DEBUG)
					Main.logger.LogDebug($"Done with single shapekey change on {nextKey.ShapekeyName}.");
#endif
				}
				else
				{
					ChangeList.Remove(nextKey);
				}
			}

#if (DEBUG)
			Main.logger.LogDebug($"Coroute is done. Now exiting...");
#endif

			CorouteRunning = false;
		}
		public static void RunShapekeyChangeNormal()
		{
#if (DEBUG)
			Main.logger.LogDebug($"Started Changer Normal...");
#endif

			while (ChangeList.Count > 0)
			{
#if (DEBUG)
				Main.logger.LogDebug($"Running the while...");
#endif

				ShapekeyChangeForm nextKey =
				ChangeList
				.FirstOrDefault(t => t.Morph != null && t.Morph.bodyskin != null && t.Morph.bodyskin.body != null && t.Morph.bodyskin.body.maid.isActiveAndEnabled);

				if (nextKey.Morph.GetBlendValues(nextKey.Index) != nextKey.Deform)
				{
#if (DEBUG)
					Main.logger.LogDebug($"{nextKey.ShapekeyName} could be changed.");
#endif

					if (nextKey.Deform < 0)
					{
						nextKey.Deform = 0;
					}

#if (DEBUG)
					Main.logger.LogDebug($"Value needs setting! Setting value of {nextKey.Deform} to {nextKey.ShapekeyName}");
#endif
					Main.SetBlendValues(nextKey.Index, (nextKey.Deform / 100f), nextKey.Morph);

					try
					{
						nextKey.Morph.FixBlendValues();
					}
					catch
					{
#if (DEBUG)
						Main.logger.LogWarning($"Had some issues fixing blend values on {nextKey.ShapekeyName}. Be wary.");
#endif
					}

#if (DEBUG)
					Main.logger.LogDebug($"Checking if {nextKey.ShapekeyName} was a face morph and needs updating");
#endif
					if (nextKey.Morph.bodyskin.body.Face != null && nextKey.Morph.bodyskin.body.Face.morph != null && nextKey.Morph.bodyskin.body.Face.morph.Contains(nextKey.ShapekeyName))
					{
						nextKey.Morph.bodyskin.body.Face.morph.FixBlendValues_Face();
					}

					ChangeList.Remove(nextKey);

#if (DEBUG)
					Main.logger.LogDebug($"Done with single shapekey change on {nextKey.ShapekeyName}.");
#endif
				}
				else
				{
					ChangeList.Remove(nextKey);
				}
			}

#if (DEBUG)
			Main.logger.LogDebug($"Coroute is done. Now exiting...");
#endif
		}
	}
}