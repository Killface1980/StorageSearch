using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;


namespace RSA.Core {
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Libarary class")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Libarary class")]
    public static partial class SearchCategories
    {
        private static readonly IDictionary<string, SearchTerm> CachedTerms;

        public static SearchTerm TermFor([NotNull] string category) {
            if (!CachedTerms.TryGetValue(category, out SearchTerm term))
                CachedTerms.Add(category, term = new SearchTerm(category));

            return term;

        }
    }
}
