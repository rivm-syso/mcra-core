using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceSection : InternalExposureDistributionSectionBase<SourceContributorKey, ExposureBySourceRecord, ExposureBySourceBoxPlotRecord> {

        private static readonly double _upperWhisker = 95;

        public override string DescriptorKey => "Source";
        public override string DescriptorName => "source";

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            bool isPerPerson,
            PopulationStratifier outputStratifier,
            bool skipPrivacySensitiveOutputs
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalIndividualExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;

            var exposureCollection = ExposureBySourceCalculator.CalculateExposures(
                externalIndividualExposures,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            Records = summarizeExposureRecords(exposureCollection, percentages, outputStratifier);

            BoxPlotRecords = summarizeBoxPlotsRecords(exposureCollection, targetUnit);

            if (outputStratifier != null) {
                StratifiedBoxPlotRecords = summarizeStratifiedBoxPlots(exposureCollection, targetUnit, outputStratifier);
            }
        }
    }
}
