using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.SingleValueDietaryExposures {
    public enum SingleValueDietaryExposures {
        //No sub-sections
    }
    public sealed class SingleValueDietaryExposuresSummarizer : ActionModuleResultsSummarizer<SingleValueDietaryExposuresModuleConfig, SingleValueDietaryExposuresActionResult> {

        public SingleValueDietaryExposuresSummarizer(SingleValueDietaryExposuresModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig outputConfig,
            SingleValueDietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<SingleValueDietaryExposures>(outputConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new SingleValueDietaryExposuresSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            if (result.Exposures?.Any() ?? false) {
                summarizeSingleValueDietaryExposuresByFoodSubstance(
                    result,
                    _configuration.SingleValueDietaryExposureCalculationMethod,
                    subHeader,
                    order
                );
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("ExposureUnit", data.SingleValueDietaryExposureUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)),
                new("ConsumptionUnit", data.ConsumptionUnit.GetShortDisplayName()),
                new("ConcentrationUnit", data.SingleValueConcentrationUnit.GetShortDisplayName()),
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"),
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound}"),
                new("BodyWeightUnit", data.BodyWeightUnit.GetShortDisplayName())
            };
            return result;
        }

        private void summarizeSingleValueDietaryExposuresByFoodSubstance(
                SingleValueDietaryExposuresActionResult result,
                SingleValueDietaryExposuresCalculationMethod singleValueDietaryExposureCalculationMethod,
                SectionHeader header,
                int order
            ) {
            if (result.Exposures.All(r => r is AcuteSingleValueDietaryExposureResult)) {
                var records = result.Exposures.Cast<AcuteSingleValueDietaryExposureResult>().ToList();
                var section = new AcuteSingleValueDietaryExposuresSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Single value dietary exposures by food and substance", order);
                section.Summarize(records, singleValueDietaryExposureCalculationMethod);
                subHeader.SaveSummarySection(section);
            } else if (result.Exposures.All(r => r is NediSingleValueDietaryExposureResult)) {
                var records = result.Exposures.Cast<NediSingleValueDietaryExposureResult>().ToList();
                var section = new NediSingleValueDietaryExposuresSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Single value dietary exposures by food and substance", order);
                section.Summarize(records);
                subHeader.SaveSummarySection(section);
            } else if (result.Exposures.All(r => r is ChronicSingleValueDietaryExposureResult)) {
                var records = result.Exposures.Cast<ChronicSingleValueDietaryExposureResult>().ToList();
                var section = new ChronicSingleValueDietaryExposuresSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Single value dietary exposures by food and substance", order);
                section.Summarize(records);
                subHeader.SaveSummarySection(section);
            } else {
                throw new NotImplementedException();
            }


            if (result.Exposures.Count > 0 && !result.Exposures.Any(r => r.ProcessingType != null)) {
                if (result.Exposures.All(r => r is NediSingleValueDietaryExposureResult)) {
                    var records = result.Exposures.Cast<NediSingleValueDietaryExposureResult>().ToList();
                    var section = new ChronicSingleValueEstimatesSection();
                    var subHeader = header.AddSubSectionHeaderFor(section, "Single value dietary exposure estimates by substance", order);
                    section.Summarize(records);
                    subHeader.SaveSummarySection(section);
                } else if (result.Exposures.All(r => r is ChronicSingleValueDietaryExposureResult)) {
                    var records = result.Exposures.Cast<ChronicSingleValueDietaryExposureResult>().ToList();
                    var section = new ChronicSingleValueEstimatesSection();
                    var subHeader = header.AddSubSectionHeaderFor(section, "Single value dietary exposure estimates by substance", order);
                    section.Summarize(records);
                    subHeader.SaveSummarySection(section);
                }
            }
        }
    }
}
