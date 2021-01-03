using System.Linq;
using HarmonyLib;
using RimWorld;
using RSA.HaulingHysterisis;
using Verse;

namespace RSA
{
    [HarmonyPatch(typeof(StoreUtility), "NoStorageBlockersIn")]
    internal class StoreUtility_NoStorageBlockersIn
    {

        [HarmonyPostfix]
        public static void FilledEnough(ref bool __result, IntVec3 c, Map map, Thing thing) {
            // if base implementation waves of, then don't need to care
            if (__result) {
                float num = 100f;
                bool flag = c.GetSlotGroup(map) != null && c.GetSlotGroup(map).Settings != null;
                if (flag) {
                    num = StorageSettings_Mapping.Get(c.GetSlotGroup(map).Settings).FillPercent;
                }

                __result &= !map.thingGrid.ThingsListAt(c).Any(t => t.def.EverStorable(false) && t.stackCount >= thing.def.stackLimit*(num/100f));
            }
        }
    }
}
