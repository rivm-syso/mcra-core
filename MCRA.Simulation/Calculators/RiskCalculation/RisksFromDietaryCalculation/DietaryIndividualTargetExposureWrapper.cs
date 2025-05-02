using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public sealed class DietaryIndividualTargetExposureWrapper : ITargetIndividualExposure {

        private readonly List<DietaryIndividualDayIntake> _dietaryIndividualDayTargetExposures;

        private readonly ExposureUnitTriple _exposureUnit;

        public DietaryIndividualTargetExposureWrapper(
            List<DietaryIndividualDayIntake> dietaryIndividualTargetExposures,
            ExposureUnitTriple exposureUnit
        ) {
            _exposureUnit = exposureUnit;
            _dietaryIndividualDayTargetExposures = dietaryIndividualTargetExposures;
            TargetExposuresBySubstance = _dietaryIndividualDayTargetExposures
                .SelectMany(r => r.GetTotalIntakesPerSubstance())
                .GroupBy(r => r.Compound)
                .Select(r => new SubstanceTargetExposure() {
                    Substance = r.Key,
                    Exposure = r.Sum(i => i.Amount) / _dietaryIndividualDayTargetExposures.Count
                        / (_exposureUnit.IsPerUnit ? 1 : SimulatedIndividual.BodyWeight)
                } as ISubstanceTargetExposure)
                .ToDictionary(r => r.Substance);
        }

        public IDictionary<Compound, ISubstanceTargetExposure> TargetExposuresBySubstance { get; set; }

        /// <summary>
        /// Gets the substances for which exposures are recorded.
        /// </summary>
        public ICollection<Compound> Substances => TargetExposuresBySubstance.Keys;

        public SimulatedIndividual SimulatedIndividual => _dietaryIndividualDayTargetExposures.First().SimulatedIndividual;

        public double IntraSpeciesDraw { get; set; }

        /// <summary>
        /// Returns the (cumulative) substance conconcentration of the
        /// target. I.e., the total (corrected) amoount divided by the
        /// volume of the target.
        /// </summary>
        public double GetSubstanceExposure(
             Compound substance
        ) {
            if (!TargetExposuresBySubstance.ContainsKey(substance)) {
                return 0D;
            }
            return TargetExposuresBySubstance[substance].Exposure;
        }

        /// <summary>
        /// Gets the target exposure value for a substance, corrected for relative
        /// potency and membership probability.
        /// </summary>
        public double GetSubstanceExposure(
            Compound substance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return TargetExposuresBySubstance.ContainsKey(substance)
                ? TargetExposuresBySubstance[substance].EquivalentSubstanceExposure(relativePotencyFactors[substance], membershipProbabilities[substance])
                : 0D;
        }

        public ISubstanceTargetExposure GetSubstanceTargetExposure(Compound compound) {
            return TargetExposuresBySubstance.ContainsKey(compound) ? TargetExposuresBySubstance[compound] : null;
        }

        public bool IsPositiveExposure() {
            throw new NotImplementedException();
        }

        public double GetCumulativeExposure(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            var totalExposure = TargetExposuresBySubstance?
                .Values
                .Sum(ipc => ipc
                    .EquivalentSubstanceExposure(
                        relativePotencyFactors[ipc.Substance],
                        membershipProbabilities[ipc.Substance]
                    )
                ) ?? double.NaN;
            return totalExposure;
        }

        public IDictionary<Food, IIntakePerModelledFood> GetModelledFoodTotalExposures(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities) {
            var intakesPerModelledFood = _dietaryIndividualDayTargetExposures
                .Select(c => c.GetModelledFoodTotalExposures(relativePotencyFactors, membershipProbabilities, _exposureUnit.IsPerUnit))
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
           IDictionary<Compound, double> membershipProbabilities) {
            var intakePerModelledFoodSubstance = _dietaryIndividualDayTargetExposures
                .Select(c => c.GetModelledFoodSubstanceTotalExposures(relativePotencyFactors, membershipProbabilities, _exposureUnit.IsPerUnit))
                .SelectMany(c => c.Values)
                .GroupBy(c => (c.ModelledFood, c.Substance))
                .Select(c => new IntakePerModelledFoodSubstance() {
                    Substance = c.Key.Substance,
                    ModelledFood = c.Key.ModelledFood,
                    Exposure = c.Sum(r => r.Exposure) / _dietaryIndividualDayTargetExposures.Count
                })
                .Cast<IIntakePerModelledFoodSubstance>()
                .ToDictionary(c => (c.ModelledFood, c.Substance));


            return intakePerModelledFoodSubstance;
        }
    }
}
