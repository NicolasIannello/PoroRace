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

            if(thing.def?.apparel != null && pawn.genes !=null)
            {
                GeneDef poroGene = DefDatabase<GeneDef>.GetNamedSilentFail("Gene_PoroBody");
                var layers = thing.def.apparel.layers;
                if (poroGene != null && layers != null && thing.def.apparel.tags != null) 
                {
                    bool poroApparel = thing.def.apparel.tags.Contains("PoroApparel");
                    bool headOrBelt = layers.Contains(ApparelLayerDefOf.Overhead) || layers.Contains(ApparelLayerDefOf.Belt) || layers.Contains(ApparelLayerDefOf.EyeCover);
                    bool torsoOrLegs = layers.Contains(ApparelLayerDefOf.Middle) || layers.Contains(ApparelLayerDefOf.OnSkin) || layers.Contains(ApparelLayerDefOf.Shell);
                    bool isPoro = pawn.genes.HasActiveGene(poroGene);
                    if (isPoro && (!poroApparel && (!headOrBelt || torsoOrLegs)) )
                    {
                        cantReason = "Poro cannot wear this.";
                        __result = false;
                    }
                    else if(!isPoro && poroApparel)
                    {
                        cantReason = "Cannot wear Poro apparel.";
                        __result = false;
                    }
                }
            }
        }
    }
}
