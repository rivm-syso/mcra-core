namespace MCRA.Utils.Hierarchies {
    /// <summary>
    /// Methods to construct and traverse hierarcies.
    /// </summary>
    public static class HierarchyUtilities {

        /// <summary>
        /// Builds a hierarchy from the provided nodes.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectedNodes"></param>
        /// <param name="allNodes"></param>
        /// <param name="keyExtractor"></param>
        /// <param name="parentKeyExtractor"></param>
        /// <returns></returns>
        public static IEnumerable<HierarchyNode<T>> BuildHierarchy<TKey, T>(IEnumerable<T> selectedNodes, IEnumerable<T> allNodes, Func<T, TKey> keyExtractor, Func<T, TKey> parentKeyExtractor)
            where T : class, new() {

            var nodeLookup = allNodes.ToDictionary(n => keyExtractor(n));

            var prunedTreeNodes = new Dictionary<TKey, T>();
            foreach (var node in selectedNodes) {
                var currentNode = node;
                TKey key = keyExtractor(currentNode);
                while (currentNode != null && !prunedTreeNodes.ContainsKey(key)) {
                    prunedTreeNodes.Add(key, currentNode);
                    var parentKey = parentKeyExtractor(currentNode);
                    if (parentKey != null) {
                        nodeLookup.TryGetValue(parentKey, out currentNode);
                    } else {
                        currentNode = null;
                    }
                    key = parentKey;
                }
            }

            var childrenLookup = prunedTreeNodes
                .Select(r => r.Value)
                .ToList()
                .ToLookup(r => parentKeyExtractor(r));
            var roots = prunedTreeNodes
                .Select(r => r.Value)
                .Where(r => parentKeyExtractor(r) == null);

            return roots.Select(r => hiearchyBuild<TKey, T>(r, keyExtractor, childrenLookup));
        }

        /// <summary>
        /// Traverses the hierarchy, defined as a collection of root nodes. Returns all
        /// possible subtrees of the hierarchy.
        /// </summary>
        /// <typeparam name="T">The hierarchy node type.</typeparam>
        /// <param name="roots">The root nodes of the hierarchy.</param>
        /// <returns>All possible subtrees of the hierarchy.</returns>
        public static IEnumerable<HierarchyNode<T>> Traverse<T>(this IEnumerable<HierarchyNode<T>> roots)
            where T : new() {
            return roots.SelectMany(r => r.Traverse());
        }

        /// <summary>
        /// Returns all nodes of the tree as a list, order by 1) the hierarchy, 2) the provided sort-key for arranging
        /// the subnodes within a node.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="roots"></param>
        /// <param name="sortKey"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetTreeOrderedList<T, TKey>(this IEnumerable<HierarchyNode<T>> roots, Func<T, TKey> sortKey)
            where T : new() {
            var orderedRoots = roots.OrderBy(r => sortKey(r.Node));
            foreach (var root in orderedRoots) {
                yield return root.Node;
                var subTreeItems = root.Children.GetTreeOrderedList(sortKey);
                foreach (var item in subTreeItems) {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Returns a flattened list of the elements of hierarchy ordered according to the depth-first
        /// order of the hierarchy/tree structure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="nodes"></param>
        /// <param name="keyExtractor"></param>
        /// <param name="parentKeyExtractor"></param>
        /// <returns></returns>
        public static IEnumerable<T> TreeOrder<T, TKey>(this IEnumerable<T> nodes, Func<T, TKey> keyExtractor, Func<T, TKey> parentKeyExtractor) where T : class, new() {
            var roots = nodes.Where(node => parentKeyExtractor(node) == null);
            var childrenLookup = nodes.Where(n => parentKeyExtractor(n) != null).ToLookup(n => parentKeyExtractor(n));
            foreach (var root in roots) {
                foreach (var child in treeOrder(root, childrenLookup, keyExtractor)) {
                    yield return child;
                }
            }
        }

        private static HierarchyNode<T> hiearchyBuild<TKey, T>(T root, Func<T, TKey> keyExtractor, ILookup<TKey, T> childrenLookup)
            where T : new() {
            return new HierarchyNode<T>() {
                Node = root,
                Children = childrenLookup[keyExtractor(root)].Select(c => hiearchyBuild(c, keyExtractor, childrenLookup)).ToList(),
            };
        }

        private static IEnumerable<T> treeOrder<T, TKey>(T root, ILookup<TKey, T> childrenLookup, Func<T, TKey> keyExtractor) {
            yield return root;
            if (!childrenLookup.Contains(keyExtractor(root))) {
                yield break;
            }
            foreach (var child in childrenLookup[keyExtractor(root)]) {
                foreach (var node in treeOrder(child, childrenLookup, keyExtractor)) {
                    yield return node;
                }
            }
        }
    }
}
