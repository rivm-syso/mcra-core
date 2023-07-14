using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation.ForwardCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public class RiskCalculator<T> where T : ITargetIndividualExposure {

        private const double _eps = 10E7D;

        public RiskCalculator() {
            ExposureType = typeof(T) == typeof(ITargetIndividualDayExposure) ? ExposureType.Acute : ExposureType.Chronic;
        }

        protected ExposureType ExposureType { get; private set; }

        /// <summary>
        /// Calculates the risks by substance for the specified substances.
        /// </summary>
        public Dictionary<Compound, List<IndividualEffect>> ComputeBySubstance(
            ICollection<T> targetIndividualExposures,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            ICollection<Compound> substances,
            TargetUnit exposureUnit,
            HealthEffectType healthEffectType,
            bool isPerPerson
        ) {
            var results = substances
                .ToDictionary(
                    substance => substance,
                    substance => {
                        var targetDoseModel = hazardCharacterisations[substance];
                        var individualEffects = targetIndividualExposures
                            .AsParallel()
                            .Select(c => {
                                var exposureConcentration = c.GetSubstanceConcentrationAtTarget(substance, isPerPerson);
                                return new IndividualEffect {
                                    SamplingWeight = c.IndividualSamplingWeight,
                                    SimulatedIndividualId = GetSimulatedId(c),
                                    ExposureConcentration = exposureConcentration,
                                    CompartmentWeight = c.CompartmentWeight,
                                    IntraSpeciesDraw = c.IntraSpeciesDraw,
                                    IsPositive = exposureConcentration > 0
                                };
                            })
                            .ToList();
                        return CalculateRisk(
                            individualEffects,
                            substance,
                            targetDoseModel,
                            exposureUnit,
                            healthEffectType,
                            isPerPerson
                        );
                    });
            var individualEffectsDict = results;
            return individualEffectsDict;
        }

        /// <summary>
        /// Calculates cumulative individual risks based on RPF weighing.
        /// </summary>
        public List<IndividualEffect> ComputeCumulative(
            ICollection<T> targetIndividualExposures,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceSubstance,
            TargetUnit exposureUnit,
            HealthEffectType healthEffectType,
            bool isPerPerson
        ) {
            var individualCumulativeEffects = targetIndividualExposures
                .AsParallel()
                .Select(c => {
                    var exposure = c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson);
                    return new IndividualEffect {
                        ExposureConcentration = exposure,
                        SamplingWeight = c.IndividualSamplingWeight,
                        SimulatedIndividualId = GetSimulatedId(c),
                        CompartmentWeight = c.CompartmentWeight,
                        IntraSpeciesDraw = c.IntraSpeciesDraw,
                        IsPositive = exposure > 0,
                    };
                })
                .OrderBy(c => c.SimulatedIndividualId)
                .ToList();

            var result = CalculateRisk(
                individualCumulativeEffects,
                referenceSubstance,
                hazardCharacterisations[referenceSubstance],
                exposureUnit,
                healthEffectType,
                isPerPerson
            );
            return result;
        }

        /// <summary>
        /// Computes the cumulative risks based on sum of ratios approach.
        /// </summary>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="healthEffectType"></param>
        /// <returns></returns>
        public List<IndividualEffect> ComputeSumOfRatios(
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            IDictionary<Compound, double> membershipProbabilities,
            HealthEffectType healthEffectType
        ) {

            if (membershipProbabilities == null) {
                membershipProbabilities = individualEffectsBySubstance.ToDictionary(c => c.Key, c => 1d);
            }

            var individualEffects = individualEffectsBySubstance
               .SelectMany(c => c.Value, (v, s) => (Substance: v.Key, IndividualEffect: s))
               .ToLookup(c => c.IndividualEffect.SimulatedIndividualId, c => (c.Substance, c.IndividualEffect));

            var risks = individualEffects.Select(c => {
                var thresholdExposureRatio = 1 / c.Sum(r => 1 / getThresholdExposureRatio(healthEffectType, r.IndividualEffect.CriticalEffectDose, r.IndividualEffect.ExposureConcentration * membershipProbabilities[r.Substance]));
                var exposureThresholdRatio = c.Sum(r => getExposureThresholdRatio(healthEffectType, r.IndividualEffect.CriticalEffectDose, r.IndividualEffect.ExposureConcentration * membershipProbabilities[r.Substance]));
                return new IndividualEffect() {
                    ThresholdExposureRatio = thresholdExposureRatio,
                    ExposureThresholdRatio = exposureThresholdRatio,
                    SamplingWeight = c.First().IndividualEffect.SamplingWeight,
                    SimulatedIndividualId = c.First().IndividualEffect.SimulatedIndividualId,
                    IsPositive = !c.All(r => r.IndividualEffect.ExposureConcentration * membershipProbabilities[r.Substance] == 0),
                };
            }).ToList();

            
            return risks;
        }

        /// <summary>
        /// Calculate individual effects i.c. critical effect dose, threshold value/exposure, equivalent animal dose.
        /// Use the original critical effect dose; i.e., the critical effect dose defined on the original animal 
        /// (or in vitro system) of the dose response model.
        /// Note that the critical effect dose is corrected.
        /// A correction for (Exposure) Per Person is needed because exposures are at kg per bw level or per person,
        /// For the threshold value/exposure this is not needed in principal.
        /// Maybe a correction as applied her is incorrect and there should be one value for all humans
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="substance"></param>
        /// <param name="hazardCharacterisation"></param>
        /// <param name="intakeUnit"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected List<IndividualEffect> CalculateRisk(
            List<IndividualEffect> individualEffects,
            Compound substance,
            IHazardCharacterisationModel hazardCharacterisation,
            TargetUnit intakeUnit,
            HealthEffectType healthEffectType,
            bool isPerPerson
        ) {
            if (individualEffects.Any(c => c.IntraSpeciesDraw == 0)) {
                throw new Exception("Random draw contains zeros");
            }
            foreach (var item in individualEffects) {
                item.CriticalEffectDose = hazardCharacterisation.DrawIndividualHazardCharacterisation(item.IntraSpeciesDraw) * (isPerPerson ? item.CompartmentWeight : 1);
                item.EquivalentTestSystemDose = item.ExposureConcentration / hazardCharacterisation.CombinedAssessmentFactor;
                item.ThresholdExposureRatio = getThresholdExposureRatio(healthEffectType, item.CriticalEffectDose, item.ExposureConcentration);
                item.ExposureThresholdRatio = getExposureThresholdRatio(healthEffectType, item.CriticalEffectDose, item.ExposureConcentration);
            }

            // Forward calculation
            var model = hazardCharacterisation?.TestSystemHazardCharacterisation?.DoseResponseRelation;
            if (model != null) {
                var modelEquation = model.DoseResponseModelEquation;
                var modelParameters = model.DoseResponseModelParameterValues;
                var modelDoseUnit = hazardCharacterisation.TestSystemHazardCharacterisation.DoseUnit;
                var modelDoseUnitCorrectionFactor = modelDoseUnit.GetDoseAlignmentFactor(intakeUnit, substance.MolecularMass);
                var rModel = new RModelHealthImpact();
            }
            return individualEffects;
        }

        private int GetSimulatedId(T exposure) {
            if (ExposureType == ExposureType.Acute) {
                return (exposure as ITargetIndividualDayExposure).SimulatedIndividualDayId;
            } else {
                return exposure.SimulatedIndividualId;
            }
        }

        private double getThresholdExposureRatio(HealthEffectType healthEffectType, double CriticalEffectDose, double ExposureConcentration) {
            var iced = CriticalEffectDose;
            var iexp = ExposureConcentration;
            if (healthEffectType == HealthEffectType.Benefit) {
                return iced > iexp / _eps ? iexp / iced : _eps;
            } else {
                return iexp > iced / _eps ? iced / iexp : _eps;
            }
        }

        private double getExposureThresholdRatio(HealthEffectType healthEffectType, double CriticalEffectDose, double ExposureConcentration) {
            if (healthEffectType == HealthEffectType.Benefit) {
                return CriticalEffectDose / ExposureConcentration;
            } else {
                return ExposureConcentration / CriticalEffectDose;
            }
        }
    }
}
