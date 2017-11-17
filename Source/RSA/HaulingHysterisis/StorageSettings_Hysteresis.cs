using Verse;

namespace RSA.HaulingHysterisis
{
    public class StorageSettings_Hysteresis : IExposable
    {
        public float FillPercent = 100f;

        public void ExposeData()
        {
            Scribe_Values.Look<float>(ref this.FillPercent, "fillPercent", 100f, false);
        }
    }
}
