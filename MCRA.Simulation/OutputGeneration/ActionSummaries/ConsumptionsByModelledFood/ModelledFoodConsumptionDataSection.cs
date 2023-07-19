using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Hierarchies;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General.Action.Settings;
using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ModelledFoodConsumptionDataSection : SummarySection
    {
        private double _lowerPercentage;
        private double _upperPercentage;
        public List<ModelledFoodConsumptionDataRecord> Records { get; set; }

        public List<HierarchyNode<ModelledFoodConsumptionDataRecord>> FoodAsMeasuredConsumptionHierarchy { get; set; }

        public bool UseSamplingWeights { get; set; }

        #region VisibilityProperties

        /// <summary>
        /// Returns whether this section has hierarchical data or not.
        /// </summary>
        public bool HasHierarchicalData {
            get {
                return FoodAsMeasuredConsumptionHierarchy != null && FoodAsMeasuredConsumptionHierarchy.Count > 0;
            }
        }

        /// <summary>
        /// Returns all modelled food lists from the hierarchy as a flat list, with different flags for
        /// summary nodes containing data, summary nodes not containing data, and leaf nodes. This list
        /// is ordered based on the hierarchy; child nodes directly follow their parents.
        /// </summary>
        public List<ModelledFoodConsumptionDataRecord> HierarchySummaryList {
            get {
                if (FoodAsMeasuredConsumptionHierarchy != null) {
                    Func<ModelledFoodConsumptionDataRecord, string> orderByExtractor = (ModelledFoodConsumptionDataRecord f) => {
                        if (f.__IsSummaryRecord) {
                            return "2" + f.FoodName;
                        } else if (f.__Id != null) {
                            return "1" + f.FoodName;
                        } else {
                            return "0" + f.FoodName;
                        }
                    };
                    return FoodAsMeasuredConsumptionHierarchy.GetTreeOrderedList(orderByExtractor).ToList();
                } else {
                    return null;
                }
            }
        }

        #endregion

        #region Sections

        [DisplayName("Market shares")]
        public ModelledFoodMarketShareDataSection MarketShareSection { get; set; }

        [DisplayName("Processed")]
        public ProcessedModelledFoodConsumptionSummarySection ProcessingSection { get; set; }

        #endregion

        /// <summary>
        /// Weighted
        /// This is with compound information included  (if needed for different conversion routes).
        /// </summary>
        /// <param name="header"></param>
        /// <param name="project"></param>
        /// <param name="data"></param>
        /// <param name="consumptionsByModelledFood"></param>
        public void Summarize(
            SectionHeader header,
            ProjectDto project,
            ActionData data,
            ICollection<ConsumptionsByModelledFood> consumptionsByModelledFood
        ) {
            Summarize(
                data.ModelledFoodConsumerDays,
                data.AllFoods,
                data.ModelledFoods,
                consumptionsByModelledFood,
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage
            );
            // Summarize processing
            if (project.ConcentrationModelSettings.IsProcessing) {
                ProcessingSection = new ProcessedModelledFoodConsumptionSummarySection();
                header.AddSubSectionHeaderFor(ProcessingSection, "Processed", 1);
                ProcessingSection.Summarize(
                    data.ModelledFoodConsumerDays,
                    consumptionsByModelledFood,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage
               );
            }

            // Summarize market shares
            if (consumptionsByModelledFood.Any(c => c.IsBrand)) {
                this.MarketShareSection = new ModelledFoodMarketShareDataSection();
                header.AddSubSectionHeaderFor(MarketShareSection, "Market shares", 2);
                this.MarketShareSection.Summarize(consumptionsByModelledFood);
            }
        }

        public void Summarize(
            ICollection<IndividualDay> simulatedIndividualDays,
            ICollection<Food> allFoods,
            ICollection<Food> foodsAsMeasured,
            ICollection<ConsumptionsByModelledFood> consumptionsByModelledFood,
            double lowerPercentage,
            double upperPercentage
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            var nIndividualDays = simulatedIndividualDays.Count;
            var totalSamplingWeightsAllDays = simulatedIndividualDays.Sum(c => c.Individual.SamplingWeight);
            UseSamplingWeights = simulatedIndividualDays.Any(c => c.Individual.SamplingWeight != 1);
            var consumptionAmountsPerFoodAsMeasuredLookup = consumptionsByModelledFood.ToLookup(r => r.FoodAsMeasured);

            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage, 95 };
            Records = consumptionAmountsPerFoodAsMeasuredLookup
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(g => summarizeConsumptionsByModelledFoodGroup(nIndividualDays, totalSamplingWeightsAllDays, g.Key, g, false))
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(g => g.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
            Records.TrimExcess();

            var duplicates2 = allFoods
                .GroupBy(r => r.Code.GetChecksum())
                .Where(r => r.Count() > 1)
                .Select(r => (
                    Code: r.Key,
                    Foods: r.ToList()
                ))
                .ToList();

            // Create foods hierarchy
            var foodHierarchy = HierarchyUtilities.BuildHierarchy(foodsAsMeasured, allFoods, (Food f) => f.Code, (Food f) => f.Parent?.Code);
            var allFoodSubTrees = foodHierarchy.Traverse().ToList();

            // Create summary records
            var hiearchyNodeRecords = allFoodSubTrees
                .Where(n => n.Children.Any())
                .Select(n => {
                    var f = n.Node;
                    var allNodes = n.AllNodes();
                    var consumptions = allNodes.SelectMany(r => consumptionAmountsPerFoodAsMeasuredLookup[r]);
                    return summarizeConsumptionsByModelledFoodGroup(nIndividualDays, totalSamplingWeightsAllDays, f, consumptions, true);
                }).ToList();

            if (hiearchyNodeRecords.Count > 0) {
                // Update the data records that are also summary records
                var endNodes = allFoodSubTrees.Where(n => !n.Children.Any()).ToDictionary(n => n.Node.Code);
                var dataRecords = Records.Select(r => (ModelledFoodConsumptionDataRecord)r.Clone()).ToList();
                foreach (var record in dataRecords) {
                    if (!endNodes.ContainsKey(record.__Id)) {
                        record.__IdParent = record.__Id;
                        record.__Id = "-" + record.__Id;
                        record.FoodName = record.FoodName + " - (unspecified)";
                    }
                }

                // Create consumption records tree
                var allRecords = hiearchyNodeRecords.Union(dataRecords);

                // Create the hierarchy
                FoodAsMeasuredConsumptionHierarchy = HierarchyUtilities.BuildHierarchy(dataRecords, allRecords, (ModelledFoodConsumptionDataRecord f) => f.__Id, (ModelledFoodConsumptionDataRecord f) => f.__IdParent).ToList();
                FoodAsMeasuredConsumptionHierarchy.TrimExcess();
            }
        }

        private ModelledFoodConsumptionDataRecord summarizeConsumptionsByModelledFoodGroup(
                int nIndividualDays, 
                double totalSamplingWeightsAllDays,
                Food foodAsMeasured, 
                IEnumerable<ConsumptionsByModelledFood> consumptionsByModelledFood,
                bool isSummaryRecord
            ) {
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage, 95 };
            var consumptionsPerIndividualDay = consumptionsByModelledFood.GroupBy(co => (co.Day, co.Individual))
                .Select(gdi => (
                    Individual: gdi.Key.Individual,
                    Day: gdi.Key.Day,
                    TotalAmount: gdi.Sum(di => di.AmountFoodAsMeasured),
                    MarketShare: foodAsMeasured.MarketShare?.Percentage / 100D ?? 1D,
                    SamplingWeight: gdi.Key.Individual.SamplingWeight
                )).ToList();
            var weights = consumptionsPerIndividualDay.Select(id => id.SamplingWeight).ToList();
            var totalSamplingWeightsConsumptionDays = weights.Sum();
            var consumptionAmountsPerIndividualDay = consumptionsPerIndividualDay.Select(id => id.MarketShare * id.TotalAmount).ToList();
            var percentiles = consumptionAmountsPerIndividualDay.PercentilesWithSamplingWeights(weights, percentages);
            var totalConsumption = consumptionsPerIndividualDay.Sum(id => id.SamplingWeight * id.MarketShare * id.TotalAmount);
            var samplingWeightsZeros = totalSamplingWeightsAllDays - totalSamplingWeightsConsumptionDays;
            var percentilesAll = consumptionAmountsPerIndividualDay.PercentilesAdditionalZeros(weights, percentages, samplingWeightsZeros);
            return new ModelledFoodConsumptionDataRecord() {
                __Id = foodAsMeasured.Code,
                __IdParent = foodAsMeasured.Parent?.Code,
                __IsSummaryRecord = isSummaryRecord,
                FoodCode = foodAsMeasured.Code,
                FoodName = foodAsMeasured.Name,
                MeanConsumption = totalConsumption / totalSamplingWeightsConsumptionDays,
                Percentile25Consumption = percentiles[0],
                MedianConsumption = percentiles[1],
                Percentile75Consumption = percentiles[2],
                Percentile95Consumption = percentiles[3],
                Percentile25ConsumptionAll = percentilesAll[0],
                MeanConsumptionAll = totalConsumption / totalSamplingWeightsAllDays,
                MedianConsumptionAll = percentilesAll[1],
                Percentile75ConsumptionAll = percentilesAll[2],
                Percentile95ConsumptionAll = percentilesAll[3],
                NumberOfConsumptionDays = consumptionsPerIndividualDay.Count,
                TotalNumberOfIndividualDays = nIndividualDays,
                TotalSamplingWeightsAllDays = totalSamplingWeightsAllDays,
                TotalSamplingWeightsConsumptionDays = totalSamplingWeightsConsumptionDays
            };
        }
    }
}
