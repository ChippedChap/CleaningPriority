using Harmony;
using RimWorld;
using Verse;

namespace CleaningPriority.ListerFilthPrioritizedNotifiers
{
	[HarmonyPatch(typeof(ListerFilthInHomeArea))]
	[HarmonyPatch("Notify_FilthDespawned")]
	class FilthDespawned
	{
		static void Postfix(Map ___map, Filth f)
		{
			___map.GetComponent<ListerFilthPrioritized_MapComponent>().OnFilthDespawned(f);
		}
	}
}