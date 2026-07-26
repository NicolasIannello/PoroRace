using HarmonyLib;
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
    
}
