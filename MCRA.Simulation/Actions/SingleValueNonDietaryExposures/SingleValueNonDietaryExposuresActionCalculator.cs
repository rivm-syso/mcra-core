﻿using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueInternalExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {

    [ActionType(ActionType.SingleValueNonDietaryExposures)]
    public class SingleValueNonDietaryExposuresActionCalculator : ActionCalculatorBase<SingleValueNonDietaryExposuresActionResult> {

        public SingleValueNonDietaryExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SingleValueNonDietaryExposuresSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override SingleValueNonDietaryExposuresActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var result = new SingleValueNonDietaryExposuresActionResult {
                Exposures = new List<ISingleValueNonDietaryExposure> { new SingleValueNonDietaryExposure { Exposure = 0.9637 } },
                ExposureUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.WholeBody)
            };

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(SingleValueNonDietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var summarizer = new SingleValueNonDietaryExposuresSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
        }

        protected override void updateSimulationData(ActionData data, SingleValueNonDietaryExposuresActionResult result) {
            data.SingleValueInternalExposureResults = result.Exposures;
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, SingleValueNonDietaryExposuresActionResult result) {
            var outputWriter = new SingleValueNonDietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(_project, data, result, rawDataWriter);
        }
    }
}