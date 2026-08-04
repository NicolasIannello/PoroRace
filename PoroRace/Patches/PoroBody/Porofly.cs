using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PoroRace.Patches.PoroBody
{
    [HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell", typeof(Pawn), typeof(IntVec3))]
    public static class Patch_Pawn_PathFollower_CostToMoveIntoCell
    {
        public static void Postfix(Pawn pawn, IntVec3 c, ref float __result)
        {
            if (pawn?.genes == null) return;

            if (pawn.genes.HasActiveGene(PoroRaceDefOf.Porofly_wings) && __result > 15) __result = 15;
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
    public static class Patch_Pawn_HealthTracker_PreApplyDamage
    {
        public static bool Prefix(Pawn_HealthTracker __instance, ref DamageInfo dinfo, out bool absorbed, Pawn ___pawn)
        {
            absorbed = false;
            if (___pawn?.genes == null) return true;

            Gene_PoroflyBody shield = ___pawn.genes.GetFirstGeneOfType<Gene_PoroflyBody>();
            if (shield != null && shield.ShieldState == ShieldState.Active)
            {
                if (dinfo.Def == DamageDefOf.EMP)
                {
                    shield.Break();
                    return true;
                }
                if (!dinfo.Def.ignoreShields && (dinfo.Def.isRanged || dinfo.Def.isExplosive))
                {
                    shield.energy -= dinfo.Amount * 3.33f;
                    if (shield.energy <= 0f) shield.Break();
                    else shield.AbsorbedDamage(dinfo);

                    absorbed = true;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "DrawAt")]
    public static class Patch_Pawn_DrawAt
    {
        public static void Postfix(Pawn __instance, Vector3 drawLoc)
        {
            if (__instance.genes != null)
            {
                Gene_PoroflyBody shield = __instance.genes.GetFirstGeneOfType<Gene_PoroflyBody>();
                shield?.Draw();
            }
        }
    }
}
