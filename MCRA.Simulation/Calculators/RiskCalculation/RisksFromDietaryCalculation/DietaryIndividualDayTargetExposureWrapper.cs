using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public sealed class DietaryIndividualDayTargetExposureWrapper : ITargetIndividualDayExposure {

        private DietaryIndividualDayIntake _dietaryIndividualDayIntake;

        public DietaryIndividualDayTargetExposureWrapper(DietaryIndividualDayIntake dietaryIndividualDayIntake) {
            _dietaryIndividualDayIntake = dietaryIndividualDayIntake;
            TargetExposuresBySubstance = dietaryIndividualDayIntake
                .GetTotalIntakesPerSubstance()
                .ToDictionary(r => r.Compound, r => new SubstanceTargetExposure() {
                    Substance = r.Compound,
                    SubstanceAmount = r.Exposure
                } as ISubstanceTargetExposure);
        }

        public IDictionary<Compound, ISubstanceTargetExposure> TargetExposuresBySubstance { get; set; }

        /// <summary>
        /// Gets the substances for which exposures are recorded.
        /// </summary>
        public ICollection<Compound> Substances {
            get {
                return TargetExposuresBySubstance.Keys;
            }
        }

        public string Day {
            get {
                return _dietaryIndividualDayIntake.Day;
            }
        }

        public double IndividualSamplingWeight {
            get {
                return _dietaryIndividualDayIntake.IndividualSamplingWeight;
            }
        }

        public int SimulatedIndividualDayId {
            get {
                return _dietaryIndividualDayIntake.SimulatedIndividualDayId;
            }
        }

        public int SimulatedIndividualId {
            get {
                return _dietaryIndividualDayIntake.SimulatedIndividualId;
            }
        }

        public Individual Individual {
            get {
                return _dietaryIndividualDayIntake.Individual;
            }
        }

        public double CompartmentWeight {
            get {
                return Individual.BodyWeight * RelativeCompartmentWeight;
            }
        }

        public double RelativeCompartmentWeight {
            get {
                return 1D;
            }
        }

        public DietaryIndividualDayIntake DietaryIndividualDayIntake {
            get {
                return _dietaryIndividualDayIntake;
            }
        }

        public double IntraSpeciesDraw { get; set; }

        public double GetExposureForSubstance(Compound compound) {
            return TargetExposuresBySubstance.ContainsKey(compound)
                ? TargetExposuresBySubstance[compound].SubstanceAmount
                : double.NaN;
        }

        /// <summary>
        /// Gets the target exposure value for a substance, corrected for relative
        /// potency and membership probability.
        /// </summary>
        public double GetExposureForSubstance(
            Compound substance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return TargetExposuresBySubstance.ContainsKey(substance)
                ? TargetExposuresBySubstance[substance].EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance])
                    / (isPerPerson ? 1 : CompartmentWeight)
                : 0D;
        }

        public ISubstanceTargetExposureBase GetSubstanceTargetExposure(Compound compound) {
            return TargetExposuresBySubstance.ContainsKey(compound)
                ? TargetExposuresBySubstance[compound]
                : null;
        }

        public bool IsPositiveExposure() {
            throw new NotImplementedException();
        }

        public double TotalAmountAtTarget(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return TargetExposuresBySubstance?.Values
                .Sum(ipc => ipc.EquivalentSubstanceAmount(
                    relativePotencyFactors[ipc.Substance], membershipProbabilities[ipc.Substance])
                ) ?? double.NaN;
        }

        public double TotalConcentrationAtTarget(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : RelativeCompartmentWeight * Individual.BodyWeight);
        }

        public IDictionary<Food, IIntakePerModelledFood> GetModelledFoodTotalExposures(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return _dietaryIndividualDayIntake
                .GetModelledFoodTotalExposures(relativePotencyFactors, membershipProbabilities, isPerPerson);
        }
        public IDictionary<(Food, Compound), IIntakePerModelledFoodSubstance> GetModelledFoodSubstanceTotalExposures(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return _dietaryIndividualDayIntake
                .GetModelledFoodSubstanceTotalExposures(relativePotencyFactors, membershipProbabilities, isPerPerson);
        }

        public double GetSubstanceConcentrationAtTarget(
            Compound substance,
            bool isPerPerson
        ) {
            if (!TargetExposuresBySubstance.ContainsKey(substance)) {
                return 0D;
            }
            return TargetExposuresBySubstance[substance].SubstanceAmount / (isPerPerson ? 1 : CompartmentWeight);
        }
    }
}
