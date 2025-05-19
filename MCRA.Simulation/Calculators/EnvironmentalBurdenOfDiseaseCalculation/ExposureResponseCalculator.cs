using MCRA.General;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class ExposureResponseCalculator {

        public IExposureResponseFunctionModel ExposureResponseFunctionModel { get; set; }
        public List<ExposureInterval> ExposureIntervals { get; set; }

        public ExposureResponseCalculator(IExposureResponseFunctionModel exposureResponseFunctionModel) {
            ExposureResponseFunctionModel = exposureResponseFunctionModel;
        }

        public List<ExposureResponseResultRecord> Compute(
            List<ITargetIndividualExposure> exposures,
            TargetUnit targetUnit,
            List<PercentileInterval> defaultPercentileIntervals,
            ExposureGroupingMethod exposureGroupingMethod
        ) {
            var erf = ExposureResponseFunctionModel.ExposureResponseFunction;

            var substance = erf.Substance;

            var unitAlignmentFactor = targetUnit.GetAlignmentFactor(
                erf.TargetUnit, substance.MolecularMass, double.NaN
            );

            var useErfBins = erf.HasErfSubGroups
                && exposureGroupingMethod == ExposureGroupingMethod.ErfDefinedBins;

            var bins = getBins(
                exposures,
                defaultPercentileIntervals,
                unitAlignmentFactor,
                useErfBins
            );

            var result = bins
                .Select((r, ix) => compute(
                    r.ExposureInterval,
                    r.PercentileInterval,
                    targetUnit,
                    unitAlignmentFactor,
                    useErfBins,
                    ix
                ))
                .ToList();

            return result;
        }

        private List<(ExposureInterval ExposureInterval, PercentileInterval PercentileInterval)> getBins(
            List<ITargetIndividualExposure> exposures,
            List<PercentileInterval> defaultPercentileIntervals,
            double unitAlignmentFactor,
            bool useErfBins
        ) {
            return useErfBins
                ? computeBinsFromErf(exposures, unitAlignmentFactor)
                : computeExposureBinsFromPercentiles(exposures, defaultPercentileIntervals);
        }

        private List<(ExposureInterval, PercentileInterval)> computeBinsFromErf(
            List<ITargetIndividualExposure> exposures,
            double unitAlignmentFactor
        ) {
            var erf = ExposureResponseFunctionModel.ExposureResponseFunction;

            var substance = erf.Substance;

            var weights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.GetSubstanceExposure(substance))
                .ToList();
            var upperBounds = new List<double> {
                erf.CounterfactualValue * unitAlignmentFactor
            };
            if (erf.HasErfSubGroups) {
                upperBounds.AddRange(
                    erf.ErfSubgroups
                        .OrderBy(r => r.ExposureUpper ?? double.PositiveInfinity)
                        .Select(r => r.ExposureUpper * unitAlignmentFactor ?? double.NaN)
                    );
            }
            var exposureLevels = allExposures
                .PercentagesWithSamplingWeights(weights, upperBounds)
                .ToList();

            var result = new List<(ExposureInterval, PercentileInterval)>();
            var lower = double.NaN;
            var lowerPercentile = double.NaN;
            for (int i = 0; i < upperBounds.Count; i++) {
                var upper = upperBounds[i];
                var upperPercentile = !double.IsNaN(upper) ? exposureLevels[i] : double.NaN;
                var exposureInterval = new ExposureInterval() {
                    Lower = lower,
                    Upper = upper
                };
                var percentileInterval = new PercentileInterval() {
                    Lower = lowerPercentile,
                    Upper = upperPercentile
                };
                result.Add((exposureInterval, percentileInterval));
                lower = upper;
                lowerPercentile = upperPercentile;
            }
            return result;
        }

        private List<(ExposureInterval, PercentileInterval)> computeExposureBinsFromPercentiles(
            List<ITargetIndividualExposure> exposures,
            List<PercentileInterval> percentileIntervals
        ) {
            var erf = ExposureResponseFunctionModel.ExposureResponseFunction;

            var substance = erf.Substance;

            var weights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.GetSubstanceExposure(substance))
                .ToList();

            var upperBounds = percentileIntervals
                .Select(r => r.Upper)
                .ToList();

            var exposureLevels = allExposures
                .PercentilesWithSamplingWeights(weights, upperBounds)
                .ToList();

            var result = new List<(ExposureInterval, PercentileInterval)>();
            for (int i = 0; i < exposureLevels.Count; i++) {
                var percentileInterval = percentileIntervals[i];
                var exposureInterval = new ExposureInterval() {
                    Lower = i == 0 ? 0 : exposureLevels[i - 1],
                    Upper = i == exposureLevels.Count - 1 ? double.NaN : exposureLevels[i]
                };
                result.Add((exposureInterval, percentileInterval));
            }
            return result;
        }

        private ExposureResponseResultRecord compute(
            ExposureInterval exposureInterval,
            PercentileInterval percentileInterval,
            TargetUnit targetUnit,
            double unitAlignmentFactor,
            bool useErfBins,
            int exposureBinId
        ) {
            var erf = ExposureResponseFunctionModel.ExposureResponseFunction;

            var exposureLevel = !double.IsNaN(exposureInterval.Upper)
                ? exposureInterval.Upper : exposureInterval.Lower;

            var result = new ExposureResponseResultRecord {
                ExposureResponseFunction = erf,
                ExposureResponseType = erf.ExposureResponseType,
                IsErfDefinedExposureBin = useErfBins,
                ExposureBinId = exposureBinId,
                ExposureInterval = exposureInterval,
                PercentileInterval = percentileInterval,
                ExposureLevel = exposureLevel,
                ErfDoseUnitAlignmentFactor = unitAlignmentFactor,
                PercentileSpecificRisk = ExposureResponseFunctionModel
                    .Compute(exposureLevel * unitAlignmentFactor),
                TargetUnit = targetUnit,
                Substance = erf.Substance,
                EffectMetric = erf.EffectMetric
            };
            return result;
        }
    }
}
