using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PotentialCellsMod.Patches
{
    /// <summary>
    /// When loading level, setup "remaining - potential" counter 
    /// </summary>
    [HarmonyPatch(typeof(HexScoring), nameof(HexScoring.Start))]
    class HexScoringStartPatch
    {
        private static int numberPotentialBlues;
        private static String originalText;
        private static TextMesh remainingText;

        static void Postfix(HexScoring __instance)
        {
            var remainingTextField =
                typeof(HexScoring).GetField("remainingText", BindingFlags.NonPublic | BindingFlags.Instance);
            remainingText = remainingTextField.GetValue(__instance) as TextMesh;
            UpdateOriginalText();
        }

        private static void UpdateDisplay()
        {
            if (numberPotentialBlues > 0)
            {
                ushort originalNumber = ushort.Parse(originalText);
                remainingText.text = $"{originalText} ({originalNumber - numberPotentialBlues})";
            }
            else
            {
                remainingText.text = originalText;
            }
        }

        public static void UpdateOriginalText()
        {
            originalText = remainingText.text;
            UpdateDisplay();
        }

        public static void PotentialCellPlacementChanged(bool added)
        {
            numberPotentialBlues += added ? 1 : -1;
            UpdateDisplay();
        }

        public static void PotentialCellsReset()
        {
            numberPotentialBlues = 0;
            UpdateDisplay();
        }
    }

    /// <summary>
    /// update original remaining number when finding blue
    /// </summary>
    [HarmonyPatch(typeof(HexScoring), nameof(HexScoring.CorrectTileFound))]
    class HexScoringBlueUncoveredPatch
    {
        static void Postfix(HexScoring __instance)
        {
            HexScoringStartPatch.UpdateOriginalText();
        }
    }
}