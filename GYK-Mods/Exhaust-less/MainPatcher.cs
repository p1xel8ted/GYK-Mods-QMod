using System.Reflection;
using Harmony;

namespace Exhaustless
{
    public class MainPatcher
    {
        public static void Patch()
        {
            HarmonyInstance val = HarmonyInstance.Create("p1xel8ted.graveyardkeeper.exhaust-less");
            val.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(PlayerComponent))]
        [HarmonyPatch(nameof(PlayerComponent.TrySpendEnergy))]
        public static class Patch_TrySpendEnergy
        {
            [HarmonyPrefix]
            public static void Prefix(ref float need_energy)
            {
                need_energy /= 2f;
            }
        }
    }
}