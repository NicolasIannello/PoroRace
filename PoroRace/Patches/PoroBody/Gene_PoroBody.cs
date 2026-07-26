using RimWorld;
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
                pawn.story.bodyType = DefDatabase<BodyTypeDef>.GetNamed("PoroBodyTypeDef");

                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }
        }
    }
}
