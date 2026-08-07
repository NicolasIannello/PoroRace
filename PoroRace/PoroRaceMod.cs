using HarmonyLib;
using RimWorld;
using Verse;

namespace PoroRace
{
    public class PoroRaceMod : Mod
    {
        public static Harmony Harmony;
        public static PoroRaceMod Instance;

        public PoroRaceMod(ModContentPack content) : base(content)
        {
            Harmony = new Harmony("com.eximeisty.pororace");
            Harmony.PatchAll();
            Instance = this;
        }

        public override string SettingsCategory()
        {
            return "PoroRace";
        }
  
    }

    [DefOf]
    public static class PoroRaceDefOf
    {
        public static GeneDef Gene_PoroMetabolism;
        public static GeneDef Gene_FablePoroBody;
        public static GeneDef Gene_PoroBody;
        public static GeneDef Porofly_wings;
        public static XenotypeDef FablePoro_XenoType;
        public static XenotypeDef Porofly_XenoType;
        public static XenotypeDef ProtoPoro_XenoType;
        public static PawnKindDef PoroSlave;

        static PoroRaceDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PoroRaceDefOf));
        }
    }

}