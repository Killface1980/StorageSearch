using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using Verse;

namespace RSA.Core
{
    /// <summary>
    /// Allow on-demand filtering of <see cref="TreeNode_ThingCategory" /> in <see cref="Listing_TreeThingFilter.DoCategoryChildren"/> (as of A17 
    /// <see cref="ThingFilterUI.DoThingFilterConfigWindow"/> is currently the only invoker of that method).
    /// </summary>
    [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoCategoryChildren))]
    static class ThingFilter_InjectFilter {
		private static readonly Queue<Func<TreeNode_ThingCategory, TreeNode_ThingCategory>> projections = new Queue<Func<TreeNode_ThingCategory, TreeNode_ThingCategory>>();

	    internal static Queue<Func<TreeNode_ThingCategory, TreeNode_ThingCategory>> Projections => projections;

		[HarmonyPrefix]
		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Harmony patch method")]
		public static void Before_DoCategoryChildren(ref TreeNode_ThingCategory node) {
		    if (projections.Count == 0)
		        return;

		    node = projections.Dequeue()(node);
		}
	}
}
