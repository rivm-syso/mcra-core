using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class ExposureEffectCalculator {

        public ExposureEffectFunction ExposureEffectFunction { get; set; }

        public ExposureEffectCalculator(ExposureEffectFunction exposureEffectFunction = null) {
            ExposureEffectFunction = exposureEffectFunction ?? new ExposureEffectFunction();
        }

        public List<ExposureEffectResultRecord> Compute(
            List<ITargetIndividualExposure> exposures,
            TargetUnit exposureUnit,
            List<PercentileInterval> percentileIntervals,
            CompositeProgressState progressState = null
        ) {
            var substance = ExposureEffectFunction.Substance;

            var unitAlignmentFactor = exposureUnit.GetAlignmentFactor(
                ExposureEffectFunction.TargetUnit, substance.MolecularMass, double.NaN
            );

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

            var result = percentileIntervals
                .Zip(exposureLevels)
                .Select(r => (percentileInterval: r.First, exposureLevel: r.Second))
                .Select(r => compute(
                    r.exposureLevel,
                    r.percentileInterval,
                    unitAlignmentFactor
                ))
                .ToList();

            return result;
        }

        private ExposureEffectResultRecord compute(
            double exposureLevel,
            PercentileInterval percentileInterval,
            double unitAlignmentFactor
        ) {
            var result = new ExposureEffectResultRecord {
                ExposureEffectFunction = ExposureEffectFunction,
                PercentileInterval = percentileInterval,
                ExposureLevel = exposureLevel,
                PercentileSpecificRisk = ExposureEffectFunction.Compute(exposureLevel * unitAlignmentFactor),
                TargetUnit = ExposureEffectFunction.TargetUnit,
                Substance = ExposureEffectFunction.Substance,
                EffectMetric = ExposureEffectFunction.EffectMetric
            };
            return result;
        }
    }
}
