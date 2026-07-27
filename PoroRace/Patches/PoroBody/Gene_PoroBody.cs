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

            if (pawn.story != null)
            {
                BodyTypeDef poroBody = DefDatabase<BodyTypeDef>.GetNamedSilentFail("Naked_Poro");
                if (poroBody != null)
                {
                    pawn.story.bodyType = poroBody;

                    pawn.Drawer?.renderer?.SetAllGraphicsDirty();
                    pawn.Drawer?.renderer?.renderTree?.SetDirty();
                }
            }
        }
    }

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
}
