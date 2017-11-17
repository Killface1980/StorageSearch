using System;
using System.Linq;
using Verse;

namespace RSA.Core {
    public class SearchTerm
    {
        private readonly string _categoryKey;

        internal string Value = string.Empty;

        internal bool Focused = false;
        internal readonly string ControlName;

        public string CategoryKey {
            get { return _categoryKey; }            
        }

        internal SearchTerm(string categoryKey) {
            _categoryKey = categoryKey;
            ControlName = $"SearchFilterInput_{categoryKey}";
        }

        public void Reset() {
            Value = String.Empty;
        }

        internal TreeNode_ThingCategory FilterNodes(TreeNode_ThingCategory node)
        {
            if (!string.IsNullOrEmpty(Value))
            {
                TreeNode_ThingCategory rootNode = new TreeNode_ThingCategory(new ThingCategoryDef());

                foreach (ThingDef currentThing in node.catDef.DescendantThingDefs.Where(
                    td => td.label.IndexOf(Value, StringComparison.CurrentCultureIgnoreCase) != -1))
                {

                    rootNode.catDef.childThingDefs.Add(currentThing);

                    if (Settings.IncludeParentCategory)
                    {
                        if (!rootNode.catDef.childCategories.Contains(currentThing.FirstThingCategory))
                        {
                            rootNode.catDef.childCategories.Add(currentThing.FirstThingCategory);
                        }
                    }
                }

                node = rootNode;
            }
            return node;
        }
    }
}
