using System;
using JetBrains.Annotations;
using Verse;

namespace RSA.Core {
    public static class ThingFilterUtil {
        /// <summary>
        /// Queue up a projection on the displayed <see cref="TreeNode_ThingCategory"/> for the next invocation of 
        /// <see cref="Listing_TreeThingFilter.DoCategoryChildren"/>.
        /// </summary>
        /// <param name="projection">projection to apply to <see cref="Listing_TreeThingFilter.DoCategoryChildren"/>'s <c>node</c> partameter before execution.</param>
        /// <remarks>This projection is 'consumed' on invocation. Any subsequent incovations will need their own queued projections if desired.</remarks>
        public static void QueueNextInvocationFilter([NotNull]Func<TreeNode_ThingCategory, TreeNode_ThingCategory> projection) {
            ThingFilter_InjectFilter.Projections.Enqueue(projection);
        }

        /// <summary>
        /// Queue up an embedding of a search input filter for the next invocation of <see cref="ThingFilterUI.DoThingFilterConfigWindow"/>.
        /// </summary>
        /// <param name="term"><see cref="SearchTerm"/> to use.</param>
        /// <param name="watermark">Use this parameter to change the search boxes default watermark/seach hint</param>
        public static void QueueNextInvocationSearch([NotNull] SearchTerm term, string watermark = null) {
            FilterSearch_InjectSearchBox.searchOptions.Enqueue(
                new SearchOptions
                {
                    Term = term,
                    Watermark = watermark
                });
            FilterSearch_InjectSearchBox.showSearchCount++;
        }
    }
}
