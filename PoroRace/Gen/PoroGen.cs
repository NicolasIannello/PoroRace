using PoroRace.UI;
using RimWorld;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PoroRace
{
    public class Gene_PoroBody : Gene
    {
        readonly string body = "Naked_Poro";
        public override void PostAdd()
        {
            base.PostAdd();

            if (pawn.story != null && Active)
            {
                BodyTypeDef poroBody = DefDatabase<BodyTypeDef>.GetNamedSilentFail(body);
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

                if (existingPorosnackRecovery_Hediff == null && pawn.ageTracker.AgeBiologicalYears >= 13)
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

    public class Gene_FablePoroBody : Gene_PoroBody
    {
        readonly string body = "Naked_FablePoro";
    }

    [StaticConstructorOnStartup]
    public class Gene_PoroflyBody : Gene_PoroBody
    {
        public float energy;
        protected int ticksToReset = -1;
        protected int lastKeepDisplayTick = -9999;
        private Vector3 impactAngleVect;
        private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.MoteGlow, new Color(0.96f, 0.39f, 0.8f));
        public float EnergyMax = 100f;
        private float EnergyGainPerTick = 2 / 60f;
        public int EnergyReset = 3200;
        private int KeepDisplayingTicks = 1000;
        private int lastAbsorbDamageTick = -9999;
        public static readonly ConditionalWeakTable<Pawn, Gene_PoroflyBody> PoroflyWeakTable = new ConditionalWeakTable<Pawn, Gene_PoroflyBody>();
        public ShieldState ShieldState
        {
            get
            {
                if (pawn is Pawn p && (p.IsCharging() || p.IsSelfShutdown()))
                {
                    return ShieldState.Disabled;
                }
                CompCanBeDormant comp = pawn.GetComp<CompCanBeDormant>();
                if (comp != null && !comp.Awake)
                {
                    return ShieldState.Disabled;
                }
                if (ticksToReset <= 0)
                {
                    return ShieldState.Active;
                }
                return ShieldState.Resetting;
            }
        }
        protected bool ShouldDisplay
        {
            get
            {
                if (!pawn.Spawned || pawn.Dead || pawn.Downed)
                {
                    return false;
                }
                if (pawn.InAggroMentalState)
                {
                    return true;
                }
                if (pawn.Drafted)
                {
                    return true;
                }
                if (pawn.Faction.HostileTo(Faction.OfPlayer) && !pawn.IsPrisoner)
                {
                    return true;
                }
                if (Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks)
                {
                    return true;
                }
                if (ModsConfig.BiotechActive && pawn.IsColonyMech && Find.Selector.SingleSelectedThing == pawn)
                {
                    return true;
                }
                return false;
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            PoroflyWeakTable.Remove(this.pawn);
            PoroflyWeakTable.Add(this.pawn, this);
        }

        public override void PostRemove()
        {
            base.PostRemove();
            PoroflyWeakTable.Remove(this.pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref energy, "energy", 0f);
            Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
            Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick", 0);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && this.pawn != null) PoroflyWeakTable.Add(this.pawn, this);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerable<Gizmo> gizmos = base.GetGizmos();
            if (gizmos != null) foreach (var g in gizmos) yield return g;
            if (Find.Selector.SingleSelectedThing == pawn) yield return new Gizmo_GeneShieldStatus { shieldGene = this };
        }

        public override void Tick()
        {
            base.Tick();
            if (ShieldState == ShieldState.Resetting)
            {
                ticksToReset--;
                if (ticksToReset <= 0)
                {
                    ResetShield();
                }
            }
            else if (ShieldState == ShieldState.Active)
            {
                energy += EnergyGainPerTick;
                if (energy > EnergyMax)
                {
                    energy = EnergyMax;
                }
            }
        }

        private void ResetShield()
        {
            SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            FleckMaker.ThrowLightningGlow(pawn.TrueCenter(), pawn.Map, 3f);
            ticksToReset = -1;
            energy = EnergyMax/4;
        }

        public void Break()
        {
            if (pawn.Spawned)
            {
                float scale = Mathf.Lerp(1.2f, 1.5f, energy);
                EffecterDefOf.Shield_Break.SpawnAttached(pawn, pawn.MapHeld, scale);
                FleckMaker.Static(pawn.TrueCenter(), pawn.Map, FleckDefOf.ExplosionFlash, 12f);
                for (int i = 0; i < 6; i++)
                {
                    FleckMaker.ThrowDustPuff(pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), pawn.Map, Rand.Range(0.8f, 1.2f));
                }
            }
            energy = 0f;
            ticksToReset = EnergyReset;
        }

        public void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = pawn.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(loc, pawn.Map, FleckDefOf.ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                FleckMaker.ThrowDustPuff(loc, pawn.Map, Rand.Range(0.8f, 1.2f));
            }
            lastAbsorbDamageTick = Find.TickManager.TicksGame;
            //KeepDisplaying();
        }

        public void Draw()
        {
            if (ShieldState == ShieldState.Active && ShouldDisplay)
            {
                float num = Mathf.Lerp(1.2f, 1.5f, energy);
                Vector3 drawPos = pawn.Drawer.DrawPos;
                drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    drawPos += impactAngleVect * num3;
                    num -= num3;
                }
                float angle = Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }
        }
    }
}