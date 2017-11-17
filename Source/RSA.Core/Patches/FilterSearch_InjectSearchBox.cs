using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Harmony;
using RSA.Core.Util;
using RSA.Languages;
using UnityEngine;
using Verse;

namespace RSA.Core {

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


        internal static Func<GUIStyle> Text_GetCurFontStyle = Access.GetPropertyGetter<GUIStyle>("CurFontStyle", typeof(Verse.Text));


        public static void DoThingFilterConfigWindowHeader(ref Rect rect, ref Vector2 scrollPosition, ThingFilter filter, ThingFilter parentFilter = null, int openMask = 1, IEnumerable<ThingDef> forceHiddenDefs = null, IEnumerable<SpecialThingFilterDef> forceHiddenFilters = null, List<ThingDef> suppressSmallVolumeTags = null) {
            bool showSearch = showSearchCount-- > 0 && searchOptions.Count != 0;
            showSearchCount = Math.Max(0, showSearchCount);

            string s = @"


sometext";

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
                var minWidth = new[] {clear, allow }.Max(c => Text_GetCurFontStyle().CalcSize(new GUIContent(c)).x);

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
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            #region Transpiler Explanation

            #region C#
            /*
                Change from
              
                    ...
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
                    ...
                to
                    ...
                    FilterSearch_InjectSearchBox.DoThingFilterConfigWindowHeader(ref rect, 
                                                                                            ref scrollPosition, 
                                                                                            filter, 
                                                                                            parentFilter, 
                                                                                            openMask, 
                                                                                            forceHiddenDefs, 
                                                                                            forceHiddenFilters, 
                                                                                            suppressSmallVolumeTags);
                    ...
                
                (Can't do this inline, need external method, since we add quite a lot of code)

            */
            #endregion

            #region IL-Code
            /*
                Change from: 
              
	                IL_0000: ldarg.0
	                IL_0001: ldc.i4.1
	                IL_0002: call void Verse.Widgets::DrawMenuSection(valuetype [UnityEngine]UnityEngine.Rect, bool)

                    <<< SKIP BELOW HERE        

	                IL_0007: ldc.i4.0
	                IL_0008: call void Verse.Text::set_Font(valuetype Verse.GameFont)
	                IL_000d: ldarga.s rect
	                IL_000f: call instance float32 [UnityEngine]UnityEngine.Rect::get_width()
	                IL_0014: ldc.r4 2
	                IL_0019: sub
	                IL_001a: stloc.0
	                IL_001b: ldloca.s 2
	                IL_001d: ldarga.s rect
	                IL_001f: call instance float32 [UnityEngine]UnityEngine.Rect::get_x()
	                IL_0024: ldc.r4 1
	                IL_0029: add
	                IL_002a: ldarga.s rect
	                IL_002c: call instance float32 [UnityEngine]UnityEngine.Rect::get_y()
	                IL_0031: ldc.r4 1
	                IL_0036: add
	                IL_0037: ldloc.0
	                IL_0038: ldc.r4 2
	                IL_003d: div
	                IL_003e: ldc.r4 24
	                IL_0043: call instance void [UnityEngine]UnityEngine.Rect::.ctor(float32, float32, float32, float32)
	                IL_0048: ldloc.2
	                IL_0049: ldstr "ClearAll"
	                IL_004e: call string Verse.Translator::Translate(string)
	                IL_0053: ldc.i4.1
	                IL_0054: ldc.i4.0
	                IL_0055: ldc.i4.1
	                IL_0056: call bool Verse.Widgets::ButtonText(valuetype [UnityEngine]UnityEngine.Rect, string, bool, bool, bool)
	                IL_005b: brfalse IL_006a

	                IL_0060: ldarg.2
	                IL_0061: ldarg.s forceHiddenDefs
	                IL_0063: ldarg.s forceHiddenFilters
	                IL_0065: callvirt instance void Verse.ThingFilter::SetDisallowAll(class [mscorlib]System.Collections.Generic.IEnumerable`1<class Verse.ThingDef>, class [mscorlib]System.Collections.Generic.IEnumerable`1<class Verse.SpecialThingFilterDef>)

	                IL_006a: ldloca.s 3
	                IL_006c: ldloca.s 2
	                IL_006e: call instance float32 [UnityEngine]UnityEngine.Rect::get_xMax()
	                IL_0073: ldc.r4 1
	                IL_0078: add
	                IL_0079: ldloca.s 2
	                IL_007b: call instance float32 [UnityEngine]UnityEngine.Rect::get_y()
	                IL_0080: ldarga.s rect
	                IL_0082: call instance float32 [UnityEngine]UnityEngine.Rect::get_xMax()
	                IL_0087: ldc.r4 1
	                IL_008c: sub
	                IL_008d: ldloca.s 2
	                IL_008f: call instance float32 [UnityEngine]UnityEngine.Rect::get_xMax()
	                IL_0094: ldc.r4 1
	                IL_0099: add
	                IL_009a: sub
	                IL_009b: ldc.r4 24
	                IL_00a0: call instance void [UnityEngine]UnityEngine.Rect::.ctor(float32, float32, float32, float32)
	                IL_00a5: ldloc.3
	                IL_00a6: ldstr "AllowAll"
	                IL_00ab: call string Verse.Translator::Translate(string)
	                IL_00b0: ldc.i4.1
	                IL_00b1: ldc.i4.0
	                IL_00b2: ldc.i4.1
	                IL_00b3: call bool Verse.Widgets::ButtonText(valuetype [UnityEngine]UnityEngine.Rect, string, bool, bool, bool)
	                IL_00b8: brfalse IL_00c4

	                IL_00bd: ldarg.2
	                IL_00be: ldarg.3
	                IL_00bf: callvirt instance void Verse.ThingFilter::SetAllowAll(class Verse.ThingFilter)

	                IL_00c4: ldc.i4.1
	                IL_00c5: call void Verse.Text::set_Font(valuetype Verse.GameFont)
	                IL_00ca: ldarga.s rect
	                IL_00cc: ldloca.s 2
	                IL_00ce: call instance float32 [UnityEngine]UnityEngine.Rect::get_yMax()
	                IL_00d3: call instance void [UnityEngine]UnityEngine.Rect::set_yMin(float32)

                    <<<< SKIP ABOVE HERE

            	    IL_00d8: ldloca.s 4
	                IL_00da: ldc.r4 0.0
	                IL_00df: ldc.r4 0.0
               
                to:

                    IL_0000: ldarga.s     0
                    IL_0002: ldarg.1      // scrollPosition
                    IL_0003: ldarg.2      // 'filter'
                    IL_0004: ldarg.3      // parentFilter
                    IL_0005: ldarg.s      4
                    IL_0007: ldarg.s      5
                    IL_0009: ldarg.s      6
                    IL_000b: ldarg.s      7
                    IL_000d: call         void SearchFilter.FilterSearch_InjectSearchBox::DoThingFilterConfigWindowHeader(valuetype [UnityEngine]UnityEngine.Rect&, valuetype [UnityEngine]UnityEngine.Vector2&, class ['Assembly-CSharp']Verse.ThingFilter, class ['Assembly-CSharp']Verse.ThingFilter, int32, class [mscorlib]System.Collections.Generic.IEnumerable`1<class ['Assembly-CSharp']Verse.ThingDef>, class [mscorlib]System.Collections.Generic.IEnumerable`1<class ['Assembly-CSharp']Verse.SpecialThingFilterDef>, class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.ThingDef>)
                      
                    >>> INSERT UNTIL HERE        

                    IL_0012: ret
                                                                          
            */
            #endregion

            #endregion

            var instructions = new List<CodeInstruction>(instr);

            DumpIL(instructions, "Before patch");

            // Number of *instructions* to remove 68 
            instructions.RemoveRange(3, 68);

            var patch = new[]
                        {
                            new CodeInstruction(OpCodes.Ldarga_S, 0),
                            new CodeInstruction(OpCodes.Ldarg_1),
                            new CodeInstruction(OpCodes.Ldarg_2),
                            new CodeInstruction(OpCodes.Ldarg_3),
                            new CodeInstruction(OpCodes.Ldarg_S, 4),
                            new CodeInstruction(OpCodes.Ldarg_S, 5),
                            new CodeInstruction(OpCodes.Ldarg_S, 6),
                            new CodeInstruction(OpCodes.Ldarg_S, 7),
                            new CodeInstruction(OpCodes.Call,
                                typeof(FilterSearch_InjectSearchBox)
                                    .GetMethod(nameof(DoThingFilterConfigWindowHeader), BindingFlags.Public | BindingFlags.Static))
                        };
            instructions.InsertRange(3, patch);
            // we skip 0xd1 (=0xd8-0x7) *bytes*, and inject 0x12 *bytes* - so pad with appropriate number of (1 byte) no-ops

            instructions.InsertRange(3 + patch.Length, Enumerable.Repeat(new CodeInstruction(OpCodes.Nop), 0xd1 - 0x12));

            DumpIL(instructions, "After patch");

            return instructions;
        }

        [Conditional("TRACE")]
        private static void DumpIL(IEnumerable<CodeInstruction> instr, string header = null)
        {
            Func<Label, string> lblToString = l => $"Label_{l.GetHashCode()}";

            Func<IEnumerable<Label>, string> concatLabels = labels =>
            {
                var str = labels.Aggregate(
                    new StringBuilder(),
                    (sb, l) => (sb.Length != 0 ? sb.Append(", ") : sb).Append(lblToString(l)),
                    sb => sb.ToString()

                );

                return !String.IsNullOrEmpty(str) ? $"[{str}]:\t" : null;
            };

            Log.Message(instr.Aggregate(
                    new StringBuilder(header != null ? $"{header}\r\n" : null),
                    (sb, ci) => sb.AppendLine($"{concatLabels(ci.labels)}{ci.opcode}\t{(ci.operand is Label ? lblToString((Label)ci.operand) : ci.operand)}"),
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
