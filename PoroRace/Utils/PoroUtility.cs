using RimWorld;
using Verse;

namespace PoroRace.Utils
{
    public class PoroUtility
    {
        public static void CanEvolvePoro(SkillDef learnedSkill, Pawn user, int level)
        {
            if (learnedSkill == SkillDefOf.Construction || learnedSkill == SkillDefOf.Crafting) EvolvePoro(user, level, PoroRaceDefOf.FablePoro_XenoType);
            if (learnedSkill == SkillDefOf.Medicine) EvolvePoro(user, level, PoroRaceDefOf.Porofly_XenoType);
            if (learnedSkill == SkillDefOf.Melee) EvolvePoro(user, level, PoroRaceDefOf.ProtoPoro_XenoType);
        }

        public static void CanEvolvePoro(int seniority, Pawn user, int level)
        {
            if (seniority > 200 && user.Faction == Faction.OfPlayer) EvolvePoro(user, level, PoroRaceDefOf.PoroKing_XenoType);
        }

        public static void EvolvePoro(Pawn poro, int level, XenotypeDef xenotype)
        {
            int range = Rand.RangeInclusive(1, 100);
            int chance = 15 + level * 2;
            //Log.Message("Chance: " + chance + " Range: " + range);

            if (chance > range)
            {
                poro.genes.SetXenotype(xenotype);

                for (int i = poro.genes.GenesListForReading.Count - 1; i >= 0; i--)
                {
                    poro.genes.RemoveGene(poro.genes.GenesListForReading[i]);
                }

                foreach (GeneDef geneDef in xenotype.genes)
                {
                    poro.genes.AddGene(geneDef, xenogene: false);
                }

                string label = "PoroRace.PoroEvolveLabel".Translate();
                string text = "PoroRace.PoroEvolveText".Translate(poro.Name.ToStringShort, xenotype.LabelCap);
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, new LookTargets(poro));
            }
        }
    }
}
