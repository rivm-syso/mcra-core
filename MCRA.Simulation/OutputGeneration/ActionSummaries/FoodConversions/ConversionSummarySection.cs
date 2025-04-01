using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConversionSummarySection : SummarySection {

        [DisplayName("Number of conversion paths")]
        [Description("The total number of conversion paths found by the conversion algorithm.")]
        public int NumberOfConversionPaths { get; set; }

        [DisplayName("Number of matched foods-as-eaten")]
        [Description("The number of foods-as-eaten for which at least one modelled food was found.")]
        public int NumberOfMatchedFoodsAsEaten { get; set; }

        [DisplayName("Number of unmatched foods-as-eaten")]
        [Description("The number of foods-as-eaten for which no modelled food were found.")]
        public int NumberOfUnmatchedFoodsAsEaten { get; set; }

        [DisplayName("Number of matched modelled foods")]
        [Description("The number of modelled foods that were found by the conversion algorithm.")]
        public int NumberOfFoodsAsMeasured { get; set; }

        [DisplayName("Number of unmatched modelled foods")]
        [Description("The number of modelled foods for which no foods-as-eaten were found.")]
        public int NumberOfUnmatchedModelledFoods { get; set; }

        [Display(AutoGenerateField = false)]
        public List<ConversionStepStatisticsRecord> ConversionStepStatistics { get; set; }

        [Display(AutoGenerateField = false)]
        public List<ConversionPathStatisticsRecord> ConversionPathStatistics { get; set; }

        public  void Summarize(
            ICollection<FoodConversionResult> conversionResults,
            ICollection<FoodConversionResult> failedFoodConversionResults,
            ICollection<Food> modelledFoods
        ) {
            var matchedFoodsAsEaten = conversionResults.Select(r => r.FoodAsEaten).ToHashSet();
            var matchedModelledFoods = conversionResults.Select(r => r.FoodAsMeasured).ToHashSet();
            var unMatchedFoodsAsEaten = failedFoodConversionResults.Select(c => c.FoodAsEaten).ToHashSet();
            NumberOfMatchedFoodsAsEaten = matchedFoodsAsEaten.Count;
            NumberOfFoodsAsMeasured = matchedModelledFoods.Count;
            NumberOfConversionPaths = conversionResults.GroupBy(r => r.AllStepsToMeasuredString).Count();
            NumberOfUnmatchedFoodsAsEaten = unMatchedFoodsAsEaten.Count(c => !matchedFoodsAsEaten.Contains(c));
            NumberOfUnmatchedModelledFoods = modelledFoods.Count(r => !matchedModelledFoods.Contains(r));
            ConversionStepStatistics = conversionResults
                .AsParallel()
                .SelectMany(r => r.ConversionStepResults, (cr, cs) => (
                    ConversionRecord: cr,
                    Step: cs.Step
                ))
                .GroupBy(r => (r.ConversionRecord, r.Step))
                .Select(r => (
                    Step: r.Key.Step,
                    ConversionRecord: r.Key.ConversionRecord,
                    Count: r.Count()
                ))
                .GroupBy(r => r.Step)
                .Select(r => new ConversionStepStatisticsRecord() {
                    Step = r.Key,
                    PathsWithStep = r.Count(),
                    TotalOccurrences = r.Sum(cs => cs.Count),
                })
                .OrderBy(r => r.Step.GetDisplayName(), StringComparer.OrdinalIgnoreCase)
                .ToList();
            ConversionPathStatistics = conversionResults
                .AsParallel()
                .GroupBy(r => string.Join(" > ", r.ConversionStepResults.Select(c => c.Step.GetShortDisplayName())))
                .Select(r => new ConversionPathStatisticsRecord {
                    ConversionPath = r.Key,
                    TotalOccurrences = r.Count(),
                })
                .OrderBy(r => r.TotalOccurrences)
                .ThenBy(r => r.ConversionPath, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
