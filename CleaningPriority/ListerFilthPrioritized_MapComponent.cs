using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CleaningPriority
{
	class ListerFilthPrioritized_MapComponent : MapComponent
	{
		private Dictionary<Area, List<Thing>> filthDictionary = new Dictionary<Area, List<Thing>>();
		private List<Area> priorityList = new List<Area>();

		private Area prioritizedArea;
		private bool needToUpdatePrioritized;
		private List<Area> cachedAddableAreas = new List<Area>();
		private bool needToUpdateAddables;

		public static HashSet<Type> excludedTypes = new HashSet<Type>() { typeof(Area_SnowClear), typeof(Area_NoRoof) };

		[TweakValue("CleaningPriority")]
		public static bool DrawPriorities = false;

		public int AreaCount => priorityList.Count;

		public Area this[int index] => priorityList[index];

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
					cachedAddableAreas = map.areaManager.AllAreas.ToList();
					cachedAddableAreas.RemoveAll(x => excludedTypes.Contains(x.GetType()) || priorityList.Contains(x));
					needToUpdateAddables = false;
				}
				return cachedAddableAreas;
			}
		}

		public ListerFilthPrioritized_MapComponent(Map map) : base(map)
		{
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref priorityList, "cleaningPriority", LookMode.Reference);
			EnsureHasAtLeastOneArea();
		}

		public override void FinalizeInit()
		{
			RegenerateDictionary();
			EnsureHasAtLeastOneArea();
		}

		public override void MapComponentOnGUI()
		{
			if (DrawPriorities)
			{
				float elementHeight = 30f;
				HashSet<Thing> alreadyLabeled = new HashSet<Thing>();
				for (int i = 0; i < priorityList.Count; i++)
				{
					EnsureAreaHasKey(priorityList[i]);
					Rect listRect = new Rect(UI.screenWidth - 100f, i * elementHeight, 100f, 100f);
					Widgets.Label(listRect, priorityList[i].Label);
					for (int j = 0; j < filthDictionary[priorityList[i]].Count; j++)
					{
						Thing filth = filthDictionary[priorityList[i]][j];
						if (alreadyLabeled.Contains(filth)) continue;
						alreadyLabeled.Add(filth);

						Vector2 screenLabelPos = UI.MapToUIPosition(new Vector3(filth.Position.x, filth.Position.y, filth.Position.z + 1));
						Vector2 tileWidth = new Vector2(Find.CameraDriver.CellSizePixels, Find.CameraDriver.CellSizePixels);
						Rect labelRect = new Rect(screenLabelPos, tileWidth);
						Text.Anchor = TextAnchor.MiddleCenter;
						Widgets.Label(labelRect, i.ToString());
						Text.Anchor = TextAnchor.UpperLeft;
					}
				}
			}
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

		public IEnumerable<Thing> FilthInCleaningAreas()
		{
			HashSet<Thing> filthListWithoutDuplicates = new HashSet<Thing>();
			for (int i = 0; i < priorityList.Count; i++)
			{
				for (int j = 0; j < filthDictionary[priorityList[i]].Count; j++)
				{
					filthListWithoutDuplicates.Add(filthDictionary[priorityList[i]][j]);
				}
			}
			return filthListWithoutDuplicates;
		}

		public void OnFilthSpawned(Filth spawned)
		{
			for (int i = 0; i < map.areaManager.AllAreas.Count; i++)
			{
				Area area = map.areaManager.AllAreas[i];
				if (excludedTypes.Contains(area.GetType())) continue;
				if (area is Area_Home)
				{
					filthDictionary[area] = map.listerFilthInHomeArea.FilthInHomeArea;
				}
				else if (area[spawned.Position])
				{
					filthDictionary[area].Add(spawned);
				}
			}
			needToUpdatePrioritized = true;
		}

		public void OnFilthDespawned(Filth despawned)
		{
			for (int i = 0; i < map.areaManager.AllAreas.Count; i++)
			{
				Area area = map.areaManager.AllAreas[i];
				if (excludedTypes.Contains(area.GetType())) continue;
				if (area is Area_Home)
				{
					filthDictionary[area] = map.listerFilthInHomeArea.FilthInHomeArea;
				}
				else
				{
					filthDictionary[area].Remove(despawned);
				}
			}
			needToUpdatePrioritized = true;
		}

		public void OnAreaChange(IntVec3 cell, bool newVal, Area area)
		{
			if (!excludedTypes.Contains(area.GetType()))
			{
				List<Thing> thingsInCell = cell.GetThingList(map);
				if (newVal)
				{
					filthDictionary[area].AddRange(thingsInCell.Where(x => x is Filth));
				}
				else
				{
					filthDictionary[area].RemoveAll(x => thingsInCell.Contains(x));
				}
			}
			needToUpdatePrioritized = true;
			needToUpdateAddables = true;
		}

		public void OnAreaDeleted(Area deletedArea)
		{
			if (priorityList.Contains(deletedArea))
			{
				filthDictionary.Remove(deletedArea);
				priorityList.Remove(deletedArea);
				EnsureHasAtLeastOneArea();
			}
			needToUpdatePrioritized = true;
			needToUpdateAddables = true;
		}

		public void OnAreaAdded(Area area)
		{
			EnsureAreaHasKey(area);
			needToUpdateAddables = true;
		}

		private void EnsureAreaHasKey(Area area)
		{
			if (!filthDictionary.ContainsKey(area)) filthDictionary[area] = new List<Thing>();
		}

		private void EnsureAllAreasHaveKeys()
		{
			for (int i = 0; i < map.areaManager.AllAreas.Count; i++) EnsureAreaHasKey(map.areaManager.AllAreas[i]);
		}

		private void EnsureHasAtLeastOneArea()
		{
			if (!priorityList.Any()) priorityList.Add(map.areaManager.Home);
		}

		private void ReacalculatePriorityArea()
		{
			prioritizedArea = priorityList[priorityList.Count - 1];
			for (int i = 0; i < priorityList.Count; i++)
			{
				for (int j = 0; j < filthDictionary[priorityList[i]].Count; j++)
				{
					Filth currentFilth = (Filth)filthDictionary[priorityList[i]][j];
					if (currentFilth.TicksSinceThickened >= WorkGiver_CleanFilthPrioritized.MinTicksSinceThickened)
					{
						prioritizedArea = priorityList[i];
						needToUpdatePrioritized = false;
						return;
					}
				}
			}
		}

		private void RegenerateDictionary()
		{
			EnsureAllAreasHaveKeys();
			needToUpdateAddables = true;
			needToUpdatePrioritized = true;
			foreach (IntVec3 c in map.AllCells)
			{
				for (int i = 0; i < map.areaManager.AllAreas.Count; i++)
				{
					EnsureAreaHasKey(map.areaManager.AllAreas[i]);
					if (map.areaManager.AllAreas[i][c]) OnAreaChange(c, true, map.areaManager.AllAreas[i]);
				}
			}
		}
	}
}