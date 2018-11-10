using System;
using Verse;

namespace CleaningPriority
{
	static class GetMapComponentShorthand
	{
		public static ListerFilthPrioritized_MapComponent GetPrioritizedFilthLister(this Map map)
		{
			return map.GetComponent<ListerFilthPrioritized_MapComponent>();
		}
	}
}