using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.SingleValueDietaryExposures {
    public enum SingleValueDietaryExposures {
        //No sub-sections
    }
    public sealed class SingleValueDietaryExposuresSummarizer : ActionResultsSummarizerBase<SingleValueDietaryExposuresActionResult> {

        public override ActionType ActionType => ActionType.SingleValueDietaryExposures;

        public override void Summarize(
            ProjectDto project,
            SingleValueDietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<SingleValueDietaryExposures>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new SingleValueDietaryExposuresSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project, data);
            subHeader.SaveSummarySection(section);

            if (result.Exposures?.Any() ?? false) {
                summarizeSingleValueDietaryExposuresByFoodSubstance(
                    result,
                    project.DietaryIntakeCalculationSettings.SingleValueDietaryExposureCalculationMethod,
                    subHeader,
                    order
                );
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord>();
            result.Add(new ActionSummaryUnitRecord("ExposureUnit", data.SingleValueDietaryExposureUnit.GetShortDisplayName(true)));
            result.Add(new ActionSummaryUnitRecord("ConsumptionUnit", data.ConsumptionUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("ConcentrationUnit", data.SingleValueConcentrationUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"));
            result.Add(new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}"));
            result.Add(new ActionSummaryUnitRecord("LowerBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyLowerBound}"));
            result.Add(new ActionSummaryUnitRecord("UpperBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyUpperBound}"));
            result.Add(new ActionSummaryUnitRecord("BodyWeightUnit", data.BodyWeightUnit.GetShortDisplayName()));
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
