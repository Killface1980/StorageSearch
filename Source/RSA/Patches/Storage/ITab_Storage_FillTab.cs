using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using RSA.Core;
using RSA.Core.Util;
using StorageSearch;
using UnityEngine;
using Verse;

namespace RSA
{
    [HarmonyPatch(typeof(ITab_Storage), "FillTab")]
    public class ITab_Storage_FillTab {

        private static Func<RimWorld.ITab_Storage, IStoreSettingsParent> GetSelStoreSettingsParent;


        static ITab_Storage_FillTab() {
            GetSelStoreSettingsParent = Access.GetPropertyGetter<RimWorld.ITab_Storage, IStoreSettingsParent>("SelStoreSettingsParent");
        }


        [HarmonyPrefix]
        public static void Before_ITab_Storage_FillTab(ITab_Storage __instance) {
            ThingFilterUtil.QueueNextInvocationSearch(SearchCategories.Storage);

            if (ReferenceEquals(__instance.GetType().Assembly, typeof(ITab_Storage).Assembly))
            {
                // only show hysteresis option for non derived (non-custom) storage(s)
                HaulingHysteresis_InjectControls.showHysteresisCount++;

                IStoreSettingsParent selStoreSettingsParent =  GetSelStoreSettingsParent(__instance);
                HaulingHysteresis_InjectControls.SettingsQueue.Enqueue(selStoreSettingsParent.GetStoreSettings());
            }
        }
    }
}
