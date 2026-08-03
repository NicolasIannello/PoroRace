using Verse;

namespace PoroRace.Things
{
    public class Hediff_Porosnack : HediffWithComps
    {
        public override string SeverityLabel => this.Severity.ToStringPercent();
    }
}
