using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation.ForwardCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public class RiskCalculator<T> : RiskCalculatorBase
        where T : ITargetIndividualExposure {

        public RiskCalculator(HealthEffectType healthEffectType)
            : base(healthEffectType) {
            ExposureType = typeof(T) == typeof(ITargetIndividualDayExposure)
                ? ExposureType.Acute : ExposureType.Chronic;
        }

        protected ExposureType ExposureType { get; private set; }

        /// <summary>
        /// Calculates cumulative individual risks for a single substance.
        /// </summary>
        /// <param name="exposures"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="hazardCharacterisation"></param>
        /// <param name="hazardCharacterisationUnit"></param>
        /// <param name="substance"></param>
        /// <returns></returns>
        public List<IndividualEffect> ComputeSingleSubstance(
            ICollection<T> exposures,
            TargetUnit exposureUnit,
            IHazardCharacterisationModel hazardCharacterisation,
            TargetUnit hazardCharacterisationUnit,
            Compound substance
        ) {
            double exposureExtractor(T c) => c.GetSubstanceExposure(substance);
            var result = computeSubstanceRisks(
                exposures,
                exposureExtractor,
                exposureUnit,
                hazardCharacterisation,
                hazardCharacterisationUnit,
                substance
            );
            return result;
        }

        /// <summary>
        /// Calculates the risks by substance for all individuals, RPF weighted.
        /// </summary>
        /// <param name="exposures"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="hazardCharacterisationUnit"></param>
        /// <param name="substances"></param>
        /// <param name="correctedRelativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="referenceDose"></param>
        /// <returns></returns>
        public Dictionary<Compound, List<IndividualEffect>> ComputeBySubstanceRpfWeighted(
            ICollection<T> exposures,
            TargetUnit exposureUnit,
            TargetUnit hazardCharacterisationUnit,
            ICollection<Compound> substances,
            IDictionary<Compound, double> correctedRelativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IHazardCharacterisationModel referenceDose
        ) {
            var results = substances
                .ToDictionary(
                    substance => substance,
                    substance => {
                        double exposureExtractor(T c) =>
                            c.GetSubstanceExposure(
                                substance, 
                                correctedRelativePotencyFactors, 
                                membershipProbabilities
                            );
                        return computeSubstanceRisks(
                            exposures,
                            exposureExtractor,
                            exposureUnit,
                            referenceDose,
                            hazardCharacterisationUnit,
                            substance
                        );
                    }
                );
            return results;
        }

        /// <summary>
        /// Calculates the risks by substance for the specified substances.
        /// </summary>
        /// <param name="exposures"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="hazardCharacterisationUnit"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public Dictionary<Compound, List<IndividualEffect>> ComputeBySubstance(
            ICollection<T> exposures,
            TargetUnit exposureUnit,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            TargetUnit hazardCharacterisationUnit,
            ICollection<Compound> substances
        ) {
            var results = substances
                .ToDictionary(
                    substance => substance,
                    substance => {
                        double exposureExtractor(T c) => c.GetSubstanceExposure(substance);
                        return computeSubstanceRisks(
                            exposures,
                            exposureExtractor,
                            exposureUnit,
                            hazardCharacterisations[substance],
                            hazardCharacterisationUnit,
                            substance
                        );
                    }
                );
            return results;
        }

        /// <summary>
        /// Calculates cumulative individual risks based on RPF weighing.
        /// </summary>
        /// <param name="exposures"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="hazardCharacterisation"></param>
        /// <param name="hazardCharacterisationUnit"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="referenceSubstance"></param>
        /// <returns></returns>
        public List<IndividualEffect> ComputeRpfWeighted(
            ICollection<T> exposures,
            TargetUnit exposureUnit,
            IHazardCharacterisationModel hazardCharacterisation,
            TargetUnit hazardCharacterisationUnit,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceSubstance
        ) {
            double exposureExtractor(T c) =>
                c.GetCumulativeExposure(relativePotencyFactors, membershipProbabilities);
            var result = calculateRisk(
                exposures,
                exposureExtractor,
                exposureUnit,
                hazardCharacterisation,
                hazardCharacterisationUnit,
                referenceSubstance
            );
            return result;
        }

        /// <summary>
        /// Computes the cumulative risks based on sum of ratios approach.
        /// </summary>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public List<IndividualEffect> ComputeSumOfRatios(
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            membershipProbabilities = membershipProbabilities
                ?? individualEffectsBySubstance.ToDictionary(c => c.Key, c => 1d);

            var individualEffects = individualEffectsBySubstance
               .SelectMany(c => c.Value, (v, s) => (Substance: v.Key, IndividualEffect: s))
               .ToLookup(c => c.IndividualEffect.SimulatedIndividualId, c => (c.Substance, c.IndividualEffect));

            var risks = individualEffects
                .Select(c => {
                    var hazardExposureRatio = 1 / c.Sum(r => 1 / getHazardExposureRatio(HealthEffectType, r.IndividualEffect.CriticalEffectDose, r.IndividualEffect.Exposure * membershipProbabilities[r.Substance]));
                    var exposureHazardRatio = c.Sum(r => getExposureHazardRatio(HealthEffectType, r.IndividualEffect.CriticalEffectDose, r.IndividualEffect.Exposure * membershipProbabilities[r.Substance]));
                    return new IndividualEffect() {
                        SimulatedIndividualId = c.First().IndividualEffect.SimulatedIndividualId,
                        Individual = c.First().IndividualEffect.Individual,
                        SamplingWeight = c.First().IndividualEffect.SamplingWeight,
                        HazardExposureRatio = hazardExposureRatio,
                        ExposureHazardRatio = exposureHazardRatio,
                        IsPositive = c.Any(r => r.IndividualEffect.Exposure * membershipProbabilities[r.Substance] > 0),
                    };
                })
                .ToList();

            return risks;
        }

        /// <summary>
        /// Computes the individual effects for a single substance.
        /// </summary>
        /// <param name="exposures"></param>
        /// <param name="exposureExtractor"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="hazardCharacterisation"></param>
        /// <param name="hazardCharacterisationUnit"></param>
        /// <param name="substance"></param>
        /// <returns></returns>
        private List<IndividualEffect> computeSubstanceRisks(
            ICollection<T> exposures,
            Func<T, double> exposureExtractor,
            TargetUnit exposureUnit,
            IHazardCharacterisationModel hazardCharacterisation,
            TargetUnit hazardCharacterisationUnit,
            Compound substance) {
            return calculateRisk(
                exposures,
                exposureExtractor,
                exposureUnit,
                hazardCharacterisation,
                hazardCharacterisationUnit,
                substance
            );
        }

        /// <summary>
        /// Calculate individual effects i.c. critical effect dose, threshold value/exposure,
        /// equivalent animal dose. Use the original critical effect dose; i.e., the critical
        /// effect dose defined on the original animal (or in vitro system) of the dose response
        /// model.
        /// Note that the critical effect dose is corrected.
        /// A correction for (exposure) per person is needed because exposures are at kg per bw level or per person,
        /// Maybe a correction as applied here is incorrect and there should be one value for all individuals.
        /// </summary>
        /// <param name="exposures"></param>
        /// <param name="exposureExtractor"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="hazardCharacterisation"></param>
        /// <param name="hazardCharacterisationUnit"></param>
        /// <param name="substance"></param>
        /// <returns></returns>
        private List<IndividualEffect> calculateRisk(
            ICollection<T> exposures,
            Func<T, double> exposureExtractor,
            TargetUnit exposureUnit,
            IHazardCharacterisationModel hazardCharacterisation,
            TargetUnit hazardCharacterisationUnit,
            Compound substance
        ) {
            var individualEffects = exposures
                .AsParallel()
                .Select(c => {
                    var alignmentFactor = exposureUnit
                        .GetAlignmentFactor(
                            hazardCharacterisationUnit,
                            substance.MolecularMass,
                            double.NaN
                        );
                    var exposure = exposureExtractor(c) * alignmentFactor;
                    var age = c.Individual.GetAge();
                    var ced = (hazardCharacterisation.HCSubgroups?.Count > 0)
                        ? hazardCharacterisation.DrawIndividualHazardCharacterisationSubgroupDependent(c.IntraSpeciesDraw, age)
                        : hazardCharacterisation.DrawIndividualHazardCharacterisation(c.IntraSpeciesDraw);

                    var item = new IndividualEffect {
                        SimulatedIndividualId = getSimulatedId(c),
                        Individual = c.Individual,
                        SamplingWeight = c.IndividualSamplingWeight,
                        Exposure = exposure,
                        IntraSpeciesDraw = c.IntraSpeciesDraw,
                        IsPositive = exposure > 0,
                        CriticalEffectDose = ced,
                        HazardExposureRatio = getHazardExposureRatio(HealthEffectType, ced, exposure),
                        ExposureHazardRatio = getExposureHazardRatio(HealthEffectType, ced, exposure),
                        // Note: equivalent test-system dose is here assumed to be the exposure corrected by the
                        // combined assessment factor of the hazard characterisation (i.e., the factor translating
                        // the test-system point-of-departure to the hazard characterisation value). It is used
                        // for forward effect calculations.
                        EquivalentTestSystemDose = exposure / hazardCharacterisation.CombinedAssessmentFactor
                    };
                    return item;
                })
                .ToList();

            // Forward calculation
            var model = hazardCharacterisation?.TestSystemHazardCharacterisation?.DoseResponseRelation;
            if (model != null) {
                var modelEquation = model.DoseResponseModelEquation;
                var modelParameters = model.DoseResponseModelParameterValues;
                var modelDoseUnit = hazardCharacterisation.TestSystemHazardCharacterisation.DoseUnit;
                var modelDoseUnitCorrectionFactor = modelDoseUnit
                    .GetDoseAlignmentFactor(hazardCharacterisationUnit.ExposureUnit, substance.MolecularMass);
                var rModel = new RModelHealthImpact();
            }
            return individualEffects;
        }

        private int getSimulatedId(T exposure) {
            if (ExposureType == ExposureType.Acute) {
                return ((ITargetIndividualDayExposure)exposure).SimulatedIndividualDayId;
            } else {
                return exposure.SimulatedIndividualId;
            }
        }
    }
}
