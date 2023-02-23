namespace MCRA.Utils.Hierarchies {

    /// <summary>
    /// Represents a data-node of a hiearchy/tree structure.
    /// Within this hierarchy structure, all nodes can contain
    /// data of type T and a subtree/sub-hierarchy of child-nodes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class HierarchyNode<T> where T : new() {

        /// <summary>
        /// The (data of) the node itself.
        /// </summary>
        public T Node { get; set; }

        /// <summary>
        /// List of references to the children of this node.
        /// </summary>
        public List<HierarchyNode<T>> Children { get; set; }

        /// <summary>
        /// Returns a flattened list of all sub-elements of the hierarchy node
        /// ordered in depth-first.
        /// </summary>
        /// <returns></returns>
        public List<T> AllNodes() {
            var nodes = new List<T> { Node };
            nodes.AddRange(Children.SelectMany(n => n.AllNodes()));
            return nodes;
        }

        /// <summary>
        /// Returns an enumerable to traverse the elements of the hierarchy/tree
        /// in dept-first order.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HierarchyNode<T>> Traverse() {
            yield return this;
            if (Children != null) {
                foreach (var child in Children) {
                    var nodes = child.Traverse();
                    foreach (var node in nodes) {
                        yield return node;
                    }
                }
            }
        }
    }
}
