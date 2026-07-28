using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PoroRace
{
    public class Gene_PoroBody : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();

            if (pawn.story != null && Active)
            {
                BodyTypeDef poroBody = DefDatabase<BodyTypeDef>.GetNamedSilentFail("Naked_Poro");
                if (poroBody != null)
                {
                    if (pawn.apparel != null)
                    {
                        for (int i = pawn.apparel.WornApparel.Count - 1; i >= 0; i--)
                        {
                            Apparel apparel = pawn.apparel.WornApparel[i];
                            var layers = apparel.def.apparel.layers;

                            if (!layers.Contains(ApparelLayerDefOf.Overhead) || !layers.Contains(ApparelLayerDefOf.EyeCover) || !apparel.def.apparel.tags.Contains("PoroApparel"))
                            {
                                pawn.apparel.Remove(apparel);

                                if (pawn.inventory != null && pawn.inventory.innerContainer != null)
                                {
                                    pawn.inventory.innerContainer.TryAddOrTransfer(apparel);
                                }
                            }
                        }
                    }

                    pawn.story.bodyType = poroBody;

                    pawn.Drawer?.renderer?.SetAllGraphicsDirty();
                    pawn.Drawer?.renderer?.renderTree?.SetDirty();
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();

            Log.Message("PostRemove: "+pawn.story.bodyType);
            pawn.story.bodyType = null;
        }

        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            base.Notify_IngestedThing(thing, numTaken);


        }

    }
    
    //HAR patch
    [HarmonyPatch(typeof(PawnRenderNode_Body), nameof(PawnRenderNode_Body.GraphicFor))]
    public static class Patch_PawnRenderNode_Body_GraphicFor
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn pawn, ref Graphic __result)
        {
            if (pawn.story?.bodyType == null) return true;

            if (pawn.story.bodyType.defName == "Naked_Poro")
            {
                string path = "Things/Pawn/Humanlike/Bodies/Naked_Poro";

                __result = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutSkin, Vector2.one, pawn.story.SkinColor);

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ApparelProperties), nameof(ApparelProperties.PawnCanWear), typeof(Pawn), typeof(bool))]
    public static class Patch_ApparelProperties_PawnCanWear
    {
        public static void Postfix(ApparelProperties __instance, Pawn pawn, ref bool __result)
        {
            if (!__result || pawn?.genes == null) return;

            Gene_PoroBody poroGene = pawn.genes.GetFirstGeneOfType<Gene_PoroBody>();
            bool isPoro = (poroGene != null && pawn.genes.HasActiveGene(poroGene.def));

            if (isPoro && (!__instance.layers.Contains(ApparelLayerDefOf.Overhead) && !__instance.tags.Contains("PoroApparel"))) __result = false;
        }
    }
}
