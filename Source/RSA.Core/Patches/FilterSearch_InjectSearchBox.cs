using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RSA.Languages;
using UnityEngine;
using Verse;

namespace RSA.Core
{

    [HarmonyPatch(typeof(ThingFilterUI), nameof(ThingFilterUI.DoThingFilterConfigWindow))]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Harmony patch class")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony patch class")]
    class FilterSearch_InjectSearchBox
    {

        private const float SearchDefaultHeight = 29f;
        private const float SearchClearDefaultSize = 12f;

        private const float buttonSpacing = 2f;
        private const float buttonsInset = 2f;
        private const float buttonSize = 24f;

        private const float OverheadControlsHeight = 35f;

        internal static volatile int showSearchCount;

        internal static Queue<SearchOptions> searchOptions = new Queue<SearchOptions>();


        public static void DoThingFilterConfigWindowHeader(ref Rect rect, ref Vector2 scrollPosition, ThingFilter filter, ThingFilter parentFilter = null, int openMask = 1, IEnumerable<ThingDef> forceHiddenDefs = null, IEnumerable<SpecialThingFilterDef> forceHiddenFilters = null, List<ThingDef> suppressSmallVolumeTags = null) {
            bool showSearch = showSearchCount-- > 0 && searchOptions.Count != 0;
            showSearchCount = Math.Max(0, showSearchCount);

            if (showSearch) {

                /*      Layout
                 *                   buttonSpacing    2x buttonSpacing                                                             buttonsInset
                 *                     |                |                                                                           |        |
                 *                   |<>|            |<--->|                                                                      |<>|       |
                 *                                                                                                                           |
                 *   +---------------------------------------------------------------------------------------------------------------+   -   |
                 *   |                                                                                                               |   o---+
                 *   |  +------------+--+------------+-----+----------------------------------------------------------------------+  |   -
                 *   |  |            |  |            |     |                                                                      |  |   ^
                 *   |  |   CLEAR    |  |   ALLOW    |     |                                                                      |  |   |
                 *   |  |            |  |            |     |              SEARCH BOX                                              |  |   |- buttonSize
                 *   |  |    ALL     |  |    ALL     |     |                                                                      |  |   |
                 *   |  |            |  |            |     |                                                                      |  |   v
                 *   |  +------------+--+------------+-----+------------ headRect ------------------------------------------------+  |   -
                 *   |                                                                                                               |
                 *      
                 *   
                 *      |<------ restWidth --------->|     |<--------------------------- searchWidth ---------------------------->|
                 *      
                 *      
                 *      |<--------->|   |<---------->|
                 *              |             | 
                 *              clearAllowWidth
                 *              (clamped to min
                 *               buttonSize+2*2)
                 */


                var headRect = new Rect(rect.x + buttonsInset, rect.y + buttonsInset, rect.width - (2 * buttonsInset), buttonSize);
                Text.Font = GameFont.Tiny;
                var restWidth = headRect.width*(1-Settings.SearchWidth) - 3 * buttonSpacing;
                var clearAllowWidth = Math.Max(buttonSize, restWidth /2f);


                Text.Font = GameFont.Tiny;
                var clear = "ClearAll".Translate();
                var allow = "AllowAll".Translate();

                // check min width "clear"/"allow" texts would need
                var minWidth = new[] {clear, allow }.Max(c => Text.CurFontStyle.CalcSize(new GUIContent(c)).x);

                Func<Rect, string, object, bool> drawButton;

                if (minWidth +4f > clearAllowWidth) {
                    // use icons
                    drawButton = (r, t, o) => ExtraWidgets.ButtonImage(r, (Texture2D)o, true, new TipSignal {  text = t }, new Rect(r.x+(r.width - buttonSize)/2f, r.y, buttonSize, buttonSize));
                } else {
                    // use text buttons
                    drawButton = (r, t, _) => Widgets.ButtonText(r, t, true, false, true);
                }

                Rect rect2 = new Rect(headRect.x, headRect.y, clearAllowWidth, buttonSize);
                if (drawButton(rect2, clear, Widgets.CheckboxOffTex)) {
                    filter.SetDisallowAll(forceHiddenDefs, forceHiddenFilters);
                }
                Rect rect3 = new Rect(headRect.x + clearAllowWidth + buttonSpacing, headRect.y, clearAllowWidth, buttonSize);
                if (drawButton(rect3, allow, Widgets.CheckboxOnTex)) {
                    filter.SetAllowAll(parentFilter);
                }

                var searchWidth = headRect.width - (2*clearAllowWidth + 3*buttonSpacing);

                Rect searchRect = new Rect(headRect.xMax - searchWidth, headRect.y, searchWidth, buttonSize);

                var options = searchOptions.Dequeue();
                DoSearchBlock(searchRect, options.Term, options.Watermark);
                ThingFilter_InjectFilter.Projections.Enqueue(options.Term.FilterNodes);

                rect.yMin = searchRect.yMax;
            } else {
                Text.Font = GameFont.Tiny;
                float num = rect.width - 2f;
                Rect rect2 = new Rect(rect.x + 1f, rect.y + 1f, num / 2f, 24f);
                if (Widgets.ButtonText(rect2, "ClearAll".Translate(), true, false, true)) {
                    filter.SetDisallowAll(forceHiddenDefs, forceHiddenFilters);
                }
                Rect rect3 = new Rect(rect2.xMax + 1f, rect2.y, rect.xMax - 1f - (rect2.xMax + 1f), 24f);
                if (Widgets.ButtonText(rect3, "AllowAll".Translate(), true, false, true)) {
                    filter.SetAllowAll(parentFilter);
                }
                Text.Font = GameFont.Small;
                rect.yMin = rect2.yMax+1;
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr) {
            #region Transpiler Explanation

            #region C#

            /*
                Change from
              

                    ...
                    Widgets.DrawMenuSection(rect);
                    ------------------               REMOVE BELOW HERE
                    Text.Font = GameFont.Tiny;
                    float num = rect.width - 2f;
                    Rect rect2 = new Rect(rect.x + 1f, rect.y + 1f, num / 2f, 24f);
                    if (Widgets.ButtonText(rect2, "ClearAll".Translate(), true, false, true))
                    {
                        filter.SetDisallowAll(forceHiddenDefs, forceHiddenFilters);
                    }
                    Rect rect3 = new Rect(rect2.xMax + 1f, rect2.y, rect.xMax - 1f - (rect2.xMax + 1f), 24f);
                    if (Widgets.ButtonText(rect3, "AllowAll".Translate(), true, false, true))
                    {
                        filter.SetAllowAll(parentFilter);
                    }
                    Text.Font = GameFont.Small;
                    rect.yMin = rect2.yMax;
                    ------------------              REMOVE ABOVE HERE
                    TreeNode_ThingCategory node = ThingCategoryNodeDatabase.RootNode;
                    ...
                to
                    Widgets.DrawMenuSection(rect);
                    ------------------               
                    FilterSearch_InjectSearchBox.DoThingFilterConfigWindowHeader(ref rect, 
                                                                                            ref scrollPosition, 
                                                                                            filter, 
                                                                                            parentFilter, 
                                                                                            openMask, 
                                                                                            forceHiddenDefs, 
                                                                                            forceHiddenFilters, 
                                                                                            suppressSmallVolumeTags);
                    ------------------
                    TreeNode_ThingCategory node = ThingCategoryNodeDatabase.RootNode;
                    ...
                
                (Can't do this inline, need external method, since we add quite a lot of code)

            */

            #endregion

            #endregion

            var instructions = new List<CodeInstruction>(instr);

            DumpIL(instructions, "Before patch");
            var miDrawMenuSection = AccessTools.Method(typeof(Widgets), nameof(Widgets.DrawMenuSection));
            var miGetRootNode = AccessTools.Property(typeof(ThingCategoryNodeDatabase), nameof(ThingCategoryNodeDatabase.RootNode)).GetGetMethod(true);

            var idxStart = instructions.FindIndex(ci => ci.opcode == OpCodes.Call && ci.operand is MethodInfo && (MethodInfo)ci.operand == miDrawMenuSection);
            if (idxStart == -1)
            {
                Log.Warning("Could not find ThingFilterUI.DoThingFilterConfigWindow DrawMenuSection anchor - not transpiling code");
                return instructions;
            }

            var idxEnd = instructions.FindIndex(idxStart, ci => ci.opcode == OpCodes.Call && ci.operand is MethodInfo && (MethodInfo)ci.operand == miGetRootNode);
            if (idxEnd == -1)
            {
                Log.Warning("Could not find ThingFilterUI.DoThingFilterConfigWindow RootNode anchor - not transpiling code");
                return instructions;
            }

            // remove anything between idxStart & idxEnd (excluding both ends)
            for (int i = idxEnd - 1; i > idxStart; i--)
                instructions.RemoveAt(i);

            instructions.InsertRange(
                idxStart + 1,
                new[]
                {
                    new CodeInstruction(OpCodes.Ldarga_S, 0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Ldarg_S, 4),
                    new CodeInstruction(OpCodes.Ldarg_S, 5),
                    new CodeInstruction(OpCodes.Ldarg_S, 6),
                    new CodeInstruction(OpCodes.Ldarg_S, 8),
                    new CodeInstruction(OpCodes.Call,
                                        typeof(FilterSearch_InjectSearchBox)
                                            .GetMethod(nameof(DoThingFilterConfigWindowHeader), BindingFlags.Public | BindingFlags.Static))
                });

            DumpIL(instructions, "After patch");

            return instructions;
        }

        [Conditional("TRACE")]
        private static void DumpIL(IEnumerable<CodeInstruction> instr, string header = null)
        {
            if(!Prefs.DevMode)
            {
                return;
            }
            string lblToString(Label l) => $"Label_{l.GetHashCode()}";

            string concatLabels(IEnumerable<Label> labels)
            {
                var str = labels.Aggregate(
                    new StringBuilder(),
                    (sb, l) => (sb.Length != 0 ? sb.Append(", ") : sb).Append(lblToString(l)),
                    sb => sb.ToString()

                );

                return !string.IsNullOrEmpty(str) ? $"[{str}]:\t" : null;
            }

            Log.Message(instr.Aggregate(
                    new StringBuilder(header != null ? $"{header}\r\n" : null),
                    (sb, ci) => sb.AppendLine($"{concatLabels(ci.labels)}{ci.opcode}\t{(ci.operand is Label label ? lblToString(label) : ci.operand)}"),
                    sb => sb.ToString()
                )
            );
        }

        public readonly static GUIStyle DefaultSearchBoxStyle;

        static FilterSearch_InjectSearchBox()
        {
            Text.Font = GameFont.Small;
            DefaultSearchBoxStyle = new GUIStyle(Text.CurTextFieldStyle)
            {
                //border = new RectOffset()
            };
        }

        private static void DoSearchBlock(Rect area, SearchTerm term, string weatermark = null, GUIStyle style = null)
        {
            float scale = area.height / SearchDefaultHeight;
            float clearSize = SearchClearDefaultSize * Math.Min(1, scale);

            Rect clearSearchRect = new Rect(area.xMax - 4f - clearSize, area.y + (area.height - clearSize) / 2, clearSize, clearSize);
            bool shouldClearSearch = Widgets.ButtonImage(clearSearchRect, Widgets.CheckboxOffTex);

            Rect searchRect = area;
            string watermark = (term.Value != string.Empty || term.Focused) ? term.Value : (weatermark ?? RSACoreKeys.RSACore_SearchWatermark.Translate());

            bool escPressed = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape;
            bool clickedOutside = !Mouse.IsOver(searchRect) && Event.current.type == EventType.MouseDown;

            if (!term.Focused)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.6f);
            }

            GUI.SetNextControlName(term.ControlName);
            string searchInput = GUI.TextField(searchRect, watermark, style ?? DefaultSearchBoxStyle);
            GUI.color = Color.white;

            if (term.Focused)
            {
                term.Value = searchInput;
            }

            if ((GUI.GetNameOfFocusedControl() == term.ControlName || term.Focused) && (escPressed || clickedOutside))
            {
                GUIUtility.keyboardControl = 0;
                term.Focused = false;
            }
            else if (GUI.GetNameOfFocusedControl() == term.ControlName && !term.Focused)
            {
                term.Focused = true;
            }

            if (shouldClearSearch)
            {
                term.Value = string.Empty;
            }
        }
    }
}
