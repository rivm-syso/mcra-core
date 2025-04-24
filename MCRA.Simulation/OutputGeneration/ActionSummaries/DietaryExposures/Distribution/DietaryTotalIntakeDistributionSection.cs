using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using System.Xml.Serialization;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total transformed exposure distribution in bins, is used for plotting the transformed exposure distribution
    /// </summary>
    [XmlInclude(typeof(DietaryTotalIntakeCoExposureDistributionSection))]
    public class DietaryTotalIntakeDistributionSection : DietaryDistributionSectionBase, IIntakeDistributionSection {
    public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarizes this section based on the main simulation run.
        /// </summary>
        /// <param name="dietaryIndividualDayIntake"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="percentages"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="uncertaintyLowerLimit"></param>
        /// <param name="uncertaintyUpperLimit"></param>
        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntake,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double[] percentages,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            Summarize(
                null,
                dietaryIndividualDayIntake,
                relativePotencyFactors,
                membershipProbabilities,
                percentages,
                isPerPerson,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
            );
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle (acute).
        /// Make sure to call SummarizeReferenceResults first.
        /// Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="dietaryIndividualDayIntake"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUncertainty(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntake,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var weights = dietaryIndividualDayIntake.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var dietaryIntakes = dietaryIndividualDayIntake.Select(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            _percentiles.AddUncertaintyValues(dietaryIntakes.PercentilesWithSamplingWeights(weights, _percentiles.XValues));
        }
    }
}
