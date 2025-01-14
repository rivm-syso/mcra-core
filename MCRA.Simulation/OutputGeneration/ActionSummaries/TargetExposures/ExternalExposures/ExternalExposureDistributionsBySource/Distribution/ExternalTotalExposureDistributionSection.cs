using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalTotalExposureDistributionSection : ExternalExposureDistributionBase, IIntakeDistributionSection {

        public void Summarize(
            HashSet<int> coExposures,
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double[] percentages,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            Summarize(
                coExposures,
                externalIndividualDayExposures,
                relativePotencyFactors,
                membershipProbabilities,
                percentages,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit,
                isPerPerson
            );
        }

        public void SummarizeUncertainty(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var weights = externalIndividualDayExposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            Percentiles.AddUncertaintyValues(externalIndividualDayExposures
                .Select(i => i.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .PercentilesWithSamplingWeights(weights, Percentiles.XValues));
        }
    }
}
