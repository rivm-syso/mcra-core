using MCRA.Utils.Hierarchies;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    public class MockTreeNode {
        public MockTreeNode() {

        }

        public MockTreeNode(string id, string idParent, int order) {
            Id = id;
            IdParent = idParent;
            Order = order;
        }

        public string Id { get; set; }
        public string IdParent { get; set; }
        public int Order { get; set; }
    }

    [TestClass]
    public class HierarchyUtilitiesTests {

        #region Mock data

        private static List<MockTreeNode> GetAllMockNodes() {
            var records = new List<MockTreeNode> {
                new("Root", null, 1),
                new("Sub1", "Root", 1),
                new("Sub11", "Sub1", 1),
                new("Sub12", "Sub1", 2),
                new("Sub2", "Root", 2),
                new("Sub3", "Root", 3)
            };
            return records;
        }
        private static List<MockTreeNode> GetAllMockNodesExtended() {
            var records = new List<MockTreeNode> {
                new("Root1", null, 1),
                new("Sub1", "Root1", 1),
                new("Sub11", "Sub1", 1),
                new("Sub12", "Sub1", 2),
                new("Sub2", "Root1", 2),
                new("Sub3", "Root1", 3),
                new("Root2", null, 1),
                new("NewSub1", "Root2", 1),
                new("NewSub11", "NewSub1", 1),
                new("NewSub12", "NewSub1", 2),
                new("NewSub2", "Root2", 2),
                new("NewSub3", "Root2", 3)
            };
            return records;
        }

        private static List<MockTreeNode> GetMockCycleDefinition() {
            var records = new List<MockTreeNode> {
                new("Root", "Sub111", 1),
                new("Sub1", "Root", 1),
                new("Sub11", "Sub1", 1),
                new("Sub111", "Sub11", 1)
            };
            return records;
        }

        #endregion

        [TestMethod]
        public void HierarchyUtilities_BuildHierarchyTest1() {
            var mockNodes = GetAllMockNodes();
            var hierarchy = HierarchyUtilities.BuildHierarchy<string, MockTreeNode>(mockNodes, mockNodes, (MockTreeNode n) => n.Id, (MockTreeNode n) => n.IdParent);
            Assert.AreEqual(1, hierarchy.Count());
        }

        [TestMethod]
        public void HierarchyUtilities_BuildHierarchyTestCycle() {
            var mockNodes = GetMockCycleDefinition();
            var hierarchy = HierarchyUtilities.BuildHierarchy<string, MockTreeNode>(mockNodes, mockNodes, (MockTreeNode n) => n.Id, (MockTreeNode n) => n.IdParent);
            Assert.AreEqual(0, hierarchy.Count());
        }

        [TestMethod]
        public void HierarchyUtilities_TestTraverse() {
            var mockNodes = GetAllMockNodes();
            var hierarchy = HierarchyUtilities.BuildHierarchy<string, MockTreeNode>(mockNodes, mockNodes, (MockTreeNode n) => n.Id, (MockTreeNode n) => n.IdParent);
            var allSubTrees = hierarchy.Traverse();
            Assert.AreEqual(6, allSubTrees.Count());
        }

        [TestMethod]
        public void HierarchyUtilities_TestTraverseExtended() {
            var mockNodes = GetAllMockNodesExtended();
            var hierarchy = HierarchyUtilities.BuildHierarchy<string, MockTreeNode>(mockNodes, mockNodes, (MockTreeNode n) => n.Id, (MockTreeNode n) => n.IdParent);
            var allSubTrees = hierarchy.Traverse();
            var test = allSubTrees
                .Where(n => n.Children.Any())
                .Select(n => {
                    return n.AllNodes();
                    }).ToList();
            Assert.AreEqual(12, allSubTrees.Count());
        }
    }
}
