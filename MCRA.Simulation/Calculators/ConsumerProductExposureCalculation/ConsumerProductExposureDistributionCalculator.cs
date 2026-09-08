using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {
    public sealed class ConsumerProductExposureDistributionCalculator {

        public List<ConsumerProductExposureDistribution> Compute(
            List<ConsumerProductExposureAssessment> assessments
        ) {
            var parameters = assessments
                .SelectMany(c => c.ExposureSets, (a, e) => (a, e))
                .Where(c => c.e.ExposureRoutes.Count == 1)
                .Select(c => {
                    var absorptionFactor = c.e.AbsorptionFactor;
                    var percentiles = c.e.ExposurePercentiles
                        .Where(c => c.Value > 0)
                        .Where(c => c.Percentage > 0 && c.Percentage < 100)
                        .Select(c => (c.Percentage, c.Value / absorptionFactor.Value))
                        .ToList();
                    if (percentiles.Count > 0) {
                        var fractionZeros = c.e.PercentageZeros.HasValue
                            ? c.e.PercentageZeros.Value / 100
                            : c.e.ExposurePercentiles.Count / (double)c.e.ExposurePercentiles.Count(r => r.Value <= 0);

                        var distribution = LogNormalDistribution.FitFromPercentiles(percentiles);
                        return new ConsumerProductExposureDistribution {
                            ExposureSetCode = c.e.IdExposureSet,
                            ExposureRoute = c.e.ExposureRoutes.First(),
                            Substance = c.e.Substance,
                            AgeMin = c.a.AgeMin,
                            AgeMax = c.a.AgeMax,
                            Gender = c.a.Gender,
                            Unit = c.e.ConsumerProductExposureEstimateUnit,
                            Distribution = distribution,
                            FractionZeros = fractionZeros,
                            AbsorptionFactor = absorptionFactor.Value
                        };
                    } else {
                        return null;
                    }
                })
                .Where(c => c != null)
                .ToList();
            return parameters;
        }
    }
}
