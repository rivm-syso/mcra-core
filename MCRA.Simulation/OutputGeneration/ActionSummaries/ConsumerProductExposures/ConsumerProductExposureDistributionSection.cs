using MathNet.Numerics.Distributions;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductExposureDistributionSection : SummarySection {

        public List<ParameterRecord> FittedPercentileRecords { get; set; }
        public void Summarize(
            ICollection<ConsumerProductExposureDistribution> distributions,
            IDictionary<string, ConsumerProductExposureAssessment> exposureAssessments
         ) {
            FittedPercentileRecords = [.. exposureAssessments.Values
                .SelectMany(c => c.ExposureSets, (cpA, cpS) => (cpA, cpS))
                .Where(c => !c.cpS.ExposurePercentiles.All(p => p.Value == 0))
                .Select(c => {
                    var set = distributions?.FirstOrDefault(d => d.ExposureSetCode == c.cpS.IdExposureSet);
                    var fittedValues = getFittedValues(c, set);
                    return new ParameterRecord() {
                        Distribution = set?.Distribution,
                        FittedValues = fittedValues,
                        AssessmentName = c.cpA.IdAssessment,
                        ExposureSetCode = c.cpS.IdExposureSet,
                        SubstanceName = c.cpS.Substance.Name,
                        Route = string.Join(", ",   c.cpS.ExposureRoutes.Select(r => r.GetShortDisplayName())),
                        Unit = c.cpS.ConsumerProductExposureEstimateUnit.GetShortDisplayName(),
                        Percentiles = [.. c.cpS.ExposurePercentiles.Select(c => (c.Value, c.Percentage))],
                    };
                })];
        }

        private static List<(double, double)> getFittedValues(
            (ConsumerProductExposureAssessment cpA, ConsumerProductExposureSet cpS) c,
            ConsumerProductExposureDistribution set
        ) {
            var steps = 100;
            var exposures = c.cpS.ExposurePercentiles.Select(c => c.Value).ToList();
            var min = exposures.Min();
            var max = exposures.Max();
            var logStart = Math.Log(min);
            var logEnd = Math.Log(max);
            var stepSize = (logEnd - logStart) / (steps - 1);
            var range = new List<double>();
            var fittedValues = new List<(double, double)>();
            for (int i = 0; i < steps; i++) {
                var currentLog = logStart + (i * stepSize);
                range.Add(Math.Exp(currentLog));
            }
            range[steps - 1] = max;
            foreach (var r in range) {
                var percentile = LogNormal.CDF(set?.Distribution?.Mu ?? 0, set?.Distribution?.Sigma ?? 0, r);
                fittedValues.Add((r, percentile));
            }
            return fittedValues;
        }
    }
}
