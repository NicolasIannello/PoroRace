using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace PoroRace
{
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new Type[] { typeof(Pawn), typeof(SkillDef), typeof(bool) })]
    public static class Patch_QualityUtility_GenerateQualityCreatedByPawn
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