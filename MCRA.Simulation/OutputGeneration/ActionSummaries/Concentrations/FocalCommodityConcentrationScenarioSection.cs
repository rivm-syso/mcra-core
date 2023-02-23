using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the concentrations of modelled foods from input data
    /// </summary>
    public sealed class FocalCommodityConcentrationScenarioSection : SamplesByFoodSubstanceSection {

        public ConcentrationLimitsFoodBySubstanceRecord Record { get; set; }

        [Description("Replacement method to be used for replacing base concentration data with concentration data of the focal commodity/commodities concentrations.")]
        [DisplayName("Focal commodity concentrations replacement method")]
        public FocalCommodityReplacementMethod FocalCommodityReplacementMethod { get; set; }

        [Description("Focal commodity substance occurrence percentage")]
        [DisplayName("Focal commodity substance occurrence percentage")]
        public double FocalCommoditySubstanceOccurrencePercentage { get; set; }

        [Description("Optional adjustment factor for the focal food/substance concentration. E.g., the expected ratio of mean monitoring concentration and mean field trial concentration.")]
        [DisplayName("Adjustment factor for the focal food/substance concentration")]
        public double FocalCommodityConcentrationAdjustmentFactor { get; set; }

        public void SummarizeConcentrationLimits(
            ProjectDto project,
            ICollection<(Food Food, Compound Substance)> focalCommodityCombinations,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits
        ) {
            var food = focalCommodityCombinations.Select(c => c.Food).Single();
            var substance = focalCommodityCombinations.Select(c => c.Substance).Single();
            var concentrationLimit = maximumConcentrationLimits[(focalCommodityCombinations.First().Food, focalCommodityCombinations.First().Substance)];
            FocalCommodityReplacementMethod = project.ConcentrationModelSettings.FocalCommodityReplacementMethod;
            FocalCommoditySubstanceOccurrencePercentage = project.ConcentrationModelSettings.FocalCommodityScenarioOccurrencePercentage;
            FocalCommodityConcentrationAdjustmentFactor = project.ConcentrationModelSettings.FocalCommodityConcentrationAdjustmentFactor;
            Record = summarizeFocalCombination(food, substance, concentrationLimit.Limit, concentrationLimit.ConcentrationUnit.GetDisplayName());
        }

        public void SummarizeReplaceSubstances(
            ProjectDto project,
            ICollection<(Food Food, Compound Substance)> focalCommodityCombinations,
            ICollection<SampleCompoundCollection> focalCommoditySubstanceSampleCollections,
            ConcentrationUnit unit
        ) {
            Summarize(
                focalCommoditySubstanceSampleCollections,
                focalCommodityCombinations,
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage
            );

            if (ConcentrationInputDataRecords.Count == 1 && ConcentrationInputDataRecords.Single().TotalCount == 1) {
                var concentrationValue = (double)ConcentrationInputDataRecords.Single().MeanPositiveResidues;
                var food = focalCommodityCombinations.Select(c => c.Food).Single();
                var substance = focalCommodityCombinations.Select(c => c.Substance).Single();
                Record = summarizeFocalCombination(food, substance, concentrationValue, unit.GetDisplayName());
            }
            FocalCommodityReplacementMethod = project.ConcentrationModelSettings.FocalCommodityReplacementMethod;
            FocalCommoditySubstanceOccurrencePercentage = project.ConcentrationModelSettings.FocalCommodityScenarioOccurrencePercentage;
            FocalCommodityConcentrationAdjustmentFactor = project.ConcentrationModelSettings.FocalCommodityConcentrationAdjustmentFactor;
        }

        private ConcentrationLimitsFoodBySubstanceRecord summarizeFocalCombination(
            Food Food,
            Compound Substance,
            double value,
            string concentrationUnitString
        ) {
            return new ConcentrationLimitsFoodBySubstanceRecord() {
                FoodName = Food.Name,
                FoodCode = Food.Code,
                SubstanceName = Substance.Name,
                SubstanceCode = Substance.Code,
                ConcentrationLimit = value,
                ConcentrationUnitString = concentrationUnitString,
            };
        }
    }
}
