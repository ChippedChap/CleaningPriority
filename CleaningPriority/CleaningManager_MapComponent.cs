﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CleaningPriority
{
	class CleaningManager_MapComponent : MapComponent
	{
		private List<Area> priorityList = new List<Area>();

		private List<Area> addableAreas = new List<Area>();
		private bool needToUpdateAddables = true;

		private Area prioritizedArea;
		private bool needToUpdatePrioritized = true;

		private ListerFilthInAreas_MapComponent areaFilthLister;

		[TweakValue("CleaningPriority")]
		public static bool DrawPriorities = false;

		public int AreaCount => priorityList.Count;

		public Area this[int index] => priorityList[index];

		public bool this[Area area] => priorityList.Contains(area);

		public Area PrioritizedArea
		{
			get
			{
				if (needToUpdatePrioritized) ReacalculatePriorityArea();
				return prioritizedArea;
			}
		}

		public List<Area> AddableAreas
		{
			get
			{
				if (needToUpdateAddables)
				{
					addableAreas = map.areaManager.AllAreas.ToList();
					addableAreas.RemoveAll(x => priorityList.Contains(x));
				}
				return addableAreas;
			}
		}

		public CleaningManager_MapComponent(Map map) : base(map)
		{
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref priorityList, "cleaningPriority", LookMode.Reference);
			RemoveNullsInList();
			EnsureHasAtLeastOneArea();
		}

		public override void FinalizeInit()
		{
			EnsureHasAtLeastOneArea();
			areaFilthLister = map.GetListerFilthInAreas();
		}

		public IEnumerable<Thing> FilthInCleaningAreas()
		{
			HashSet<Thing> hashSet = new HashSet<Thing>();
			for (int i = 0; i < priorityList.Count; i++)
			{
				foreach (Thing filth in areaFilthLister.GetFilthInAreaEnumerator(priorityList[i]))
				{
					hashSet.Add(filth);
				}
			}
			return hashSet;
		}

		public void AddAreaRange(IEnumerable<Area> rangeToAdd)
		{
			priorityList.AddRange(rangeToAdd);
			needToUpdatePrioritized = true;
			needToUpdateAddables = true;
		}

		public void RemoveAreaRange(IEnumerable<Area> rangeToRemove)
		{
			priorityList.RemoveAll(x => rangeToRemove.Contains(x));
			EnsureHasAtLeastOneArea();
			needToUpdatePrioritized = true;
			needToUpdateAddables = true;
		}

		public void ReorderPriorities(int from, int to)
		{
			Area areaToReorder = priorityList[from];
			priorityList.RemoveAt(from);
			priorityList.Insert(Mathf.Max(0, (to < from) ? to : to - 1), areaToReorder);
			needToUpdatePrioritized = true;
		}

		public void MarkAllForDraw()
		{
			for (int i = 0; i < priorityList.Count; i++)
			{
				priorityList[i].MarkForDraw();
			}
		}

		public bool FilthIsInCleaningArea(Filth filth)
		{
			for (int i = 0; i < priorityList.Count; i++)
			{
				if (priorityList[i][filth.Position]) return true;
			}
			return false;
		}

		public void OnFilthSpawned(Filth spawned)
		{
			needToUpdatePrioritized = true;
		}

		public void OnFilthDespawned(Filth despawned)
		{
			needToUpdatePrioritized = true;
		}

		public void OnAreaChange(IntVec3 cell, bool newVal, Area area)
		{
			needToUpdatePrioritized = true;
		}

		public void OnAreaDeleted(Area deletedArea)
		{
			priorityList.Remove(deletedArea);
			EnsureHasAtLeastOneArea();
			needToUpdatePrioritized = true;
			needToUpdateAddables = true;
		}

		public void OnAreaAdded()
		{
			needToUpdateAddables = true;
		}

		private void RemoveNullsInList()
		{
			priorityList.RemoveAll(x => x == null);
		}

		private void EnsureHasAtLeastOneArea()
		{
			if (!priorityList.Any()) AddAreaRange(new List<Area>() { map.areaManager.Home });
		}

		private void ReacalculatePriorityArea()
		{
			prioritizedArea = priorityList[priorityList.Count - 1];
			ListerFilthInAreas_MapComponent filthLister = map.GetListerFilthInAreas();
			for (int i = 0; i < priorityList.Count; i++)
			{
				foreach (Filth currentFilth in map.GetListerFilthInAreas()[priorityList[i]])
				{
					if (currentFilth.TicksSinceThickened >= WorkGiver_CleanFilthPrioritized.MinTicksSinceThickened)
					{
						prioritizedArea = priorityList[i];
						needToUpdatePrioritized = false;
						return;
					}
				}
			}
		}
	}
}