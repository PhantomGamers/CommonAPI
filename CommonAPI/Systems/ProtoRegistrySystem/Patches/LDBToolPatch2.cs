﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Configuration;
using HarmonyLib;
using xiaoye97;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public class LDBToolPatch2
    {
        [HarmonyPatch(typeof(LDBTool), "StringBind")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixLDBToolMemory(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldloc_2),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Stfld)
                );

            for (int i = 0; i < 3; i++)
            {
                while (matcher.Opcode != OpCodes.Stfld)
                {
                    matcher.RemoveInstruction();
                }

                matcher.RemoveInstruction();
            }

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 4))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<StringProto, ConfigEntry<string>, ConfigEntry<string>, ConfigEntry<string>>>(
                    (proto, znch, enus, frfr) =>
                    {
                        if (!String.Equals(znch.Value, ""))
                            proto.ZHCN = znch.Value;
                        else
                            znch.Value = proto.ZHCN;

                        if (!String.Equals(enus.Value, ""))
                            proto.ENUS = enus.Value;
                        else
                            enus.Value = proto.ENUS;

                        if (!String.Equals(frfr.Value, ""))
                            proto.FRFR = frfr.Value;
                        else
                            frfr.Value = proto.FRFR;
                    }));
                


            return matcher.InstructionEnumeration();
        }
    }
}