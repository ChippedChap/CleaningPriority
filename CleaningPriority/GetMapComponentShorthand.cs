using System;
using Verse;

namespace CleaningPriority
{
	static class GetMapComponentShorthand
	{
		public static ListerFilthPrioritized_MapComponent GetPrioritizedFilthLister(this Map map)
		{
			var prioritizedLister = map.GetComponent<ListerFilthPrioritized_MapComponent>();
			if (prioritizedLister == null)
			{
				prioritizedLister = new ListerFilthPrioritized_MapComponent(map);
				map.components.Add(prioritizedLister);
			}
			return prioritizedLister;
		}
	}
}