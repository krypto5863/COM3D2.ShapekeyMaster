using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
		private static int OrgasmDeform = 1;
		private static bool IsOrgasming = false;

		//This func is called in to register a maid into the list of maids. Handy of keeping track of them.
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
		//This can be called externally to purposefully remove a maid from tracking. Currently unused.
		public static void UnRegisterMaid(Maid maid)
		{
			maidslist.Remove(maid);
		}
		//Called so external functions can get the maid list in a controlled fashion.
		public static List<Maid> GetMaidsList()
		{
			return maidslist;
		}
		//The main idea behind MissionControl is to eventually have it drive everything on first thread and simply instruct the worker threads allowing flexibility for when we need to run something on thread 1. As a result, Mission control will have a few versions.
		public async Task MissionControl()
		{
		}
		//Async way of running the run all func.
		public async static void RunAllBackgroundAsync()
		{
			//Made async worker threading an optional. This helps debugging and comparisons.
			if (Main.BackgroundTasks)
			{
				//By pushing our func to a worker thread and calling await, we tell our program to process this lengthy and costly function in another thread, allow other code to process in maint thread and reattain control once processing in the worker thread is done.
				await Task.Factory.StartNew(() => RunAll());
			}
			else
			{
				await RunAll();
			}
		}
		//This func fetches all maids and pushes them to a function that works with every maid in turn.
		public static Task RunAll()
		{
			//yield return new WaitForEndOfFrame();

#if (DEBUG)
			Main.logger.Log($"Running on all!");
#endif

			maidslist
			.Where(m => m != null)
			.ToList()
			.ForEach(m => RunSingleMaid(m));

			Main.logger.Log("Finished RunAll");

			return null;
		}
		//Another async entry point for run on single maids. Only called by external functions, higher functions in the chain don't call these async as they themselves are already running async. This helps with stability.
		public async static void RunSingleMaidBackgroundAsync(Maid maid)
		{
			if (Main.BackgroundTasks)
			{
				await Task.Factory.StartNew(() => RunSingleMaid(maid));
			}
			else
			{
				await RunSingleMaid(maid);
			}
		}
		//This simple func just fetches all morphs from a maid and sends it to the single morph func
		public static Task RunSingleMaid(Maid maid)
		{
			//yield return new WaitForEndOfFrame();
#if (DEBUG)
			Main.logger.Log($"Working on maid: {maid.status.fullNameJpStyle} with {GetAllMorphsFromMaid(maid).Count()} TMorphs");
#endif
			GetAllMorphsFromMaid(maid)
			 .ToList()
			 .ForEach(tm => ShapekeyFetcherSetter.RunSingleTMorph(tm));

			return null;
		}
		//This function takes a morph and checks every single shapekey entry for one that edits the held morph. If one is found, pushes to modify morph.
		public static Task RunSingleTMorph(TMorph m)
		{
#if (DEBUG)
			Main.logger.Log($"Working on TMorph holding this many morphs: {m.MorphCount}");
#endif

			//Filters the list and only returns viable shapekey entries to reduce processing times.
			var templist = UI.ShapeKeys.Values
			.Where(s => s.Enabled == true
			&& s.IsProp == false
			&& (s.Maid.Equals("") || m.bodyskin.body.maid.status.fullNameJpStyle.Equals(s.Maid))
			&& m.Contains(s.ShapeKey))
			.ToList();

			//Checks every held shapekeyentry and works as required.
			foreach (ShapeKeyEntry s in templist)
			{
#if (DEBUG)
				Main.logger.Log($"Checking that it isn't a face morph...");
#endif

				if (m.bodyskin.body.Face != null && m.bodyskin.body.Face.morph.Contains(s.ShapeKey))
				{
#if (DEBUG)
					Main.logger.Log($"We are processing a facial key of {s.ShapeKey}");
#endif
					m.bodyskin.body.maid.boMabataki = false;
				}

				int shapekeyIndex = (int)m.hash[s.ShapeKey];

				//If maid is orgasming and orgasms animation is on, we call a run single on the shapekey with parameters supplied by the orgasm animator.
				if (s.GetAnimateWithOrgasm() && IsOrgasming)
				{
					Main.@this.StartCoroutine(RunShapekeyChange(m, shapekeyIndex, OrgasmDeform));

					continue;
				}

				//If the maid is supposed to be animated with excitement and we are in yotogi, we set values according to an excitement deformation calculation.
				if (s.AnimateWithExcitement && YotogiLvls.Contains(SceneManager.GetActiveScene().name))
				{
					float deform = CalculateExcitementDeformation(s, maidslist);

					Main.@this.StartCoroutine(RunShapekeyChange(m, shapekeyIndex, deform));

					continue;
				}

				//If nothing special is happening,
				Main.@this.StartCoroutine(RunShapekeyChange(m, shapekeyIndex, s.Deform));
			}
			return null;
		}
		public async static Task RunSingleShapekeyBackgroundAsync(ShapeKeyEntry s)
		{
			if (Main.BackgroundTasks)
			{
				await Task.Factory.StartNew(() => RunSingleShapekey(s));
			}
			else
			{
				await RunSingleShapekey(s);
			}

#if (DEBUG)
			Main.logger.Log($"Finished shapekey update");
#endif

			return;
		}
		//This is basically a copy of RunSingleTmorph but made to work with shapekey entries as input instead of morphs.
		public static Task RunSingleShapekey(ShapeKeyEntry s, bool coroute = true)
		{
			if (s.Enabled == false)
			{
				return null;
			}
			if (s.IsProp == true)
			{
				return null;
			}

			List<TMorph> templist = GetAllMorphsFromMaidList(maidslist)
			.Where(m =>
			(s.Maid.Equals("") || m.bodyskin.body.maid.status.fullNameJpStyle.Equals(s.Maid))
			&& m.Contains(s.ShapeKey))
			.ToList();

			foreach (TMorph m in templist)
			{
#if (DEBUG)
				Main.logger.Log($"Working on TMorph holding this many morphs: {m.MorphCount}");
				Main.logger.Log($"Checking that it isn't a face morph...");
#endif

				if (m.bodyskin.body.Face != null && m.bodyskin.body.Face.morph.Contains(s.ShapeKey))
				{
#if (DEBUG)
					Main.logger.Log($"We are processing a facial key of {s.ShapeKey}");
#endif
					m.bodyskin.body.maid.boMabataki = false;
				}

#if (DEBUG)
				Main.logger.Log($"Found! Checking if value needs setting.");
#endif
				int shapekeyIndex = (int)m.hash[s.ShapeKey];

				if (s.AnimateWithExcitement && YotogiLvls.Contains(SceneManager.GetActiveScene().name))
				{
					//RunShapekeyChange(m, shapekeyIndex, 0);

					float deform = CalculateExcitementDeformation(s, maidslist);

					Main.@this.StartCoroutine(RunShapekeyChange(m, shapekeyIndex, deform));
#if (DEBUG)
					Main.logger.Log($"Animating with yotogi...");
#endif
					continue;
				}

				if (coroute)
				{
					Main.@this.StartCoroutine(RunShapekeyChange(m, shapekeyIndex, s.Deform));
				}
				else
				{
					RunShapekeyChange(m, shapekeyIndex, s.Deform);
				}
			}
#if (DEBUG)
			Main.logger.Log($"Finished Single Key Change Update");
#endif
			return null;
		}
		//The actual changes are pushed to a coroutine. Coroutines have the property of always running on the main thread, in time with the main thread so pushing this function to a coroutine from a worker thread ensures safe changes. However when possible. It should be avoided as creating too many coroutines is asking for trouble.
		public static IEnumerator RunShapekeyChange(TMorph m, int shapekeyIndex, float deform)
		{
			yield return new WaitForEndOfFrame();

			//Setting a value of less than 0 can cause the game to crash. As such, we ensure we never set a 0 val.
			if (deform < 0)
			{
				deform = 0;
			}

			if (m.GetBlendValues(shapekeyIndex) != deform)
			{
#if (DEBUG)
				Main.logger.Log($"Value needs setting! Setting value of {deform}");
#endif
				Main.SetBlendValues(shapekeyIndex, (deform / 100f), m);
			}
			m.FixBlendValues();

			if (m.bodyskin.body.maid.boMabataki == false)
			{
				m.bodyskin.body.Face.morph.FixBlendValues_Face();
			}
		}
		/*
		//The animator can be considered something of a manager. It simply calculates orgasms and
		public static IEnumerator OrgasmAnimator()
		{
#if (DEBUG)
			Main.logger.Log($"Orgasm animator has started!!");
#endif

			yield return new WaitForSecondsRealtime(2);

			System.Random rand = new System.Random();
			Stopwatch s = new Stopwatch();
			Stopwatch s2 = new Stopwatch();
			s.Start();
			s2.Start();
			IsOrgasming = true;

			Main.@this.StartCoroutine(UpdateOrgasmKeys());

			while (s.Elapsed < TimeSpan.FromSeconds(4))
			{
				s2.Start();

				while (OrgasmDeform < 100)
				{
					OrgasmDeform += 25;

					if (OrgasmDeform > 100)
					{
						OrgasmDeform = 100;
					}

					yield return new WaitForSeconds(.1f);
				}

				while (OrgasmDeform > 0 && s.Elapsed < TimeSpan.FromSeconds(1))
				{
					OrgasmDeform -= 10;

					if (OrgasmDeform < 0)
					{
						OrgasmDeform = 0;
					}

					yield return new WaitForSeconds(.1f);
				}
				s2.Reset();
			}
			s.Stop();

#if (DEBUG)
			Main.logger.Log($"Finishing...");
#endif

			while (OrgasmDeform > 0)
			{
#if (DEBUG)
				Main.logger.Log($"Reducing {OrgasmDeform}");
#endif

				OrgasmDeform -= 10;

				yield return new WaitForSeconds(.10f);
			}

			IsOrgasming = false;

#if (DEBUG)
			Main.logger.Log($"Orgasm animator has finished: {s.Elapsed}");
#endif
		}
		private static IEnumerator UpdateOrgasmKeys()
		{
			while (IsOrgasming)
			{
				RunAllBackground

				();

				yield return new WaitForSecondsRealtime(.5f);
			}
			yield break;
		}*/
	}
}