using HarmonyLib;
using RimWorld;
using Verse;

namespace PoroRace
{
    [HarmonyPatch(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip))]
    [HarmonyPatch(new[] { typeof(Thing), typeof(Pawn), typeof(string), typeof(bool) },
        new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
    public static class Patch_EquipmentUtility_CanEquip
    {
        public static void Postfix(Thing thing, Pawn pawn, ref bool __result, ref string cantReason)
        {
            if (!__result) return;

            if (thing.def?.apparel != null && pawn.genes != null)
            {
                if (PoroApparelHelper.CanWearPoroApparel(thing.def.apparel, pawn, "CanEquip", ref cantReason)) __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreGain))]
    public static class Patch_JobGiver_OptimizeApparel_ApparelScoreGain
    {
        public static void Postfix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (ap.def?.apparel != null && pawn.genes != null)
            {
                if (PoroApparelHelper.CanWearPoroApparel(ap.def.apparel, pawn, "ScoreGain")) __result = -1000f;
            }
        }
    }

    [HarmonyPatch(typeof(ApparelProperties), nameof(ApparelProperties.PawnCanWear), typeof(Pawn), typeof(bool))]
    public static class Patch_ApparelProperties_PawnCanWear
    {
        public static void Postfix(ApparelProperties __instance, Pawn pawn, ref bool __result)
        {
            if (!__result || pawn?.genes == null) return;

            if(PoroApparelHelper.CanWearPoroApparel(__instance, pawn, "PawnCanWear")) __result = false;
        }
    }

    public static class PoroApparelHelper
    {
        public static bool CanWearPoroApparel(ApparelProperties apparel, Pawn pawn, string log)
        {
            string dummyReason = null;
            return CanWearPoroApparel(apparel, pawn, log, ref dummyReason);
        }
        public static bool CanWearPoroApparel(ApparelProperties apparel, Pawn pawn, string log, ref string cantReason)
        {
            bool isPoro = pawn.genes.HasActiveGene(PoroRaceDefOf.Gene_PoroMetabolism);
            var layers = apparel.layers;
            bool poroApparel = apparel.tags.Contains("PoroApparel") || apparel.tags.Contains("PoroArmor");
            bool headOrBelt = layers.Contains(ApparelLayerDefOf.Overhead) || layers.Contains(ApparelLayerDefOf.Belt) || layers.Contains(ApparelLayerDefOf.EyeCover);
            bool torsoOrLegs = layers.Contains(ApparelLayerDefOf.Middle) || layers.Contains(ApparelLayerDefOf.OnSkin) || layers.Contains(ApparelLayerDefOf.Shell);

            cantReason= null;

            if (isPoro && (!poroApparel && (!headOrBelt || torsoOrLegs)))
            {
                cantReason = "Poro cannot wear this.";
                return true;
            }
            else if(!isPoro && poroApparel)
            {
                cantReason = "Cannot wear Poro apparel.";
                return true;
            }

            return false;
        }
    }
}
