using Verse;

namespace RSA
{
    public class Settings : RSA.Core.Settings
    {

        public static bool EnableOutfitFilter = true;
        public static bool EnableCraftingFilter = true;


        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref EnableOutfitFilter, nameof(EnableOutfitFilter), true);
            Scribe_Values.Look(ref EnableCraftingFilter, nameof(EnableCraftingFilter), true);

        }
    }

}
