using HarmonyLib;
using RimWorld;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace PoroRace.Patches.PoroBody
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Head), nameof(PawnRenderNodeWorker_Head.OffsetFor))]
    public static class Patch_PawnRenderNodeWorker_Head_OffsetFor
    {
        private static readonly ConditionalWeakTable<Pawn, GeneCacheData> pawnGeneCache = new ConditionalWeakTable<Pawn, GeneCacheData>();
        private class GeneCacheData
        {
            public bool hasPoroGene;
            public int lastCheckedFrame = -9999;
        }

        public static void Postfix(PawnDrawParms parms, ref Vector3 __result)
        {
            Pawn pawn = parms.pawn;

            if (pawn?.genes == null) return;

            if (!pawnGeneCache.TryGetValue(pawn, out GeneCacheData cache))
            {
                cache = new GeneCacheData();
                pawnGeneCache.Add(pawn, cache);
            }

            int currentFrame = Time.frameCount;

            if (currentFrame > cache.lastCheckedFrame + 90)
            {
                cache.hasPoroGene = pawn.genes.HasActiveGene(PoroRaceDefOf.Gene_FablePoroBody);
                cache.lastCheckedFrame = currentFrame;
            }

            if (cache.hasPoroGene) __result.z -= 0.3f;
        }
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