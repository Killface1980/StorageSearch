using System;
using System.Reflection;
using HarmonyLib;
using RSA.Languages;
using UnityEngine;
using Verse;

namespace RSA.Core
{
    public class RSACoreMod : Mod
    {

        public static bool Debug = false;

        private static readonly ThingFilter filter = new ThingFilter();  // dummy filter for ui settings preview
        private static Vector2 scrollPosition = new Vector2();


        public RSACoreMod(ModContentPack content) : base(content) {
            Harmony harmonyInstance = new Harmony("RSA.Core");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());              // just use all [HarmonyPatch] decorated classes       

            
            Log.Message($"RSA Core {typeof(RSACoreMod).Assembly.GetName().Version} loaded..");

            this.GetSettings<Settings>();

            
        }

        public bool SupressSettings { get; set; }

        public override string SettingsCategory() {
            if (SupressSettings)
                return string.Empty;

            return Debug
                ? RSACoreKeys.RSACore_Debug.Translate(Assembly.GetExecutingAssembly().GetName().Version.ToString())
                : RSACoreKeys.RSACore.Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            Listing_Standard list = new Listing_Standard(GameFont.Small) {
                                        ColumnWidth = inRect.width
                                    };
            list.Begin(inRect);

            DoSettingsContents(list);
            DoPreview(list);

            list.End();
        }

        public static void DoSettingsContents(Listing_Standard list) {
            list.CheckboxLabeled(
                RSACoreKeys.RSACore_IncludeParentCategory.Translate(),
                ref Settings.IncludeParentCategory,
                RSACoreKeys.RSACore_IncludeParentCategoryTip.Translate());

            const float sliderHeight = 30f;

            Rect rect = list.GetRect(sliderHeight);
            Widgets.Label(new Rect(rect.position, new Vector2(rect.width * 0.5f, sliderHeight)), RSACoreKeys.RSACore_SearchWidth.Translate());
            Settings.SearchWidth = Widgets.HorizontalSlider(new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.5f, sliderHeight), Settings.SearchWidth, 0.25f, 1f, false, $"{Settings.SearchWidth:p0}", null, null, -1f);


        }

        public static void DoPreview(Listing_Standard list) {
            list.Gap();

            const float previewHeight = 250f;

            Rect rect = list.GetRect(previewHeight);
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width * 0.5f, Text.LineHeight), RSACoreKeys.RSACore_Preview.Translate());

            ThingFilterUtil.QueueNextInvocationSearch(SearchCategories.TermFor("Settings"));
            ThingFilterUI.DoThingFilterConfigWindow(
                new Rect(rect.width - 300f, rect.y, 300f, previewHeight),
                ref scrollPosition,
                filter);
        }
    }
}
