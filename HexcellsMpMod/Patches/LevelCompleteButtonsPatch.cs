using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HexcellsMultiplayer;
using UnityEngine;

namespace HexcellsMpMod.Patches
{
    /// <summary>
    /// Sync server host exiting a puzzle on clear screen,
    /// and disable "menu"-button while other players are still in the puzzle.
    /// </summary>
    [HarmonyPatch(typeof(LevelCompleteButtons), nameof(LevelCompleteButtons.OnMouseOver))]
    class LevelCompleteButtonsMouseOverPatch
    {
        private static Func<Boolean> everyoneFinished = () =>
            !MpModManager.Instance.InSession ||
            (MpModManager.Instance.client?.Peers?.All(p => p.CurrentState != HexPeer.State.InPuzzle) ?? true);
            
        static bool Prefix(LevelCompleteButtons __instance)
        {
            var buttonTypeField = typeof(LevelCompleteButtons).GetField("buttonType", BindingFlags.Instance | BindingFlags.Public);
            var buttonType = (LevelCompleteButtons.ButtonType)buttonTypeField.GetValue(__instance);
            if ((buttonType == LevelCompleteButtons.ButtonType.Menu ||
                 buttonType == LevelCompleteButtons.ButtonType.MenuCustom) &&
                !everyoneFinished())
            {
                return false;
            }

            return true;
        }

        static void Postfix(LevelCompleteButtons __instance)
        {
            var buttonTypeField = typeof(LevelCompleteButtons).GetField("buttonType", BindingFlags.Instance | BindingFlags.Public);
            if (Input.GetMouseButtonDown(0))
            {
                var buttonType = (LevelCompleteButtons.ButtonType)buttonTypeField.GetValue(__instance);
                if (buttonType == LevelCompleteButtons.ButtonType.Menu || buttonType == LevelCompleteButtons.ButtonType.MenuCustom)
                {
                    MpModManager.Instance.HostQuitGame();
                }
            }
        }
    }

    /// <summary>
    /// Disable next button on custom levels, if not the host
    /// </summary>
    [HarmonyPatch(typeof(LevelCompleteButtons), nameof(LevelCompleteButtons.Start))]
    class LevelCompleteButtonsStartrPatch
    {
        static void Postfix(LevelCompleteButtons __instance)
        {
            var buttonTypeField = typeof(LevelCompleteButtons).GetField("buttonType", BindingFlags.Instance | BindingFlags.Public);
            if (MpModManager.Instance.InSession && !MpModManager.Instance.IsHosting)
            {
                var buttonType = (LevelCompleteButtons.ButtonType)buttonTypeField.GetValue(__instance);
                if (buttonType == LevelCompleteButtons.ButtonType.NextCustom)
                {
                    __instance.GetComponent<BoxCollider>().enabled = false;
                }
            }
        }
    }
}