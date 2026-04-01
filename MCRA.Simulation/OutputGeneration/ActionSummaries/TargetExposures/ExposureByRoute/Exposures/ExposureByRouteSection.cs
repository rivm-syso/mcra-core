using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureByRouteSection : InternalExposureDistributionSectionBase<RouteContributorKey, ExposureByRouteRecord, ExposureByRouteBoxPlotRecord> {
        public override bool SaveTemporaryData => true;

        private static readonly double _upperWhisker = 95;

        public override string DescriptorKey => "Route";
        public override string DescriptorName => "route";

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            bool isPerPerson,
            PopulationStratifier outputStratifier,
            bool skipPrivacySensitiveOutputs
        ) {
            ShowOutliers = !skipPrivacySensitiveOutputs;

            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var routes = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();

            var exposureCollection = ExposureByRouteCalculator.CalculateExposures(
                externalIndividualExposures,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalIndividualExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }

            Records = summarizeExposureRecords(exposureCollection, percentages, outputStratifier);

            BoxPlotRecords = summarizeBoxPlotsRecords(exposureCollection, targetUnit);

            if (outputStratifier != null) {
                StratifiedBoxPlotRecords = summarizeStratifiedBoxPlots(exposureCollection, targetUnit, outputStratifier);
            }
        }
    }
}
