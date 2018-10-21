using Harmony;
using Verse;

namespace CleaningPriority.ListerFilthPrioritizedNotifiers
{
	[HarmonyPatch(typeof(AreaManager))]
	[HarmonyPatch("TryMakeNewAllowed")]
	class AreaAdded
	{
		static void Postfix(Map ___map, bool __result)
		{
			if (__result) ___map.GetPrioritizedFilthLister().OnAreaAdded();
		}
	}
}