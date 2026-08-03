using HarmonyLib;
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

            if (pawn.story.bodyType.defName == "Naked_Poro")
            {
                string path = "Things/Pawn/Humanlike/Bodies/Naked_Poro";

                __result = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutSkin, Vector2.one, pawn.story.SkinColor);

                return false;
            } 
            else if (pawn.story.bodyType.defName == "Naked_FablePoro") 
            {
                string path = "Things/Pawn/Humanlike/Bodies/Naked_FablePoro";

                __result = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutSkin, Vector2.one, pawn.story.SkinColor);

                return false;
            }

            return true;
        }
    }


}
