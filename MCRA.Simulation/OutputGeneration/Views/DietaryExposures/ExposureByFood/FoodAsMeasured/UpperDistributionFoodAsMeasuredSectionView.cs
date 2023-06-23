using MCRA.Utils.Hierarchies;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionFoodAsMeasuredSectionView : SectionView<UpperDistributionFoodAsMeasuredSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => c.NumberOfSubstances <= 1)) {
                hiddenProperties.Add("NumberOfSubstances");
            }

            var isUncertainty = Model.Records.Any() && Model.Records.First().Contributions.Any();
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }

            var records = isUncertainty
                ? Model.Records.Where(c => c.Contribution > 0 || c.MeanContribution > 0).ToList()
                : Model.Records.Where(c => c.Contribution > 0).ToList();

            //Render HTML
            if (records.Any(r => r.Total > 0)) {
                var description = $"Total {records.Count} modelled foods contribution in the upper tail. "
                    + $"Exposure: upper percentage {Model.UpperPercentage:F2} % ({Model.NRecords} records), "
                    + $"minimum value {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, "
                    + $"maximum value {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}.";

                sb.AppendDescriptionParagraph(description);

                if (Model.HasOthers) {
                    sb.AppendParagraph("In this table, each row only summarizes risk driver components selected in the screening", "note");
                }
                if (Model.HasHierarchicalData) {
                    // Returns all food-as-measured lists from the hierarchy as a flat list, with different flags for
                    // summary nodes containing data, summary nodes not containing data, and leaf nodes. This list
                    // is ordered based on the hierarchy; child nodes directly follow their parents.
                    var nodes = Model.HierarchicalNodes.Union(Model.Records).ToList();
                    var hierarchicalRecords = HierarchyUtilities
                        .BuildHierarchy(records, nodes, f => f.__Id, f => f.__IdParent)
                        .ToList();

                    Func<DistributionFoodRecord, string> orderByExtractor = (DistributionFoodRecord f) => {
                        if (f.__IsSummaryRecord) {
                            return "2" + f.FoodName;
                        } else if (f.__Id != null) {
                            return "1" + f.FoodName;
                        } else {
                            return "0" + f.FoodName;
                        }
                    };
                    var sortedHierarchicalRecords = hierarchicalRecords.GetTreeOrderedList(orderByExtractor).ToList();

                    //Get contributions per hierarchical level
                    var allFoodSubTrees = hierarchicalRecords.Traverse();

                    var ids = new List<string>() { null };
                    var selectedIds = allFoodSubTrees
                        .Where(c => string.IsNullOrEmpty(c.Node.__IdParent))
                        .Select(c => c.Node.__Id)
                        .ToList();
                    var resultsCollection = new List<List<DistributionFoodRecord>>();
                    while (selectedIds.Any()) {
                        var treeResults = allFoodSubTrees
                            .Where(c => selectedIds.Contains(c.Node.__Id))
                            .Select(c => c.Node)
                            .ToList();
                        resultsCollection.Add(treeResults);
                        selectedIds = allFoodSubTrees
                            .Where(c => selectedIds.Contains(c.Node.__IdParent))
                            .Select(c => c.Node.__Id)
                            .ToList();
                    }

                    var counter = 0;
                    var panelBuilder = new HtmlTabPanelBuilder();
                    foreach (var results in resultsCollection) {
                        var title = counter == 0
                            ? "Main level"
                            : $"Level {counter}";
                        panelBuilder.AddPanel(
                            id: $"Level {counter}",
                            title: title,
                            hoverText: $"Hierarchy level {counter}",
                            content: ChartHelpers.Chart(
                                $"UpperDistributionFoodAsMeasuredChart {counter}",
                                Model,
                                ViewBag,
                                new UpperDistributionFoodAsMeasuredPieChartCreator(Model, results, isUncertainty, counter: counter),
                                ChartFileType.Svg,
                                true,
                                $"Hierarchy level {counter}."
                            ));
                        counter++;
                    }
                    panelBuilder.RenderPanel(sb);

                    sb.AppendTable(
                         Model,
                         sortedHierarchicalRecords,
                         "UpperDistributionFoodAsMeasuredTable",
                         ViewBag,
                         "__Id",
                         "__IdParent",
                         "__IsSummaryRecord",
                         caption: "Exposure statistics by modelled food (upper tail distribution).",
                         saveCsv: true,
                         displayLimit: 20,
                         hiddenProperties: hiddenProperties,
                         isHierarchical: true
                    );
                } else {
                    if (records.Count > 1) {
                        var chartCreator = new UpperDistributionFoodAsMeasuredPieChartCreator(Model, records, isUncertainty);
                        sb.AppendChart(
                            "UpperDistributionFoodAsMeasuredChart",
                            chartCreator,
                            ChartFileType.Svg,
                            Model,
                            ViewBag,
                            chartCreator.Title,
                            true
                        );
                    }
                    sb.AppendTable(
                        Model,
                        records,
                        "UpperDistributionFoodAsMeasuredTable",
                        ViewBag,
                        caption: "Exposure statistics by modelled food (upper tail distribution).",
                        saveCsv: true,
                        displayLimit: 20,
                        hiddenProperties: hiddenProperties
                    );
                }
            } else {
                sb.AppendParagraph("No positive exposures found", "warning");
            }
        }
    }
}
