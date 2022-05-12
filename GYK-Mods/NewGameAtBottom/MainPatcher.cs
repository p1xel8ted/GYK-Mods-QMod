using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;

namespace NewGameAtBottom
{
    public class MainPatcher
    {
        public static void Patch()
        {
            var val = HarmonyInstance.Create("p1xel8ted.graveyardkeeper.newgameatbottom");
            val.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(SaveSlotsMenuGUI), "RedrawSlots")]
        public static class PatchSaveMenu
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                //originally a blank save called "new" gets added the list of saves first when the save UI is generated
                //and then actual save games are added next, so New game is always at the top (dumb right)
                //this swaps it around, so that player games are first, and "New" game is at the bottom
                var codes = new List<CodeInstruction>(instructions);
                var codesToKeep = codes.GetRange(61, 14);
                codes.RemoveRange(61,14);
                codes.InsertRange(91,codesToKeep);
                return codes.AsEnumerable();
            }
        }
    }
}