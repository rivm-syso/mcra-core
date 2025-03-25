using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class ExposureResponseCalculator {

        public ExposureResponseFunction ExposureResponseFunction { get; set; }

        public ExposureResponseCalculator(ExposureResponseFunction exposureResponseFunction = null) {
            ExposureResponseFunction = exposureResponseFunction ?? new ExposureResponseFunction();
        }

        public List<ExposureResponseResultRecord> Compute(
            List<ITargetIndividualExposure> exposures,
            TargetUnit exposureUnit,
            List<PercentileInterval> percentileIntervals,
            CompositeProgressState progressState = null
        ) {
            var substance = ExposureResponseFunction.Substance;

            var unitAlignmentFactor = exposureUnit.GetAlignmentFactor(
                ExposureResponseFunction.TargetUnit, substance.MolecularMass, double.NaN
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

        private ExposureResponseResultRecord compute(
            double exposureLevel,
            PercentileInterval percentileInterval,
            double unitAlignmentFactor
        ) {
            var result = new ExposureResponseResultRecord {
                ExposureResponseFunction = ExposureResponseFunction,
                PercentileInterval = percentileInterval,
                ExposureLevel = exposureLevel,
                PercentileSpecificRisk = ExposureResponseFunction.Compute(exposureLevel * unitAlignmentFactor),
                TargetUnit = ExposureResponseFunction.TargetUnit,
                Substance = ExposureResponseFunction.Substance,
                EffectMetric = ExposureResponseFunction.EffectMetric
            };
            return result;
        }
    }
}
