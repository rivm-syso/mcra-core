using MCRA.Utils.Hierarchies;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TotalDistributionFoodAsMeasuredSectionView : SectionView<TotalDistributionFoodAsMeasuredSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();

            if (Model.Records.All(c => c.NumberOfSubstances <= 1)) {
                hiddenProperties.Add("NumberOfSubstances");
            }

            var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }


            //Render HTML
            if (Model.Records.Any(r => r.Total > 0)) {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} modelled foods.");
                if (Model.HasOthers) {
                    sb.AppendParagraph("In this table, each row only summarizes risk driver components selected in the screening", "note");
                }

                if (Model.HasHierarchicalData) {
                    // Returns all food-as-measured lists from the hierarchy as a flat list, with different flags for
                    // summary nodes containing data, summary nodes not containing data, and leaf nodes. This list
                    // is ordered based on the hierarchy; child nodes directly follow their parents.
                    var nodes = Model.HierarchicalNodes.Union(Model.Records).ToList();

                    var hierarchicalRecords = HierarchyUtilities
                        .BuildHierarchy(Model.Records, nodes, f => f.__Id, f => f.__IdParent)
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

                    // Get contributions per hierarchical level
                    var allFoodSubTrees = hierarchicalRecords.Traverse();
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
                            id: $"level-{counter}",
                            title: title, 
                            hoverText: $"Hierarchy level {counter}",
                            content: ChartHelpers.Chart(
                                $"TotalDistributionFoodAsMeasuredChart {counter}",
                                Model,
                                ViewBag,
                                new TotalDistributionFoodAsMeasuredPieChartCreator(Model, results, isUncertainty, counter: counter),
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
                         "TotalDistributionFoodAsMeasuredTable",
                         ViewBag,
                         "__Id",
                         "__IdParent",
                         "__IsSummaryRecord",
                         caption: "Exposure statistics by modelled food (total distribution).",
                         saveCsv: true,
                         displayLimit: 20,
                         hiddenProperties: hiddenProperties,
                         isHierarchical: true
                    );
                } else {
                    if (Model.Records.Count() > 1) {
                        var chartCreator = new TotalDistributionFoodAsMeasuredPieChartCreator(Model, Model.Records, isUncertainty);
                        sb.AppendChart(
                            "TotalDistributionFoodAsMeasuredChart",
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
                        Model.Records,
                        "TotalDistributionFoodAsMeasuredTable",
                        ViewBag,
                        caption: "Exposure statistics by modelled food (total distribution).",
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
