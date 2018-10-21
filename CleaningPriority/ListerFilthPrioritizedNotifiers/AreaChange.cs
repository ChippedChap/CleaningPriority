using Harmony;
using Verse;

namespace CleaningPriority.ListerFilthPrioritizedNotifiers
{
	[HarmonyPatch(typeof(Area))]
	[HarmonyPatch("Set")]
	class AreaChange
	{
		static void Postfix(Area __instance, AreaManager ___areaManager, IntVec3 c, bool val)
		{
			___areaManager.map.GetComponent<ListerFilthPrioritized_MapComponent>().OnAreaChange(c, val, __instance);
		}
	}
}