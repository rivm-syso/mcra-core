﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public sealed class DietaryIndividualDayTargetExposureWrapper : ITargetIndividualDayExposure {

        private readonly DietaryIndividualDayIntake _dietaryIndividualDayIntake;

        private readonly ExposureUnitTriple _exposureUnit;

        public DietaryIndividualDayTargetExposureWrapper(
            DietaryIndividualDayIntake dietaryIndividualDayIntake,
            ExposureUnitTriple exposureUnit
        ) {
            _exposureUnit = exposureUnit;
            _dietaryIndividualDayIntake = dietaryIndividualDayIntake;
            TargetExposuresBySubstance = dietaryIndividualDayIntake
                .GetTotalIntakesPerSubstance()
                .ToDictionary(r => r.Compound, r => new SubstanceTargetExposure() {
                    Substance = r.Compound,
                    Exposure = r.Amount / (_exposureUnit.IsPerUnit ? 1 : SimulatedIndividual.BodyWeight)
                } as ISubstanceTargetExposure);
        }

        public IDictionary<Compound, ISubstanceTargetExposure> TargetExposuresBySubstance { get; set; }

        /// <summary>
        /// Gets the substances for which exposures are recorded.
        /// </summary>
        public ICollection<Compound> Substances => TargetExposuresBySubstance.Keys;

        public string Day => _dietaryIndividualDayIntake.Day;

        public SimulatedIndividual SimulatedIndividual => _dietaryIndividualDayIntake.SimulatedIndividual;

        public int SimulatedIndividualDayId => _dietaryIndividualDayIntake.SimulatedIndividualDayId;

        public DietaryIndividualDayIntake DietaryIndividualDayIntake => _dietaryIndividualDayIntake;

        public double IntraSpeciesDraw { get; set; }

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
                ? TargetExposuresBySubstance[substance]
                    .EquivalentSubstanceExposure(relativePotencyFactors[substance], membershipProbabilities[substance])
                : 0D;
        }

        public ISubstanceTargetExposure GetSubstanceTargetExposure(Compound compound) {
            return TargetExposuresBySubstance.ContainsKey(compound)
                ? TargetExposuresBySubstance[compound]
                : null;
        }

        public bool IsPositiveExposure() {
            throw new NotImplementedException();
        }

        public double GetCumulativeExposure(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return TargetExposuresBySubstance?.Values
                .Sum(ipc => ipc.EquivalentSubstanceExposure(
                    relativePotencyFactors[ipc.Substance], membershipProbabilities[ipc.Substance])
                ) ?? double.NaN;
        }

        public IDictionary<Food, IIntakePerModelledFood> GetModelledFoodTotalExposures(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities) {
            return _dietaryIndividualDayIntake
                .GetModelledFoodTotalExposures(relativePotencyFactors, membershipProbabilities, _exposureUnit.IsPerUnit);
        }

        public IDictionary<(Food, Compound), IIntakePerModelledFoodSubstance> GetModelledFoodSubstanceTotalExposures(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities) {
            return _dietaryIndividualDayIntake
                .GetModelledFoodSubstanceTotalExposures(relativePotencyFactors, membershipProbabilities, _exposureUnit.IsPerUnit);
        }
    }
}
