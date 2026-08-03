using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace PoroRace
{
    public class Gene_FablePoroBody : Gene_PoroBody
    {
        readonly string body = "Naked_FablePoro";
    }

    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new Type[] { typeof(Pawn), typeof(SkillDef), typeof(bool) })]
    public static class GenerateQualityCreatedByPawn
    {
        public static void Postfix(Pawn pawn, SkillDef relevantSkill, bool consumeInspiration, ref QualityCategory __result)
        {
            if (pawn?.genes == null) return;

            if (pawn.genes.HasActiveGene(PoroRaceDefOf.Gene_FablePoroBody))
            {
                if (__result < QualityCategory.Legendary) __result = (QualityCategory)((int)__result + 1);
            }
        }
    }
}