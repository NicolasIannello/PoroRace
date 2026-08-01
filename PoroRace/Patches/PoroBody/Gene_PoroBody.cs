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

            if (pawn.story != null) pawn.story.bodyType = pawn.story.Adulthood.BodyTypeFor(pawn.gender);
            if (pawn.story == null) pawn.story.bodyType = pawn.gender == Gender.Female ? BodyTypeDefOf.Female : BodyTypeDefOf.Male;

            pawn.Drawer?.renderer?.SetAllGraphicsDirty();
            pawn.Drawer?.renderer?.renderTree?.SetDirty();
        }

        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            base.Notify_IngestedThing(thing, numTaken);

            if (thing.def.defName == "Poro_Porosnack")
            {
                HediffDef Porosnack_Hediff = DefDatabase<HediffDef>.GetNamedSilentFail("Porosnack_Hediff");
                HediffDef PorosnackRecovery_Hediff = DefDatabase<HediffDef>.GetNamedSilentFail("PorosnackRecovery_Hediff");

                Hediff existingPorosnack_Hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Porosnack_Hediff);
                Hediff existingPorosnackRecovery_Hediff = pawn.health.hediffSet.GetFirstHediffOfDef(PorosnackRecovery_Hediff);

                if (existingPorosnackRecovery_Hediff == null && pawn.ageTracker.AgeBiologicalYears>=13)
                {
                    if (existingPorosnack_Hediff != null)
                    {
                        existingPorosnack_Hediff.Severity += (0.33f * numTaken);

                        if (existingPorosnack_Hediff.Severity >= 1.0f && pawn.Spawned)
                        {
                            Map map = pawn.Map;
                            IntVec3 pos = pawn.Position;

                            GenExplosion.DoExplosion(
                                center: pos, map: map, radius: 1.9f, damType: DamageDefOf.Extinguish, instigator: pawn, damAmount: -1, armorPenetration: -1f,
                                explosionSound: null, weapon: null, projectile: null, intendedTarget: null, postExplosionSpawnThingDef: ThingDefOf.Filth_FireFoam,
                                postExplosionSpawnChance: 1f, postExplosionSpawnThingCount: 1
                            );

                            PawnKindDef poroKind = DefDatabase<PawnKindDef>.GetNamedSilentFail("PoroColonist");
                            int spawnCount = Rand.RangeInclusive(1, 3);
                            for (int i = 0; i < spawnCount; i++)
                            {
                                Pawn pawnGen = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                                    poroKind, faction: pawn.Faction, forceGenerateNewPawn: true, allowDowned: false, allowPregnant: false, forceNoGear: true,
                                    dontGiveWeapon: true, allowFood: false, certainlyBeenInCryptosleep: false, fixedBiologicalAge: 3.1f, fixedChronologicalAge: 3.1f
                                ));

                                GenSpawn.Spawn(pawnGen, pos, map);
                            }

                            pawn.health.RemoveHediff(existingPorosnack_Hediff);
                            pawn.health.AddHediff(PorosnackRecovery_Hediff);
                        }
                    }
                    else
                    {
                        Hediff newHediff = pawn.health.AddHediff(Porosnack_Hediff);
                        newHediff.Severity = (0.40f * numTaken);
                    }
                }
            }
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

    public class Hediff_Porosnack : HediffWithComps
    {
        public override string SeverityLabel => this.Severity.ToStringPercent();
    }
}
