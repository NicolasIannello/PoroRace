using HarmonyLib;
using PoroRace.Utils;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace PoroRace.Patches
{
    [HarmonyPatch(typeof(PsychicRitualToil_Philophagy), "ApplyOutcome")]
    public static class Patch_PsychicRitualToil_Philophagy_ApplyOutcome
    {
        public static void Prefix(Pawn invoker, Pawn target)
        {
            if (invoker.genes != null && invoker.genes.HasActiveGene(PoroRaceDefOf.Gene_PoroBody))
            {
                SkillDef skill = PsychicRitualUtility.GetPhilophagySkillAndXpTransfer(invoker, target, 0, out _);
                if (skill != null) PoroUtility.CanEvolvePoro(invoker.skills.GetSkill(skill).def, invoker, invoker.skills.GetSkill(skill).GetLevel() + 20);
            }
        }
    }

    [HarmonyPatch(typeof(PsychicRitualGraph), nameof(PsychicRitualGraph.End))]
    public static class Patch_PsychicRitualGraph_End
    {
        public static void Postfix(PsychicRitual psychicRitual, PsychicRitualGraph parent, bool success)
        {
            if (psychicRitual.succeeded)
            {
                Pawn invoker = psychicRitual.assignments.FirstAssignedPawn(PsychicRitualRoleDefOf.Invoker);
                if (invoker!=null && invoker.genes != null && invoker.genes.HasActiveGene(PoroRaceDefOf.Gene_PoroBody)) PoroUtility.EvolvePoro(invoker, (int)(psychicRitual.power*100)/2-13, PoroRaceDefOf.Poroid_XenoType);
            }
        }
    }
}