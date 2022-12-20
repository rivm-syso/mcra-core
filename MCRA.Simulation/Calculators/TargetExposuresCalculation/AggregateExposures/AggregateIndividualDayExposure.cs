using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public sealed class AggregateIndividualDayExposure : ITargetIndividualDayExposure, IExternalIndividualDayExposure {

        public AggregateIndividualDayExposure() { }

        private double _totalConcentrationAtTarget = double.NaN;

        /// <summary>
        /// The id assigned to the simulated individual.
        /// The sampling weight of the simulated individual.
        /// For ExposureType == acute and numberOfIterations == 0, use samplingweights to determine percentiles (USESAMPLINGWEIGHTS):
        ///   - always correct input,
        ///   - correct output; 
        /// For ExposureType == acute and numberOfIterations > 0, no samplingweights to determine percentiles, weights are already in simulated exposures (DO NOT USESAMPLINGWEIGHTS)
        ///   - always correct input,
        ///   - output is already weighted;
        ///  For ExposureType == chronic (USESAMPLINGWEIGHTS)
        ///   - always correct input,
        ///   - correct output; 
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// Identifier for a simulated individual day
        /// </summary>
        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The exposed (dietary) individual.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// The dietary individual day.
        /// </summary>
        public string Day {
            get {
                return DietaryIndividualDayIntake.Day;
            }
        }

        /// <summary>
        /// The individual sampling weight.
        /// </summary>
        public double IndividualSamplingWeight { get; set; }

        /// <summary>
        /// Relative weight of the compartment to the body weight.
        /// </summary>
        public double RelativeCompartmentWeight { get; set; }

        /// <summary>
        /// Weight of target compartment.
        /// </summary>
        public double CompartmentWeight {
            get {
                return Individual.BodyWeight * RelativeCompartmentWeight;
            }
        }

        public NonDietaryIndividualDayIntake NonDietaryIndividualDayIntake { get; set; }

        public DietaryIndividualDayIntake DietaryIndividualDayIntake { get; set; }

        /// <summary>
        /// External exposure per route
        /// </summary>
        public IDictionary<ExposureRouteType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }

        /// <summary>
        /// Internal exposures
        /// </summary>
        public IDictionary<Compound, ISubstanceTargetExposure> TargetExposuresBySubstance { get; set; }
        public double IntraSpeciesDraw { get; set; }

        /// <summary>
        /// Target exposure (total)
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalAmountAtTarget(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
            return TargetExposuresBySubstance?
                    .Values
                    .Sum(ipc => ipc.EquivalentSubstanceAmount(
                        relativePotencyFactors[ipc.Substance], membershipProbabilities[ipc.Substance])
                    ) ?? double.NaN;
        }

        /// <summary>
        /// Target exposure per bodyweight
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalConcentrationAtTarget(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, bool isPerPerson) {
            if (double.IsNaN(_totalConcentrationAtTarget)) {
                _totalConcentrationAtTarget = TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : RelativeCompartmentWeight * Individual.BodyWeight);
            }
            return _totalConcentrationAtTarget;
        }

        /// <summary>
        /// Returns the total intake of the substance of the dietary individual day intake.
        /// </summary>
        /// <returns></returns>
        public double GetSubstanceTotalExposure(Compound substance) {
            return (TargetExposuresBySubstance?.ContainsKey(substance) ?? false)
                ? TargetExposuresBySubstance[substance].SubstanceAmount : 0D;
        }

        /// <summary>
        /// Computes the total dietary substance exposures per mass-unit on this individual-day.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double GetSubstanceTotalExposurePerMassUnit(
            Compound substance,
            bool isPerPerson
        ) {
            var result = GetSubstanceTotalExposure(substance) / (isPerPerson ? 1D : RelativeCompartmentWeight * Individual.BodyWeight);
            return result;
        }

        /// <summary>
        /// Returns whether there is any positive substance amount.
        /// </summary>
        /// <returns></returns>
        public bool IsPositiveExposure() {
            return TargetExposuresBySubstance.Any(r => r.Value.SubstanceAmount > 0);
        }

        /// <summary>
        /// Returns whether there is exposure of multiple substances.
        /// </summary>
        /// <returns></returns>
        public bool IsCoExposure() {
            var result = ExposuresPerRouteSubstance
                .SelectMany(r => r.Value.Where(e => e.Exposure > 0))
                .Select(r => r.Compound)
                .Distinct();
            return result.Count() > 1;
        }

        public IDictionary<Food, IIntakePerModelledFood> IntakesPerModelledFood(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, bool isPerPerson) {
            throw new System.NotImplementedException();
        }
    }
}
