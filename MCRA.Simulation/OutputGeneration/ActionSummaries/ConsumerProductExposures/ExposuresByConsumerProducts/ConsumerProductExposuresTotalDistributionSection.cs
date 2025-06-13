using System.Collections.Generic;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductExposuresTotalDistributionSection : ConsumerProductExposuresSectionBase {
        public void Summarize(
            ICollection<ConsumerProduct> allConsumerProducts,
            ICollection<ConsumerProductIndividualExposure> consumerProductIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
         ) {
            Percentages = [lowerPercentage, 50, upperPercentage];
            relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);
            if (exposureType == ExposureType.Chronic) {
                SummarizeChronic(
                    allConsumerProducts,
                    consumerProductIndividualExposures,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    routes,
                    isPerPerson
                );
            }
        }
    }
}
