using Harmony;
using RimWorld;
using Verse;

namespace CleaningPriority.ListerFilthPrioritizedNotifiers
{
	[HarmonyPatch(typeof(ListerFilthInHomeArea))]
	[HarmonyPatch("RebuildAll")]
	class RebuildAll_Detour
	{
		static void Prefix(Map ___map)
		{
			___map.GetComponent<ListerFilthPrioritized_MapComponent>().RegenerateDictionary();
		}
	}
}