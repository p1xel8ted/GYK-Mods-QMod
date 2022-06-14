using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Exhaustless.lang;
using HarmonyLib;
using UnityEngine;

namespace Exhaustless
{
    public class MainPatcher
    {
        private static Config.Options _cfg;
        private static string Lang { get; set; }

        public static void Patch()
        {
            try
            {
                _cfg = Config.GetOptions();
                var harmony = new Harmony("p1xel8ted.GraveyardKeeper.exhaust-less");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                Lang = GameSettings.me.language.Replace('_', '-').ToLower(CultureInfo.InvariantCulture).Trim();
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Lang);
            }
            catch (Exception)
            {
                //  File.AppendAllText("./qmods/dura.txt", $"{ex.Message} - {ex.Source} - {ex.StackTrace}\n");
            }
        }

        [HarmonyPatch(typeof(InGameMenuGUI), nameof(InGameMenuGUI.OnClosePressed))]
        public static class InGameMenuGuiOnClosePressedPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                Lang = GameSettings.me.language.Replace('_', '-').ToLower(CultureInfo.InvariantCulture).Trim();
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Lang);
            }
        }

        [HarmonyPatch(typeof(CraftComponent))]
        [HarmonyPatch(nameof(CraftComponent.TrySpendPlayerGratitudePoints))]
        public static class CraftComponentTrySpendPlayerGratitudePointsPatch
        {
            [HarmonyPrefix]
            public static void Prefix(ref float value)
            {
                if (_cfg.SpendHalfGratitude)
                {
                    value /= 2f;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerComponent))]
        [HarmonyPatch(nameof(PlayerComponent.TrySpendEnergy))]
        public static class PatchTrySpendEnergy
        {
            [HarmonyPrefix]
            public static void Prefix(ref float need_energy)
            {
                if (_cfg.SpendHalfEnergy)
                {
                    need_energy /= 2f;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerComponent))]
        [HarmonyPatch(nameof(PlayerComponent.SpendSanity))]
        public static class PatchSpendSanity
        {
            [HarmonyPrefix]
            public static void Prefix(ref float need_sanity)
            {
                if (_cfg.SpendHalfSanity)
                {
                    need_sanity /= 2f;
                }
            }
        }


        [HarmonyPatch(typeof(WaitingGUI))]
        [HarmonyPatch(nameof(WaitingGUI.Update))]
        public static class PatchWaiting
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                if (!_cfg.SpeedUpMeditation) return;
                MainGame.me.player.energy += 0.25f;
                MainGame.me.player.hp += 0.25f;
            }

            [HarmonyPostfix]
            public static void Postfix(WaitingGUI __instance)
            {
                if (!_cfg.AutoWakeFromMeditation) return;
                var save = MainGame.me.save;
                if (MainGame.me.player.energy.EqualsOrMore(save.max_hp) &&
                    MainGame.me.player.hp.EqualsOrMore(save.max_energy))
                {
                    typeof(WaitingGUI).GetMethod("StopWaiting", AccessTools.all)
                        ?.Invoke(__instance, null);
                }
            }
        }

        [HarmonyPatch(typeof(WorldGameObject))]
        [HarmonyPatch(nameof(WorldGameObject.EquipItem))]
        public static class PatchToolDurabilitySpeed2
        {
            [HarmonyPostfix]
            public static void Postfix(ref Item item)
            {
                if (!_cfg.MakeToolsLastLonger) return;
                if (item.definition.durability_decrease_on_use)
                {
                    item.definition.durability_decrease_on_use_speed = 0.005f;
                }
            }
        }


        [HarmonyPatch(typeof(MainGame))]
        [HarmonyPatch(nameof(MainGame.OnEquippedToolBroken))]
        public static class PatchBrokenTool
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                if (!_cfg.AutoEquipNewTool) return;
                var equippedTool = MainGame.me.player.GetEquippedTool();
                var save = MainGame.me.save;
                var playerInv = save.GetSavedPlayerInventory();
                foreach (var item in playerInv.inventory.Where(item =>
                             item.definition.type == equippedTool.definition.type))
                {
                    if (item.durability_state is not (Item.DurabilityState.Full or Item.DurabilityState.Used))
                        continue;
                    MainGame.me.player.EquipItem(item, -1, playerInv.is_bag ? playerInv : null);
                    MainGame.me.player.Say(
                        $"{strings.LuckyHadAnotherPartOne} {item.definition.GetItemName()} {strings.LuckyHadAnotherPartTwo}",
                        null, false,
                        SpeechBubbleGUI.SpeechBubbleType.Think,
                        SmartSpeechEngine.VoiceID.None, true);
                }
            }
        }


        [HarmonyPatch(typeof(SleepGUI))]
        [HarmonyPatch(nameof(SleepGUI.Update))]
        public static class PatchSleeping
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                if (!_cfg.SpeedUpSleep) return;
                MainGame.me.player.energy += 0.25f;
                MainGame.me.player.hp += 0.25f;
            }
        }


        [HarmonyPatch(typeof(BuffsLogics))]
        [HarmonyPatch(nameof(BuffsLogics.AddBuff))]
        public static class PatchBuff
        {
            [HarmonyPrefix]
            public static void Prefix(ref string buff_id)
            {
                if (!_cfg.YawnMessage) return;
                if (buff_id.Equals("buff_tired"))
                {
                    MainGame.me.player.Say(strings.Yawn, null, null,
                        SpeechBubbleGUI.SpeechBubbleType.Think, SmartSpeechEngine.VoiceID.None, true);
                }
            }
        }


        [HarmonyPatch(typeof(WorldGameObject))]
        [HarmonyPatch(nameof(WorldGameObject.GetParam))]
        public static class WorldGameObjectGetParamPatch
        {
            [HarmonyPostfix]
            private static void Postfix(ref WorldGameObject __instance, ref string param_name, ref Item ____data,
                ref float __result)
            {
                if (!param_name.Contains("tiredness")) return;
                var tiredness = ____data.GetParam("tiredness", 0f);
                __result = tiredness < 1200 ? 250 : 350;
            }
        }
    }
}