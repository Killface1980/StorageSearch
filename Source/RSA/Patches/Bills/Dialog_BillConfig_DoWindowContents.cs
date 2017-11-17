using Harmony;
using RimWorld;
using RSA.Core;
using UnityEngine;
using Verse;

namespace RSA {
    [HarmonyPatch(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents))]
    class Dialog_BillConfig_DoWindowContents
    {

        [HarmonyPrefix]
        public static void Before_DoWindowContents(Rect inRect) {
            if (!Settings.EnableCraftingFilter)
                return;

            ThingFilterUtil.QueueNextInvocationSearch(SearchCategories.Bill);
        }
    }
}
