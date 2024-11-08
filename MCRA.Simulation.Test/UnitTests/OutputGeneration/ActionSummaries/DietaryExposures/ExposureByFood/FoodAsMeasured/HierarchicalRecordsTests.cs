using MCRA.Utils.Hierarchies;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsMeasured
    /// </summary>
    [TestClass()]
    public class HierarchicalRecordsTests : SectionTestBase {
        /// <summary>
        /// GetHierarchicalRecordsTests and test total and upper distribution view
        /// </summary>
        [TestMethod]
        public void GetHierarchicalRecordsTests() {
            var foodsAsMeasured = FakeFoodsGenerator.MockFoods("C", "D");

            var allFoods = FakeFoodsGenerator.MockFoods("A", "B", "C");
            allFoods[0].Children = [allFoods[1]];
            allFoods[1].Parent = allFoods[0];
            allFoods[1].Children = [allFoods[2]];
            allFoods[2].Parent = allFoods[1];
            foodsAsMeasured[0].Parent = allFoods[1];
            foodsAsMeasured[0].Children = [foodsAsMeasured[1]];
            foodsAsMeasured[1].Parent = allFoods[2];
            foodsAsMeasured[1].Children = [];


            //Create foods hierarchy
            var foodHierarchy = HierarchyUtilities.BuildHierarchy(foodsAsMeasured, allFoods, (Food f) => f.Code, (Food f) => f.Parent?.Code);
            var allFoodSubTrees = foodHierarchy.Traverse().ToList();

            var foodRecordA = new DistributionFoodRecord() {
                FoodCode = "C",
                FoodName = "C",
                Contribution = 0.81,
                __Id = "C",
                __IdParent = "B",
                __IsSummaryRecord = false,
            };
            var foodRecordB = new DistributionFoodRecord() {
                FoodCode = "D",
                FoodName = "D",
                Contribution = 0.19,
                __Id = "D",
                __IdParent = "C",
                __IsSummaryRecord = false,
            };
            var Records = new List<DistributionFoodRecord>() { foodRecordA, foodRecordB };
            var exposuresPerFoodAsMeasuredLookup = Records.ToLookup(r => r.FoodCode);

            // Create summary records
            var HierarchicalNodes = allFoodSubTrees
                 .Where(n => n.Children.Any())
                 .Select(n => {
                     var f = n.Node;
                     var allNodes = n.AllNodes();
                     var exposures = allNodes.SelectMany(r => exposuresPerFoodAsMeasuredLookup[r.Code]);
                     return summarizeHierarchicalExposures(f, exposures, true);
                 }).ToList();

            foreach (var foodAsMeasured in Records) {
                var node = HierarchicalNodes.FirstOrDefault(c => c.FoodCode == foodAsMeasured.FoodCode);
                if (node != null) {
                    //repair nodes
                    node.FoodCode += "-group";
                    node.__Id = node.FoodCode;
                    foodAsMeasured.__IdParent = node.FoodCode;
                    foodAsMeasured.FoodName += "-unspecified";
                    //assign node to children
                    var children = Records.Where(c => c.__IdParent == foodAsMeasured.FoodCode).ToList();
                    foreach (var child in children) {
                        child.__IdParent = node.FoodCode;
                    }
                }
            }

            var nodes = HierarchicalNodes.Union(Records).ToList();
            var test0 = nodes.Select(c => c.FoodCode).ToList();
            var test2 = HierarchicalNodes.Select(c => c.FoodCode).ToList();
            var test3 = Records.Select(c => c.FoodCode).ToList();
            var hierarchicalRecords = HierarchyUtilities
                .BuildHierarchy(Records, nodes, f => f.__Id, f => f.__IdParent)
                .ToList();

            var sectionTotal = new TotalDistributionFoodAsMeasuredSection();
            Records.First().Contributions = [];
            Records.First().Total = 100;
            sectionTotal.Records = Records;
            sectionTotal.HierarchicalNodes = nodes;
            AssertIsValidView(sectionTotal);
            var sectionUpper = new UpperDistributionFoodAsMeasuredSection();
            Records.First().Contributions = [];
            Records.First().Total = 100;
            sectionUpper.Records = Records;
            sectionUpper.HierarchicalNodes = nodes;
            AssertIsValidView(sectionUpper);
        }


        private DistributionFoodRecord summarizeHierarchicalExposures(
                Food foodAsMeasured,
                IEnumerable<DistributionFoodRecord> records,
                bool isSummaryRecord
            ) {
            return new DistributionFoodRecord() {
                __Id = foodAsMeasured.Code,
                __IdParent = foodAsMeasured.Parent?.Code,
                __IsSummaryRecord = isSummaryRecord,
                Contribution = records.Sum(c => c.Contribution),
                FoodCode = foodAsMeasured.Code,
                FoodName = foodAsMeasured.Name,
            };
        }
    }
}
