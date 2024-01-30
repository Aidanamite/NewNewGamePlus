using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace NewNewGamePlus
{
    [BepInPlugin("Aidanamite.NewNewGamePlus", "NewNewGamePlus", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{Environment.CurrentDirectory}\\BepInEx\\{modName}";

        void Awake()
        {
            new Harmony($"com.Aidanamite.{modName}").PatchAll();
            Logger.LogInfo($"{modName} has loaded");
        }
    }
    [HarmonyPatch(typeof(StartGamePanel), "SetStateSelectingBeatedGame")]
    class Patch_SelectingBeatenGame
    {
        public static StartGamePanel active;
        static void Prefix(StartGamePanel __instance) => active = __instance;
        static void Postfix() => active = null;
    }
    [HarmonyPatch(typeof(GameSlotGUI), "HasBeatedGame", MethodType.Getter)]
    class Patch_GetHasBeaten
    {
        public static HashSet<GameSlotGUI> overriden = new HashSet<GameSlotGUI>();
        static bool Prefix(GameSlotGUI __instance, ref bool __result)
        {
            if (Patch_SelectingBeatenGame.active && __instance.GameSlot != null && __instance.GameSlot.gameType == GameSlot.GameType.Normal && __instance.GameSlot.IsValid)
            {
                overriden.Add(__instance);
                overriden.RemoveWhere((x) => !x);
                __result = true;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(GameSlotGUI), "MarkAsUnavailable")]
    class Patch_setu
    {
        static bool Prefix(GameSlotGUI __instance)
        {
            if (Patch_SelectingBeatenGame.active && Patch_GetHasBeaten.overriden.Contains(__instance))
            {
                Patch_GetHasBeaten.overriden.Remove(__instance);
                __instance.MarkAsAvailable();
                Patch_SelectingBeatenGame.active._validSlotIndexes.Add(Array.IndexOf(Patch_SelectingBeatenGame.active._gameSlotGUIs, __instance));
                return false;
            }
            return true;
        }
    }
}