using ShapeKeyMaster.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShapeKeyMaster
{
	internal class ShapeKeyUpdate
	{
		internal static List<TMorph> ListOfActiveMorphs = new List<TMorph>();
		//private static readonly Dictionary<string, IEnumerator> CoroutinesUpdatingMaid = new Dictionary<string, IEnumerator>();
		//private static readonly Dictionary<ShapeKeyEntry, IEnumerator> CoroutinesUpdatingMorph = new Dictionary<ShapeKeyEntry, IEnumerator>();

		private static readonly Queue<IShapeKeyWorkOrder> WorkOrdersQueueWaited = new Queue<IShapeKeyWorkOrder>();
		private static readonly Queue<IShapeKeyWorkOrder> WorkOrdersQueue = new Queue<IShapeKeyWorkOrder>();

		private static IEnumerator _worker;
		private static Coroutine _workerCoRoute;

		private static void BootWorker()
		{
			if (_worker != null && _workerCoRoute != null)
				return;

			_worker = Worker();
			_workerCoRoute = ShapeKeyMaster.Instance.StartCoroutine(_worker);
		}

		/*
		internal static void UpdateKeys(bool DoImmediately = false)
		{
			if (Worker != null)
			{
				Main.instance.StopCoroutine(Worker);
				Worker = null;
			}
			foreach (var kp in CoroutinesUpdatingMaid)
			{
				Main.instance.StopCoroutine(kp.Value);
			}
			CoroutinesUpdatingMaid.Clear();

			foreach (var kp in CoroutinesUpdatingMorph)
			{
				Main.instance.StopCoroutine(kp.Value);
			}
			CoroutinesUpdatingMorph.Clear();

			Worker = UpdateAll(DoImmediately);
			Main.instance.StartCoroutine(Worker);
		}
		*/

		internal static void UpdateKeys(string maid, bool doImmediately = false)
		{
			if (string.IsNullOrEmpty(maid)) return;

			WorkOrdersQueue.Enqueue(new UpdateMaidOrder(maid, doImmediately));

			BootWorker();
		}

		internal static void UpdateKeys(ShapeKeyEntry shapeKeyEntry, bool doImmediately = false)
		{
			if (shapeKeyEntry == null) return;

			var newTask = new UpdateKeysOrder(shapeKeyEntry, doImmediately);

			WorkOrdersQueue.Enqueue(new UpdateKeysOrder(shapeKeyEntry, doImmediately));

#if DEBUG
			ShapeKeyMaster.pluginLogger.LogDebug($"Created task {newTask.ID} to handle the change.");
#endif

			BootWorker();
		}

		public static IEnumerator Worker()
		{
			while (true)
			{
				while (WorkOrdersQueue.Count > 0)
				{
					var job = WorkOrdersQueue.Dequeue();

					if (job.DoImmediately == false)
					{
						WorkOrdersQueueWaited.Enqueue(job);
						continue;
					}

					if (job.ExecuteTask() == false)
					{
						ShapeKeyMaster.PluginLogger.LogWarning($"Failed to execute a task! @\n{Environment.StackTrace}");
					}
				}

				yield return new WaitForSecondsRealtime(ShapeKeyMaster.UpdateDelay.Value);

				while (WorkOrdersQueueWaited.Count > 0)
				{
					var job = WorkOrdersQueueWaited.Dequeue();

					if (job.ExecuteTask() == false)
					{
						ShapeKeyMaster.PluginLogger.LogWarning($"Failed to execute a task! @\n{Environment.StackTrace}");
					}
				}

				yield return null;
			}
		}

		/*
		private static IEnumerator UpdateAll(bool DoImmediately = false)
		{
			if (!DoImmediately)
			{
				yield return new WaitForSeconds(0.10f);
			}

			Ui.SkDatabase.MorphShapekeyDictionary().Keys.ToList().ForEach(morph =>
			{
				morph.FixBlendValues();
				try
				{
					if (morph == morph.bodyskin.body.Face.morph)
					{
						morph.FixBlendValues_Face();
					}
				}
				catch
				{
					// ignored
				}
			});
		}
		*/

		//Setters, loaders, etc,.
		/*
		private static IEnumerator UpdateMaid(string maid, bool DoImmediately = false)
		{
			if (!DoImmediately)
			{
				yield return new WaitForSeconds(0.10f);
			}

			try
			{
				Ui.SkDatabase.MorphShapekeyDictionary()
					.Keys
					.Where(m => m.bodyskin.body.maid.status.fullNameJpStyle.Equals(maid))
					.ToList()
					.ForEach(morph =>
				{
					morph.FixBlendValues();

					try
					{
						if (morph == morph.bodyskin.body.Face.morph)
						{
							morph.FixBlendValues_Face();
						}
					}
					catch
					{
						// ignored
					}
				});
			}
			catch
			{
				//Sometimes a maid has a nullref somewhere in that recursive line of checking. Just catch the error and discard the operation if so...
			}
		}
		*/

		/*
		private static IEnumerator UpdateMorph(ShapeKeyEntry shapeKeyEntry, bool DoImmediately = false)
		{
			if (!DoImmediately)
			{
				yield return new WaitForSeconds(0.10f);
			}

			Ui.SkDatabase.MorphShapekeyDictionary().Where(m => m.Value.Contains(shapeKeyEntry)).ToList().ForEach(keyVal =>
			{
				keyVal.Key.FixBlendValues();

				try
				{
					if (keyVal.Key == keyVal.Key.bodyskin.body.Face.morph)
					{
						keyVal.Key.FixBlendValues_Face();
					}
				}
				catch
				{
					// ignored
				}
			});
		}
		*/

		public interface IShapeKeyWorkOrder
		{
			bool DoImmediately { set; get; }

			bool ExecuteTask();
		}

		public class UpdateKeysOrder : IShapeKeyWorkOrder, IEquatable<UpdateKeysOrder>
		{
#if DEBUG
			private static System.Random rander = new System.Random();

			public readonly string ID;
#endif
			public readonly ShapeKeyEntry ShapeKeyEntry;
			public bool DoImmediately { set; get; }

			public UpdateKeysOrder(ShapeKeyEntry shapeKeyEntry, bool noWait)
			{
				ShapeKeyEntry = shapeKeyEntry;
				DoImmediately = noWait;
#if DEBUG
				ID = shapeKeyEntry.ShapeKey + rander.Next(99999);
#endif
			}

			public bool ExecuteTask()
			{
#if DEBUG
				ShapeKeyMaster.pluginLogger.LogDebug($"{ID} is now executing it's task...");
#endif

				try
				{
					var stuffToIterate = Ui.SkDatabase.MorphShapekeyDictionary()
						.Where(m => m.Value.Contains(ShapeKeyEntry)).ToList();

					stuffToIterate
						.ForEach(keyVal =>
							{
#if DEBUG
								ShapeKeyMaster.pluginLogger.LogDebug($"{ID} is updating mesh for category: {keyVal.Key.Category} | Maid: {keyVal.Key.bodyskin.body.maid.status.fullNameJpStyle}");
#endif

								keyVal.Key.FixBlendValues();

								try
								{
									if (keyVal.Key == keyVal.Key.bodyskin.body.Face.morph)
									{
										keyVal.Key.FixBlendValues_Face();
									}
								}
								catch
								{
									// ignored
								}
							});
				}
				catch
				{
					return false;
				}

				return true;
			}

			public bool Equals(UpdateKeysOrder other)
			{
				return other.ShapeKeyEntry.Id == ShapeKeyEntry.Id;
			}
		}

		public class UpdateMaidOrder : IShapeKeyWorkOrder, IEquatable<UpdateMaidOrder>
		{
			public readonly string Maid;
			public bool DoImmediately { set; get; }

			public UpdateMaidOrder(string maid, bool noWait)
			{
				Maid = maid;
				DoImmediately = noWait;
			}

			public bool ExecuteTask()
			{
				try
				{
					Ui.SkDatabase.MorphShapekeyDictionary()
						.Keys
						.Where(m => m.bodyskin.body.maid.status.fullNameJpStyle.Equals(Maid))
						.ToList()
						.ForEach(morph =>
						{
							morph.FixBlendValues();

							if (morph == morph.bodyskin.body.Face.morph)
							{
								morph.FixBlendValues_Face();
							}
						});
				}
				catch (Exception e)
				{
					ShapeKeyMaster.PluginLogger.LogWarning($"We couldn't update some keys on {Maid}, it's likely some value was null: {e.Message}\n{e.StackTrace}");
					return false;
					//Sometimes a maid has a nullref somewhere in that recursive line of checking. Just catch the error and discard the operation if so...
				}

				return true;
			}

			public bool Equals(UpdateMaidOrder other)
			{
				return Maid.Equals(other?.Maid);
			}
		}
	}
}