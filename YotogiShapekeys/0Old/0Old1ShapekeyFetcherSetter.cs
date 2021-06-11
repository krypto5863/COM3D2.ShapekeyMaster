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
		private static readonly List<TMorph> FixList = new List<TMorph>();
		private static readonly List<ShapekeyChangeForm> ChangeList = new List<ShapekeyChangeForm>();
		private static bool CorouteRunning = false;

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

			MissionControlMaid(maid);
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
		public static void MissionControlAll()
		{

			Stopwatch stop = new Stopwatch();

			stop.Start();

			List<Maid> maids = maidslist
				.Where(m => m != null)
				.ToList();

			IEnumerable<TMorph> morphs = maids
				//.Select(m => GetAllMorphsFromMaid(m))
				//.Select(m => Task.Factory.StartNew(() => GetAllMorphsFromMaid(m)))
				.Select(m => GetAllMorphsFromMaid(m))
				//.Select(async n => await n)
				//.Select(t => t.Result)
				.SelectMany(r => r);

			IEnumerable<ShapekeyChangeForm> changes = morphs
				//.Select(m => Task.Factory.StartNew(() => GetTMorphChangeForm(m)))
				.Select(m => GetTMorphChangeForm(m))
				//.Select(m => RunSingleTMorph(m))
				//.Select(async n => await n)
				//.Select(n => n.Result)
				.SelectMany(r => r);

			ProcessChanges(changes);

			stop.Stop();

#if (DEBUG)
			Main.logger.Log($"MissionControlAll finished processing in {stop.Elapsed} ms");
#endif
		}
		public static void MissionControlMaid(Maid maid)
		{


#if (DEBUG)
			Main.logger.Log($"Started");
#endif

			Stopwatch stop = new Stopwatch();

			stop.Start();


#if (DEBUG)
			Main.logger.Log($"Checking maid isn't null.");
#endif

			if (maid == null)
			{
				return;
			}


#if (DEBUG)
			Main.logger.Log($"Fetching morphs");
#endif

			//var morphs = await Task.Factory.StartNew(() => GetAllMorphsFromMaid(maid));
			IEnumerable<TMorph> morphs = GetAllMorphsFromMaid(maid);


#if (DEBUG)
			Main.logger.Log($"Converting morphs to changes...");
#endif

			IEnumerable<ShapekeyChangeForm> changes = morphs
			//.Select(m => Task.Factory.StartNew(() => GetTMorphChangeForm(m)))
			.Select(m => GetTMorphChangeForm(m))
			//.Select(async t => await t)
			//.Select(n => n.Result)
			.SelectMany(r => r);


#if (DEBUG)
			Main.logger.Log($"Processing changes...");
#endif
			ProcessChanges(changes);

			stop.Stop();

#if (DEBUG)
			Main.logger.Log($"MissionControlMaid finished processing in {stop.Elapsed} ms");
#endif
		}
		public static void MissionControlSingleEntry(ShapeKeyEntry sk)
		{
			Stopwatch stop = new Stopwatch();

			stop.Start();

#if (DEBUG)
			Main.logger.Log($"Calculating single entry...");
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
			Main.logger.Log($"Fetched Maids");
#endif

			if (maids.Count <= 0)
			{
				return;
			}

#if (DEBUG)
			Main.logger.Log($"Maids were not zero...");
#endif

			List<TMorph> morphs = maids
			//.Select(m => Task.Factory.StartNew(() => GetAllMorphsFromMaid(m)))
			.Select(m => GetAllMorphsFromMaid(m))
			//.Select(async t => await t)
			//.Select(n => n.Result)
			.SelectMany(r => r)
			.Where(m => m.Contains(sk.ShapeKey))
			.ToList();

#if (DEBUG)
			Main.logger.Log($"Fetched Morphs");
#endif

			if (morphs.Count <= 0)
			{
				return;
			}

#if (DEBUG)
			Main.logger.Log($"Morphs were not zero");
#endif

			IEnumerable<ShapekeyChangeForm> changes = morphs
			//.Select(m => Task.Factory.StartNew(() => GetShapekeyChangeForm(sk,m)))
			.Select(m => GetShapekeyChangeForm(sk, m));
			//.Select(async t => await t)
			//.Select(n => n.Result);

#if (DEBUG)
			Main.logger.Log($"Turned morphs into changes");
#endif

			ProcessChanges(changes);

#if (DEBUG)
			Main.logger.Log($"Processed changes...");
#endif

			stop.Stop();

#if (DEBUG)
			Main.logger.Log($"MissionControlSingleEntry finished processing in {stop.Elapsed} ms");
#endif
		}
		//This function takes a morph and checks every single shapekey entry for one that edits the held morph. If one is found, pushes to modify morph.
		public static List<ShapekeyChangeForm> GetTMorphChangeForm(TMorph m)
		{
#if (DEBUG)
			Main.logger.Log($"Working on TMorph holding this many morphs: {m.MorphCount}");
#endif

			//Filters the list and only returns viable shapekey entries to reduce processing times.
			List<ShapeKeyEntry> templist = UI.ShapeKeys.Values
			.Where(s => s.Enabled == true
			&& s.IsProp == false
			&& (s.Maid.Equals("") || m.bodyskin.body.maid.status.fullNameJpStyle.Equals(s.Maid))
			&& m.Contains(s.ShapeKey))
			.ToList();

			List<ShapekeyChangeForm> resultList = new List<ShapekeyChangeForm>();

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
				Main.logger.Log($"Creating change form of {shapekeyIndex} {s.Deform} {s.ShapeKey}");
#endif
			}
			return resultList;
		}
		//This is basically a copy of RunSingleTmorph but made to work with shapekey entries as input instead of morphs.
		public static ShapekeyChangeForm GetShapekeyChangeForm(ShapeKeyEntry s, TMorph m)
		{
#if (DEBUG)
			Main.logger.Log($"Working on TMorph holding this many morphs: {m.MorphCount}");
#endif

			List<ShapekeyChangeForm> resultList = new List<ShapekeyChangeForm>();

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
				return new ShapekeyChangeForm
				{
					Morph = m,
					Index = shapekeyIndex,
					Deform = OrgasmDeform,
					ShapekeyName = s.ShapeKey
				};
			}

			//If the maid is supposed to be animated with excitement and we are in yotogi, we set values according to an excitement deformation calculation.
			if (s.AnimateWithExcitement && YotogiLvls.Contains(SceneManager.GetActiveScene().name))
			{
				float deform = CalculateExcitementDeformation(s, maidslist);

				return new ShapekeyChangeForm
				{
					Morph = m,
					Index = shapekeyIndex,
					Deform = deform,
					ShapekeyName = s.ShapeKey
				};
			}

			return new ShapekeyChangeForm
			{
				Morph = m,
				Index = shapekeyIndex,
				Deform = s.Deform,
				ShapekeyName = s.ShapeKey
			};
		}
		//Even though this goes against my mission control's idea. I wanted to save myself the trouble of rewriting the same code 3 times As such, I decided to go for it. This will be run sync though.
		public static void ProcessChanges(IEnumerable<ShapekeyChangeForm> changes)
		{

#if (DEBUG)
			Main.logger.Log($"Processing changes...");
#endif

			//List<Task> taskList = new List<Task>();

#if (DEBUG)
			Main.logger.Log($"Starting foreach...");
#endif

			if (changes.Count() <= 0)
			{
#if (DEBUG)
				Main.logger.Log($"There were 0 changes to be made...");
#endif
			}

			ChangeList.AddRange(changes.ToList());

			if (CorouteRunning == false)
			{
				Main.@this.StartCoroutine(RunShapekeyChange());
			}

#if (DEBUG)
			Main.logger.Log($"Finished foreach...");
#endif

		}
		//The actual changes are pushed to a coroutine. Coroutines have the property of always running on the main thread, in time with the main thread so pushing this function to a coroutine from a worker thread ensures safe changes. However when possible. It should be avoided as creating too many coroutines is asking for trouble.
		public static IEnumerator RunShapekeyChange()
		{

			CorouteRunning = true;

			yield return new WaitForEndOfFrame();

			//Setting a value of less than 0 can cause the game to crash. As such, we ensure we never set a 0 val.

			while (ChangeList.Count > 0)
			{

				if (ChangeList[0].Morph != null && ChangeList[0].Morph.GetBlendValues(ChangeList[0].Index) != ChangeList[0].Deform)
				{

					if (ChangeList[0].Morph == null || ChangeList[0].Morph.bodyskin == null || ChangeList[0].Morph.bodyskin.body == null || ChangeList[0].Morph.bodyskin.body.maid.isActiveAndEnabled == false)
					{
						yield return new WaitForEndOfFrame();
					}

					if (ChangeList[0].Deform < 0)
					{
						ChangeList[0].Deform = 0;
					}

#if (DEBUG)
					Main.logger.Log($"Value needs setting! Setting value of {ChangeList[0].Deform}");
#endif
					Main.SetBlendValues(ChangeList[0].Index, (ChangeList[0].Deform / 100f), ChangeList[0].Morph);
					//FixSingleBlendValues(m, shapekeyIndex);

					try
					{
						ChangeList[0].Morph.FixBlendValues();
					}
					catch
					{

					}

#if (DEBUG)
					Main.logger.Log($"Checking if was a face morph and needs updating");
#endif
					if (ChangeList[0].Morph.bodyskin != null && ChangeList[0].Morph.bodyskin.body != null && ChangeList[0].Morph.bodyskin.body.Face != null && ChangeList[0].Morph.bodyskin.body.Face.morph != null && ChangeList[0].Morph.bodyskin.body.Face.morph.Contains(ChangeList[0].ShapekeyName))
					{
						ChangeList[0].Morph.bodyskin.body.Face.morph.FixBlendValues_Face();
					}

					ChangeList.RemoveAt(0);

#if (DEBUG)
					Main.logger.Log($"Done with single shapekey change.");
#endif
				}
			}
			CorouteRunning = false;
		}
		/*
		public static void FixSingleBlendValues(TMorph morph, int index) 
		{
			int num = 0;
			var BlendValuesCHK = AccessTools.DeclaredField(typeof(TMorph), "BlendValuesCHK").GetValue(morph) as float[];

			var BlendValues = AccessTools.DeclaredField(typeof(TMorph), "BlendValues").GetValue(morph) as float[];

			if (BlendValuesCHK[index] != BlendValues[index])
			{
				num++;
				BlendValuesCHK[index] = BlendValues[index];
			}
			if (num == 0)
			{
				return;
			}

			var m_vTmpVert = AccessTools.DeclaredField(typeof(TMorph), "m_vTmpVert").GetValue(morph) as Vector3[];

			var m_vTmpNorm = AccessTools.DeclaredField(typeof(TMorph), "m_vTmpNorm").GetValue(morph) as Vector3[];

			var m_bMorph = (bool)AccessTools.DeclaredField(typeof(TMorph), "m_bMorph").GetValue(morph);

			morph.m_vOriVert.CopyTo(m_vTmpVert, 0);
			morph.m_vOriNorm.CopyTo(m_vTmpNorm, 0);
			m_bMorph = true;
			if (morph.BlendDatas[index] == null)
			{
				morph.UruUruScaleX = BlendValues[index];
			}
			else
			{
				float num2 = BlendValues[index];
				if (num2 >= 0.01f || index == morph.m_MayuShapeIn.idx || index == morph.m_MayuShapeOut.idx)
				{
					int num3 = morph.BlendDatas[index].v_index.Length;
					for (int k = 0; k < num3; k++)
					{
						int num4 = morph.BlendDatas[index].v_index[k];
						m_vTmpVert[num4] += morph.BlendDatas[index].vert[k] * num2;
						m_vTmpNorm[num4] += morph.BlendDatas[index].norm[k] * num2;
					}
				}
			}

			var m_mesh = (Mesh)AccessTools.DeclaredField(typeof(TMorph), "m_mesh").GetValue(morph);

			var m_bindposes = AccessTools.DeclaredField(typeof(TMorph), "m_bindposes").GetValue(morph) as Matrix4x4[];

			m_mesh.vertices = m_vTmpVert;
			m_mesh.normals = m_vTmpNorm;
			foreach (TAttachPoint tattachPoint in morph.dicAttachPoint.Values)
			{
				int vidx = tattachPoint.vidx;
				morph.BindVert[vidx] = m_bindposes[tattachPoint.bw.boneIndex0].MultiplyPoint3x4(m_vTmpVert[vidx]);
			}
		}*/
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
				RunAllBackgroundAsync();

				yield return new WaitForSecondsRealtime(.5f);
			}
			yield break;
		}*/
	}
}
