using System;
using HarmonyLib;
using RimWorld;
using RSA.Core;
using RSA.Core.Util;
using UnityEngine;

namespace RSA
{
    [HarmonyPatch(typeof(Dialog_ManageOutfits), nameof(Dialog_ManageOutfits.DoWindowContents))]
    class DialogManageOutfits_DoWindowContents
    {

        private static readonly Func<Dialog_ManageOutfits, Outfit> GetSelectedOutfit;

        static DialogManageOutfits_DoWindowContents() {
            GetSelectedOutfit = Access.GetPropertyGetter<Dialog_ManageOutfits, Outfit>("SelectedOutfit");
        }

        [HarmonyPrefix]
        public static void Before_DoWindowContents(Rect inRect, Dialog_ManageOutfits __instance) {
            if (GetSelectedOutfit(__instance) == null)
                return;

            if (!Settings.EnableOutfitFilter)
                return;
            
            ThingFilterUtil.QueueNextInvocationSearch(SearchCategories.Outfit);
        }
    }
}
