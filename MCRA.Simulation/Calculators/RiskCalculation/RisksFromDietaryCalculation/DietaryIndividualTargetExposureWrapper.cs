using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public sealed class DietaryIndividualTargetExposureWrapper : ITargetIndividualExposure {

        private List<DietaryIndividualDayIntake> _dietaryIndividualDayTargetExposures;

        public DietaryIndividualTargetExposureWrapper(List<DietaryIndividualDayIntake> dietaryIndividualTargetExposures) {
            _dietaryIndividualDayTargetExposures = dietaryIndividualTargetExposures;
            TargetExposuresBySubstance = _dietaryIndividualDayTargetExposures
                .SelectMany(r => r.GetTotalIntakesPerSubstance())
                .GroupBy(r => r.Compound)
                .Select(r => new SubstanceTargetExposure() {
                    Substance = r.Key,
                    SubstanceAmount = r.Sum(i => i.Exposure) / _dietaryIndividualDayTargetExposures.Count
                } as ISubstanceTargetExposure)
                .ToDictionary(r => r.Substance);
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

        public double IndividualSamplingWeight {
            get {
                return _dietaryIndividualDayTargetExposures.First().IndividualSamplingWeight;
            }
        }

        public int SimulatedIndividualDayId {
            get {
                return _dietaryIndividualDayTargetExposures.First().SimulatedIndividualDayId;
            }
        }
        public Individual Individual {
            get {
                return _dietaryIndividualDayTargetExposures.First().Individual;
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

        public double IntraSpeciesDraw { get ; set; }

        public int SimulatedIndividualId {
            get {
                return _dietaryIndividualDayTargetExposures.First().SimulatedIndividualId;
            }
        }

        public double GetExposureForSubstance(Compound compound) {
            return TargetExposuresBySubstance.ContainsKey(compound) ? TargetExposuresBySubstance[compound].SubstanceAmount : double.NaN;
        }

        public ISubstanceTargetExposureBase GetSubstanceTargetExposure(Compound compound) {
            return TargetExposuresBySubstance.ContainsKey(compound) ? TargetExposuresBySubstance[compound] : null;
        }

        public bool IsPositiveExposure() {
            throw new NotImplementedException();
        }

        public double TotalAmountAtTarget(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return TargetExposuresBySubstance?
                   .Values
                   .Sum(ipc => ipc.EquivalentSubstanceAmount(
                       relativePotencyFactors[ipc.Substance], membershipProbabilities[ipc.Substance])
                   ) ?? double.NaN;
        }

        /// <summary>
        /// Returns the (cumulative) substance conconcentration of the
        /// target. I.e., the total (corrected) amoount divided by the
        /// volume of the target.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double GetSubstanceConcentrationAtTarget(
             Compound substance,
             bool isPerPerson
        ) {
            if (!TargetExposuresBySubstance.ContainsKey(substance)) {
                return 0D;
            }
            return TargetExposuresBySubstance[substance].SubstanceAmount / (isPerPerson ? 1 : CompartmentWeight);
        }

        public double TotalConcentrationAtTarget(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities) 
                / (isPerPerson ? 1 : RelativeCompartmentWeight * Individual.BodyWeight);
        }

        public IDictionary<Food, IIntakePerModelledFood> GetModelledFoodTotalExposures(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var intakesPerModelledFood = _dietaryIndividualDayTargetExposures
                .Select(c => c.GetModelledFoodTotalExposures(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .SelectMany(c => c.Values)
                .GroupBy(c => c.ModelledFood)
                .Select(c => new IntakePerModelledFood() {
                    ModelledFood = c.Key,
                    Exposure = c.Sum(r => r.Exposure) / _dietaryIndividualDayTargetExposures.Count,
                })
                .Cast<IIntakePerModelledFood>()
                .ToDictionary(c => c.ModelledFood);

            return intakesPerModelledFood;
        }

        public IDictionary<(Food, Compound), IIntakePerModelledFoodSubstance> GetModelledFoodSubstanceTotalExposures(
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           bool isPerPerson
       ) {
            var intakePerModelledFoodSubstance = _dietaryIndividualDayTargetExposures
                .Select(c => c.GetModelledFoodSubstanceTotalExposures(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .SelectMany(c => c.Values)
                .GroupBy(c => (c.ModelledFood, c.Substance))
                .Select(c => new IntakePerModelledFoodSubstance() {
                    Substance = c.Key.Substance,
                    ModelledFood = c.Key.ModelledFood,
                    Exposure = c.Sum(r => r.Exposure ) / _dietaryIndividualDayTargetExposures.Count
                })
                .Cast<IIntakePerModelledFoodSubstance>()
                .ToDictionary(c => (c.ModelledFood, c.Substance));


            return intakePerModelledFoodSubstance;
        }
    }
}
