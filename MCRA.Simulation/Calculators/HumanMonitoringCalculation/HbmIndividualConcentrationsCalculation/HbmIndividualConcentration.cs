﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public class HbmIndividualConcentration : ITargetIndividualExposure {

        public HbmIndividualConcentration() {
        }

        public HbmIndividualConcentration(HbmIndividualConcentration hbmIndividualConcentration) {
            Individual = hbmIndividualConcentration.Individual;
            SimulatedIndividualId = hbmIndividualConcentration.SimulatedIndividualId;
            IndividualSamplingWeight = hbmIndividualConcentration.IndividualSamplingWeight;
            NumberOfDays = hbmIndividualConcentration.NumberOfDays;
            SimulatedIndividualBodyWeight = hbmIndividualConcentration.SimulatedIndividualBodyWeight;
            ConcentrationsBySubstance = hbmIndividualConcentration.ConcentrationsBySubstance
                .ToDictionary(
                    r => r.Key,
                    r => r.Value.Clone()
            );
            IntraSpeciesDraw = hbmIndividualConcentration.IntraSpeciesDraw;
        }

        /// <summary>
        /// The individual belonging to this record.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// The identifier of the simulated individual.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The sampling weight of the individual.
        /// </summary>
        public double IndividualSamplingWeight { get; set; } = 1D;

        /// <summary>
        /// The body weight of the individual used in calculations, which is most of the times equal to the
        /// individual body weight read from the data or an imputed value when the body weight is missing.
        /// </summary>
        public double SimulatedIndividualBodyWeight { get; set; }

        /// <summary>
        /// The (survey)day of the exposure.
        /// </summary>
        public int NumberOfDays { get; set; }

        /// <summary>
        /// Drawn intra-species variability factor.
        /// TODO: should be removed from this class.
        /// </summary>
        public double IntraSpeciesDraw { get; set; }

        /// <summary>
        /// The target exposures by substance.
        /// </summary>
        public IDictionary<Compound, HbmSubstanceTargetExposure> ConcentrationsBySubstance { get; set; }
            = new Dictionary<Compound, HbmSubstanceTargetExposure>();

        /// <summary>
        /// Gets the substances for which exposures are recorded.
        /// </summary>
        public ICollection<Compound> Substances {
            get {
                return ConcentrationsBySubstance.Keys;
            }
        }

        /// <summary>
        /// Returns the (cumulative) substance conconcentration of the
        /// target. I.e., the total (corrected) amount divided by the
        /// volume of the target.
        /// </summary>
        public double GetSubstanceExposure(
            Compound substance
        ) {
            if (!ConcentrationsBySubstance.ContainsKey(substance)) {
                return 0D;
            }
            return ConcentrationsBySubstance[substance].Exposure;
        }

        /// <summary>
        /// Gets the target exposure value for the specified substance, corrected for relative
        /// potency and membership probability.
        /// </summary>
        public double GetSubstanceExposure(
            Compound substance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return ConcentrationsBySubstance.ContainsKey(substance)
                ? ConcentrationsBySubstance[substance]
                    .EquivalentSubstanceExposure(relativePotencyFactors[substance], membershipProbabilities[substance])
                : double.NaN;
        }

        /// <summary>
        /// Gets the target exposure for the specified substance.
        /// </summary>
        public ISubstanceTargetExposure GetSubstanceTargetExposure(Compound substance) {
            return ConcentrationsBySubstance.ContainsKey(substance)
                ? ConcentrationsBySubstance[substance] : null;
        }

        /// <summary>
        /// Concentration at target (i.e., per kg bodyweight/organ weight) corrected for relative potency and membership probability.
        /// </summary>
        public double GetCumulativeExposure(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return ConcentrationsBySubstance?.Values
                .Sum(i => i
                    .EquivalentSubstanceExposure(
                        relativePotencyFactors[i.Substance],
                        membershipProbabilities[i.Substance]
                    )
                ) ?? double.NaN;
        }

        /// <summary>
        /// Returns whether there is any positive substance amount.
        /// </summary>
        public bool IsPositiveExposure() {
            return ConcentrationsBySubstance.Any(r => r.Value.Exposure > 0);
        }

        /// <summary>
        /// Creates a clone.
        /// </summary>
        public HbmIndividualConcentration Clone() {
            var clone = new HbmIndividualConcentration() {
                Individual = Individual,
                SimulatedIndividualId = SimulatedIndividualId,
                IndividualSamplingWeight = IndividualSamplingWeight,
                NumberOfDays = NumberOfDays,
                SimulatedIndividualBodyWeight = SimulatedIndividualBodyWeight,
                ConcentrationsBySubstance = ConcentrationsBySubstance
                    .ToDictionary(
                        r => r.Key,
                        r => r.Value.Clone()
                ),
                IntraSpeciesDraw = IntraSpeciesDraw
            };
            return clone;
        }
    }
}
