using Verse;

namespace RSA.Core {
    public class Settings : ModSettings {

        public const float DefaultSearchWidth = 0.4f;

        public static bool IncludeParentCategory = true;
        public static float SearchWidth = DefaultSearchWidth;


        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref IncludeParentCategory, nameof(IncludeParentCategory), false);
            Scribe_Values.Look(ref SearchWidth, nameof(SearchWidth), DefaultSearchWidth, false);
        }
    }
}
