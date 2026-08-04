using UnityEngine;
using Verse;

namespace PoroRace.UI
{
    [StaticConstructorOnStartup]
    public class Gizmo_GeneShieldStatus : Gizmo
    {
        public Gene_PoroflyBody shieldGene;
        private static readonly Texture2D FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        private static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public override float GetWidth(float maxWidth) => 140f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(rect);
            Rect innerRect = rect.ContractedBy(6f);

            Rect textRect = innerRect;
            textRect.height = innerRect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(textRect, "Spellshield");

            Rect barRect = innerRect;
            barRect.yMin = innerRect.y + innerRect.height / 2f;
            float fillPercent = shieldGene.energy / Mathf.Max(1f, shieldGene.EnergyMax);
            Widgets.FillableBar(barRect, fillPercent, FullTex, EmptyTex, false);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, shieldGene.energy.ToString("F0") + " / " + shieldGene.EnergyMax.ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
