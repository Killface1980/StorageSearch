using System.Collections.Generic;

namespace RSA.Core {

    public static partial class SearchCategories
    {
		public const string CategoryID_Storage = "Storage";
        public static SearchTerm Storage {
			get { return TermFor(CategoryID_Storage); }
		}
		public const string CategoryID_Bill = "Bill";
        public static SearchTerm Bill {
			get { return TermFor(CategoryID_Bill); }
		}
		public const string CategoryID_Outfit = "Outfit";
        public static SearchTerm Outfit {
			get { return TermFor(CategoryID_Outfit); }
		}
		
		static SearchCategories() {
			CachedTerms = new Dictionary<string, SearchTerm>{
				{ CategoryID_Storage, new SearchTerm(CategoryID_Storage)},
				{ CategoryID_Bill, new SearchTerm(CategoryID_Bill)},
				{ CategoryID_Outfit, new SearchTerm(CategoryID_Outfit)},
			};
		}
    }
}
