using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        // Some storage mods allow more than one thing in a cell.  If they do, we need to do
        //   more of a check to see if the threshold has been met; we only check if we need to:
        public static bool checkIHoldMultipleThings=false;
        public static bool Prepare() {
            if (ModLister.GetActiveModWithIdentifier("LWM.DeepStorage")!=null) {
                checkIHoldMultipleThings=true;
                Log.Message("RSA activating compatibility for LWM.DeepStorage");
            }
            //  If other storage mods don't work, add further tests for them here:
            return true;
        }

        [HarmonyPostfix]
        public static void FilledEnough(ref bool __result, IntVec3 c, Map map, Thing thing) {
            // if base implementation waves of, then don't need to care
            if (__result) {
                float num = 100f;
                SlotGroup slotGroup=c.GetSlotGroup(map);

                bool flag = slotGroup != null && slotGroup.Settings != null;
                if (flag)
                {
                    num = StorageSettings_Mapping.Get(slotGroup.Settings).FillPercent;
                }

                //LWM.DeepStorage
                if (checkIHoldMultipleThings) {
                    // NOTE: Other storage mods may not be comp-based.  If one ever starts causing
                    //   problems with this mod, the logic here can be updated to include checking
                    //   whether the storage building itself is IHoldMultipleThings
                    foreach(Thing thisthing in map.thingGrid.ThingsListAt(c))
                    {
                        ThingWithComps th = thisthing as ThingWithComps;
                        if (th == null) continue;
                        var allComps = th.AllComps;

                        if (allComps != null)
                        {
                            foreach (var comp in allComps)
                            {
                                if (comp is IHoldMultipleThings.IHoldMultipleThings)
                                {
                                    int capacity = 0;
                                    IHoldMultipleThings.IHoldMultipleThings thiscomp = (IHoldMultipleThings.IHoldMultipleThings)comp;

                                    thiscomp.CapacityAt(thing, c, map, out capacity);
                                    // if total capacity is larger than the stackLimit (full stack available)
                                    //    Allow hauling (other choices are valid)
                                    // if (capacity > thing.def.stackLimit) return true;
                                    // only haul if count is below threshold
                                    //   which is equivalent to availability being above threshold:
                                    //            Log.Message("capacity = " + capacity);
                                    //            Log.Message("thing.def.stackLimit = " +thing.def.stackLimit);
                                    float var = (100f * (float)capacity / thing.def.stackLimit);

                                    //100 - num is necessary because capacity gives empty space not full space
                                    __result = var > (100 - num);
                                    //      if (__result == false){
                                    //          Log.Message("ITS TOO FULL stop");
                                    //      }
                                    return;
                                }
                            }
                        }

                    }

                }

                __result &= !map.thingGrid.ThingsListAt(c).Any(t => t.def.EverStorable(false) && t.stackCount >= thing.def.stackLimit*(num/100f));
            }
        }
    }
}
