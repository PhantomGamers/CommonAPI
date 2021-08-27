﻿using HarmonyLib;
using CommonAPI;
// ReSharper disable InconsistentNaming

namespace CommonAPI
{
    [HarmonyPatch]
    public class UIGamePatch
    {
        [HarmonyPatch(typeof(UIGame), "_OnCreate")]
        [HarmonyPostfix]
        public static void OnCreate(UIGame __instance)
        {
            UISystem.OnCreate(__instance);
        }
        
        [HarmonyPatch(typeof(UIGame), "_OnDestroy")]
        [HarmonyPostfix]
        public static void OnDestroy()
        {
            foreach (var window in UISystem.windows)
            {
                window._Destroy();
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnInit")]
        [HarmonyPostfix]
        public static void OnInit(UIGame __instance)
        {
            foreach (var window in UISystem.windows)
            {
                window._Init(__instance.gameData);
                window.Open(0);
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnFree")]
        [HarmonyPostfix]
        public static void OnFree()
        {
            foreach (var window in UISystem.windows)
            {
                window._Free();
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnUpdate")]
        [HarmonyPostfix]
        public static void OnUpdate()
        {
            foreach (var window in UISystem.windows)
            {
                window.OnUpdateUI();
            }
        }

        [HarmonyPatch(typeof(UIGame), "ShutInventoryConflictsWindows")]
        [HarmonyPatch(typeof(UIGame), "ShutAllFunctionWindow")]
        [HarmonyPostfix]
        public static void ShutAllFunctionWindow()
        {
            foreach (var window in UISystem.windows)
            {
                window.Close();
            }
        }
        
        [HarmonyPatch(typeof(UIGame), "get_isAnyFunctionWindowActive")]
        [HarmonyPostfix]
        public static void IsAnyActive(ref bool __result)
        {
            if (__result) return;
                
            foreach (var window in UISystem.windows)
            {
                if (window.active)
                {
                    __result = true;
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(UIGame), "OnPlayerInspecteeChange")]
        [HarmonyPostfix]
        public static void OnPlayerInspecteeChange(UIGame __instance, EObjectType objType, int objId)
        {
           UISystem.OnPlayerInspecteeChange(__instance, objType, objId);
        }
    }
}