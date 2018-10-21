using Harmony;
using Verse;

namespace CleaningPriority.ListerFilthPrioritizedNotifiers
{
	[HarmonyPatch(typeof(AreaManager))]
	[HarmonyPatch("NotifyEveryoneAreaRemoved")]
	class AreaRemoved
	{
		static void Postfix(Map ___map, Area area)
		{
			___map.GetComponent<ListerFilthPrioritized_MapComponent>().OnAreaDeleted(area);
		}
	}
}