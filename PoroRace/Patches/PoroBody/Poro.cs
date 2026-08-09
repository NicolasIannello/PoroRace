using HarmonyLib;
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

                if (learnedSkill == SkillDefOf.Construction || learnedSkill == SkillDefOf.Crafting) EvolvePoro(user, user.skills.GetSkill(__instance.Props.skill).GetLevel(), PoroRaceDefOf.FablePoro_XenoType);
                if (learnedSkill == SkillDefOf.Medicine) EvolvePoro(user, user.skills.GetSkill(__instance.Props.skill).GetLevel(), PoroRaceDefOf.Porofly_XenoType);
                if (learnedSkill == SkillDefOf.Melee) EvolvePoro(user, user.skills.GetSkill(__instance.Props.skill).GetLevel(), PoroRaceDefOf.ProtoPoro_XenoType);
            }
        }

        public static void EvolvePoro(Pawn poro, int level, XenotypeDef xenotype)
        {
            int range = Rand.RangeInclusive(1, 100);
            int chance = 15 + level * 2;
            //Log.Message("Chance: " + chance + " Range: " + range);

            if (chance > range)
            {
                poro.genes.SetXenotype(xenotype);

                for (int i = poro.genes.GenesListForReading.Count - 1; i >= 0; i--)
                {
                    poro.genes.RemoveGene(poro.genes.GenesListForReading[i]);
                }

                foreach (GeneDef geneDef in xenotype.genes)
                {
                    poro.genes.AddGene(geneDef, xenogene: false);
                }

                string label = "PoroRace.PoroEvolveLabel".Translate();
                string text = "PoroRace.PoroEvolveText".Translate(poro.Name.ToStringShort, xenotype.LabelCap);
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, new LookTargets(poro));
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
            if(__instance.GetCurrentTitle(faction).seniority>200 && __instance.pawn.Faction == Faction.OfPlayer) Patch_CompUseEffect_LearnSkill_DoEffect.EvolvePoro(__instance.pawn, 100, PoroRaceDefOf.PoroKing_XenoType);
        }
    }
}