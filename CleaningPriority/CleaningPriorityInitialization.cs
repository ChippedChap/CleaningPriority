using Harmony;
using System.Reflection;
using Verse;

namespace CleaningPriority
{
	[StaticConstructorOnStartup]
	class CleaningPriorityInitialization
	{
		static CleaningPriorityInitialization()
		{
			var harmony = HarmonyInstance.Create("com.github.chippedchap.cleaningpriority");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}