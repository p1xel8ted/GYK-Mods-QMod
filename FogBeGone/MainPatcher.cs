using System.Reflection;
using HarmonyLib;

namespace FogBeGone
{
    public class MainPatcher
    {
        public static void Patch()
        {
            var harmony = new Harmony("p1xel8ted.GraveyardKeeper.FogBeGone");
            var assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
        }

        [HarmonyPatch(typeof(SmartWeatherState))]
        [HarmonyPatch(nameof(SmartWeatherState.Update))]
        public static class WeatherPatch1
        {
            [HarmonyPrefix]
            public static bool Prefix(SmartWeatherState __instance, ref bool ____previously_enabled,
                ref bool ____enabled, ref float ____cur_amount)
            {
                if (!MainGame.game_started) return true;
                if (__instance == null) return true;
                switch (__instance.type)
                {
                    case SmartWeatherState.WeatherType.Fog:
                        ____previously_enabled = true;
                        ____enabled = false;
                        ____cur_amount = 0;
                        __instance.value = 0;
                        break;
                    case SmartWeatherState.WeatherType.Wind:
                        ____previously_enabled = true;
                        ____enabled = false;
                        ____cur_amount = 0;
                        __instance.value = 0;
                        break;
                    case SmartWeatherState.WeatherType.Rain:
                        ____previously_enabled = false;
                        break;
                    case SmartWeatherState.WeatherType.LUT:
                        break;
                    default:
                        break;
                }

                return true;
            }
        }
    }
}