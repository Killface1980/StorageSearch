using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RSA.HaulingHysterisis;
using RSA.Languages;
using UnityEngine;
using Verse;

namespace StorageSearch
{

    [HarmonyPatch(typeof(ThingFilterUI), nameof(ThingFilterUI.DoThingFilterConfigWindow))]
    class HaulingHysteresis_InjectControls {

        private const float HysteresisHeight = 30f;
        private const float HysteresisBlockHeight = 35f;

        internal static volatile int showHysteresisCount;

        private static readonly Queue<StorageSettings> _settingsQueue = new Queue<StorageSettings>();

        internal static Queue<StorageSettings> SettingsQueue => _settingsQueue;

        [HarmonyPrefix]
        public static void Before_DoThingFilterConfigWindow(ref object __state, ref Rect rect) {
            bool showHysteresis = (showHysteresisCount-- > 0) && _settingsQueue.Count != 0;
            showHysteresisCount = Math.Max(0, showHysteresisCount);

            if (showHysteresis)
            {                
                DoHysteresisBlock(new Rect(0f, rect.yMax - HysteresisHeight, rect.width, HysteresisHeight), _settingsQueue.Dequeue());

                rect = new Rect(rect.x, rect.y, rect.width, rect.height - HysteresisBlockHeight);
            }
        }        

        private static void DoHysteresisBlock(Rect rect, StorageSettings settings) {

            StorageSettings_Hysteresis storageSettings_Hysteresis = StorageSettings_Mapping.Get(settings) ?? new StorageSettings_Hysteresis();

            storageSettings_Hysteresis.FillPercent = Widgets.HorizontalSlider(rect.LeftPart(0.8f), storageSettings_Hysteresis.FillPercent, 0f, 100f, false, RSAKeys.HaulingHysteresis_RefillCellsLabel.Translate());
            Widgets.Label(rect.RightPart(0.2f), storageSettings_Hysteresis.FillPercent.ToString("N0") + "%");
            StorageSettings_Mapping.Set(settings, storageSettings_Hysteresis); 
        }        
    }
}
