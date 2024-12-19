using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the upper tail of the transformed external exposure distribution in bins,
    /// is used for plotting of the transformed exposure distribution
    /// </summary>
    public sealed class ExternalUpperExposureDistributionSection : ExternalExposureDistributionBase {

        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }

        public void Summarize(
            HashSet<int> coExposureIds,
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForUpperTail,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            UpperPercentage = 100 - percentageForUpperTail;
            var externalExposures = externalIndividualDayExposures
                .Select(c => (
                    TotalExternalExposure: c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                    SamplingWeight: c.IndividualSamplingWeight
                ));
            var externalExposureWeights = externalExposures
                .Select(c => c.SamplingWeight)
                .ToList();
            var upperExposureThreshold = externalExposures
                .Select(c => c.TotalExternalExposure)
                .PercentilesWithSamplingWeights(externalExposureWeights, percentageForUpperTail);
            var upperExposures = externalIndividualDayExposures
                .Where(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson) > upperExposureThreshold)
                .ToList();
            CalculatedUpperPercentage = upperExposures.Sum(c => c.IndividualSamplingWeight) / externalIndividualDayExposures.Sum(c => c.IndividualSamplingWeight) * 100;
            Summarize(
                coExposureIds,
                upperExposures,
                relativePotencyFactors,
                membershipProbabilities,
                null,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit,
                isPerPerson);
        }
    }
}
