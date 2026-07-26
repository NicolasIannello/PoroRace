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

            if (ShouldPreventWearing(thing.def, pawn, out string reason))
            {
                __result = false;
                cantReason = reason;
            }
        }

        private static bool ShouldPreventWearing(ThingDef thingDef, Pawn pawn, out string reason)
        {
            reason = null;

            if (thingDef?.apparel == null) return false;
            if (pawn.genes == null) return false;
            GeneDef poroGene = DefDatabase<GeneDef>.GetNamedSilentFail("Gene_PoroBody");
            if (poroGene == null) return false;

            bool isPoro = pawn.genes.HasActiveGene(poroGene);
            bool isHatOrBelt = (thingDef.apparel.layers != null &&
                (thingDef.apparel.layers.Contains(ApparelLayerDefOf.Overhead) || thingDef.apparel.layers.Contains(ApparelLayerDefOf.Belt)));

            if (isHatOrBelt) return false;

            bool isPoroClothing = thingDef.apparel.tags != null && thingDef.apparel.tags.Contains("PoroApparel");

            if ((isPoro && !isPoroClothing) || (!isPoro && isPoroClothing))
            {
                reason = "Incompatible body type for this apparel.";
                return true;
            }

            return false;
        }
    }
}
