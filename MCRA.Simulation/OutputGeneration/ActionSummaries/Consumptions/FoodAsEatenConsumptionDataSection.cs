using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Hierarchies;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the consumption of foods as eaten from consumption data.
    /// </summary>
    public sealed class FoodAsEatenConsumptionDataSection : SummarySection {

        public List<FoodAsEatenConsumptionDataRecord> Records { get; set; }

        public List<HierarchyNode<FoodAsEatenConsumptionDataRecord>> FoodAsEatenConsumptionHierarchy { get; set; }

        public bool UseSamplingWeights { get; set; }

        #region SubSections

        //[DisplayName("Brand loyalty")]
        //public FoodAsEatenBrandLoyaltyDataSection BrandLoyaltyDataSection { get; set; }

        #endregion

        #region VisibilityProperties

        /// <summary>
        /// Returns whether this section has hierarchical data or not.
        /// </summary>
        public bool HasHierarchicalData {
            get {
                return FoodAsEatenConsumptionHierarchy != null && FoodAsEatenConsumptionHierarchy.Count > 0;
            }
        }

        /// <summary>
        /// Returns all food-as-eaten lists from the hierarchy as a flat list, with different flags for
        /// summary nodes containing data, summary nodes not containing data, and leaf nodes. This list
        /// is ordered based on the hierarchy; child nodes directly follow their parents.
        /// </summary>
        public List<FoodAsEatenConsumptionDataRecord> HierarchySummaryList {
            get {
                if (FoodAsEatenConsumptionHierarchy != null) {
                    Func<FoodAsEatenConsumptionDataRecord, string> orderByExtractor = (FoodAsEatenConsumptionDataRecord f) => {
                        if (f.__IsSummaryRecord) {
                            return "2" + f.FoodName;
                        } else if (f.__Id != null) {
                            return "1" + f.FoodName;
                        } else {
                            return "0" + f.FoodName;
                        }
                    };
                    return FoodAsEatenConsumptionHierarchy.GetTreeOrderedList<FoodAsEatenConsumptionDataRecord, string>(orderByExtractor).ToList();
                } else {
                    return null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Summarizes the food consumption data.
        /// </summary>
        /// <param name="individualDays"></param>
        /// <param name="foods"></param>
        /// <param name="consumptions"></param>
        public void Summarize(
            ICollection<IndividualDay> individualDays,
            ICollection<Food> foods,
            ICollection<FoodConsumption> consumptions,
            double lowerPercentage,
            double upperPercentage
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();

            var numberOfIndividualDays = individualDays.Count;
            var totalSamplingWeightAllDays = individualDays.Sum(c => c.Individual.SamplingWeight);
            UseSamplingWeights = individualDays.Any(c => c.Individual.SamplingWeight != 1);

            var allConsumptionsPerFoodAsEaten = consumptions
                .ToLookup(r => r.Food);

            // Create data records
            Records = allConsumptionsPerFoodAsEaten
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(g => summarizeConsumptionsPerFoodAsEatenGroup(
                    numberOfIndividualDays, 
                    totalSamplingWeightAllDays, 
                    g.Key, 
                    g.ToList(), 
                    lowerPercentage,
                    upperPercentage,
                    false
                    )
                )
                .OrderBy(g => g.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(g => g.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
            Records.TrimExcess();

            // Create foods hierarchy
            var foodsAsEaten = consumptions.Select(r => r.Food).Distinct().ToList();
            var foodHierarchy = HierarchyUtilities.BuildHierarchy(foodsAsEaten, foods, (Food f) => f.Code, (Food f) => f.Parent?.Code);
            var allFoodSubTrees = foodHierarchy.Traverse().ToList();

            // Create summary records
            var hiearchyNodeRecords = allFoodSubTrees
                .Where(n => n.Children.Any())
                .Select(n => {
                    var f = n.Node;
                    var allNodes = n.AllNodes();
                    var nodeConsumptions = allNodes.SelectMany(r => allConsumptionsPerFoodAsEaten[r]);
                    return summarizeConsumptionsPerFoodAsEatenGroup(
                        numberOfIndividualDays, 
                        totalSamplingWeightAllDays, 
                        f, 
                        nodeConsumptions,
                        lowerPercentage,
                        upperPercentage,
                        true
                    );
                }).ToList();

            if (hiearchyNodeRecords.Count > 0) {
                // Update the data records that are also summary records
                var endNodes = allFoodSubTrees.Where(n => !n.Children.Any()).ToDictionary(n => n.Node.Code);
                var dataRecords = Records.Select(r => (FoodAsEatenConsumptionDataRecord)r.Clone()).ToList();
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
                FoodAsEatenConsumptionHierarchy = HierarchyUtilities.BuildHierarchy(dataRecords, allRecords, (FoodAsEatenConsumptionDataRecord f) => f.__Id, (FoodAsEatenConsumptionDataRecord f) => f.__IdParent).ToList();
                FoodAsEatenConsumptionHierarchy.TrimExcess();
            }
        }

        private static FoodAsEatenConsumptionDataRecord summarizeConsumptionsPerFoodAsEatenGroup(
            int nIndividualDays, 
            double totalSamplingWeightsAllDays, 
            Food food, 
            IEnumerable<FoodConsumption> records, 
            double lowerPercentage,
            double upperPercentage,
            bool isSummaryRecord
        ) {
            var qry = records.GroupBy(co => (co.idDay, co.Individual)).ToList();
            var nConsumptionDays = qry.Count;
            var totalSamplingWeightsConsumptionDays = qry.Sum(c => c.First().Individual.SamplingWeight);
            var totalConsumption = records.Sum(co => co.Amount * co.Individual.SamplingWeight);
            var samplingWeightsZeros = totalSamplingWeightsAllDays - totalSamplingWeightsConsumptionDays;
            var summaryPercentages = new double[] { lowerPercentage, 50, upperPercentage };
            
            var weights = qry.Select(c => c.First().Individual.SamplingWeight).ToList();
            var percentiles = qry.Select(c => c.Sum(r => r.Amount)).PercentilesWithSamplingWeights(weights, summaryPercentages);
            var percentilesAll = qry.Select(c => c.Sum(r => r.Amount)).PercentilesAdditionalZeros(weights, summaryPercentages, samplingWeightsZeros);

            return new FoodAsEatenConsumptionDataRecord {
                __Id = food.Code,
                __IdParent = food.Parent?.Code,
                __IsSummaryRecord = isSummaryRecord,
                MedianConsumption = percentiles[1],
                Percentile25Consumption = percentiles[0],
                Percentile75Consumption = percentiles[2],
                MedianConsumptionAll = percentilesAll[1],
                Percentile25ConsumptionAll = percentilesAll[0],
                Percentile75ConsumptionAll = percentilesAll[2],
                FoodCode = food.Code,
                FoodName = food.Name,
                BaseFoodCode = food.BaseFood?.Code ?? food.Code,
                BaseFoodName = food.BaseFood?.Name ?? food.Name,
                TreatmentCodes = food.FoodFacets != null ? string.Join("$", food.FoodFacets.Select(r => r.FullCode)) : null,
                TreatmentNames = food.FoodFacets != null ? string.Join("$", food.FoodFacets.Select(r => r.Name)) : null,
                MeanConsumption = totalConsumption / totalSamplingWeightsConsumptionDays,
                MeanConsumptionAll = totalConsumption / totalSamplingWeightsAllDays,
                NConsumptionDays = nConsumptionDays,
                NIndividualDays = nIndividualDays,
                TotalSamplingWeightsAllDays = totalSamplingWeightsAllDays,
                TotalSamplingWeightsConsumptionDays = totalSamplingWeightsConsumptionDays,
                BrandLoyalty = food.MarketShare?.BrandLoyalty ?? double.NaN
            };
        }
    }
}
