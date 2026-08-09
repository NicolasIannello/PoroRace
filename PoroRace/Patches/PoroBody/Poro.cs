using HarmonyLib;
using PoroRace.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace PoroRace
{
    //HAR patch
    [HarmonyPatch(typeof(PawnRenderNode_Body), nameof(PawnRenderNode_Body.GraphicFor))]
    public static class Patch_PawnRenderNode_Body_GraphicFor
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn pawn, ref Graphic __result)
        {
            if (pawn.story?.bodyType == null) return true;

            if (pawn.story.bodyType.defName == PoroRaceDefOf.Naked_Poro.defName)
            {
                string path = PoroRaceDefOf.Naked_Poro.bodyNakedGraphicPath;

                __result = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutSkin, Vector2.one, pawn.story.SkinColor);

                return false;
            }
            else if (pawn.story.bodyType.defName == PoroRaceDefOf.Naked_FablePoro.defName)
            {
                string path = PoroRaceDefOf.Naked_FablePoro.bodyNakedGraphicPath;

                __result = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutSkin, Vector2.one, pawn.story.SkinColor);

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(CompUseEffect_LearnSkill), nameof(CompUseEffect_LearnSkill.DoEffect))]
    public static class Patch_CompUseEffect_LearnSkill_DoEffect
    {
        public static void Postfix(CompUseEffect_LearnSkill __instance, Pawn user)
        {
            if (user.genes != null && user.genes.HasActiveGene(PoroRaceDefOf.Gene_PoroBody))
            {
                SkillDef learnedSkill = __instance.Props.skill;
                PoroUtility.CanEvolvePoro(learnedSkill, user, user.skills.GetSkill(learnedSkill).GetLevel());
            }
        }
    }

    [HarmonyPatch(typeof(TraderCaravanUtility), nameof(TraderCaravanUtility.GetTraderCaravanRole))]
    public static class Patch_TraderCaravanUtility_GetTraderCaravanRole
    {
        public static void Postfix(Pawn p, ref TraderCaravanRole __result)
        {
            if (p.kindDef == PoroRaceDefOf.PoroSlave) __result = TraderCaravanRole.Chattel;
        }
    }

    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), "OnPostTitleChanged")]
    public static class Patch_Pawn_RoyaltyTracker_OnPostTitleChanged
    {
        public static void Postfix(Pawn_RoyaltyTracker __instance, Faction faction)
        {
            if (__instance.pawn.genes != null && __instance.pawn.genes.HasActiveGene(PoroRaceDefOf.Gene_PoroBody))
            {
                PoroUtility.CanEvolvePoro(__instance.GetCurrentTitle(faction).seniority, __instance.pawn, 100);
            }
        }
    }
}