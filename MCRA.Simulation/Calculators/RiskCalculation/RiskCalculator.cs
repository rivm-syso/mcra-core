using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation.ForwardCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public class RiskCalculator<T> where T : ITargetIndividualExposure {

        public RiskCalculator() {
            ExposureType = typeof(T) == typeof(ITargetIndividualDayExposure) ? ExposureType.Acute : ExposureType.Chronic;
        }

        protected ExposureType ExposureType { get; private set; }

        /// <summary>
        /// Calculates health effects.
        /// 1a) Single Compound Exposure Assessment: for a single compound;
        /// 1b) Cumulative Exposure Assessement: for the cumulative RPF weighted equivalent ;
        /// 2) Cumulative Exposure Assessement: for individual compounds (multiple compounds) and the inverse of the harmonic hazard index.
        /// </summary>
        public List<IndividualEffect> ComputeCumulative(
            ICollection<T> targetIndividualExposures,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceSubstance,
            TargetUnit exposureUnit,
            bool isPerPerson
        ) {
            var individualCumulativeEffects = targetIndividualExposures
                .Select(c => new IndividualEffect {
                    ExposureConcentration = c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson),
                    SamplingWeight = c.IndividualSamplingWeight,
                    SimulationId = GetSimulatedId(c),
                    CompartmentWeight = c.CompartmentWeight,
                    IntraSpeciesDraw = c.IntraSpeciesDraw
                })
                .OrderBy(c => c.SimulationId)
                .ToList();

            return CalculateRisk(
                individualCumulativeEffects,
                referenceSubstance,
                hazardCharacterisations[referenceSubstance],
                exposureUnit,
                isPerPerson
            ).Effects;
        }

        /// <summary>
        /// Calculates health effects for modelled foods.
        /// </summary>
        public Dictionary<Compound, List<IndividualEffect>> ComputeBySubstance(
            ICollection<T> targetIndividualExposures,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            ICollection<Compound> substances,
            TargetUnit exposureUnit,
            bool isPerPerson
        ) {
            var results = substances
                .Select(substance => {
                    var targetDoseModel = hazardCharacterisations[substance];
                    var individualEffects = targetIndividualExposures
                        .AsParallel()
                        .Select(c => {
                            return new IndividualEffect {
                                SamplingWeight = c.IndividualSamplingWeight,
                                SimulationId = GetSimulatedId(c),
                                ExposureConcentration = c.GetSubstanceConcentrationAtTarget(substance, isPerPerson),
                                CompartmentWeight = c.CompartmentWeight,
                                IntraSpeciesDraw = c.IntraSpeciesDraw
                            };
                        })
                        .ToList();
                    return CalculateRisk(
                        individualEffects,
                        substance,
                        targetDoseModel,
                        exposureUnit,
                        isPerPerson
                    );
                });

            var individualEffectsDict = results.ToDictionary(c => c.Substance, c => c.Effects);
            return individualEffectsDict;
        }

        /// <summary>
        /// Calculate individual effects i.c. critical effect dose, margin of exposure, equivalent animal dose.
        /// Use the original critical effect dose; i.e., the critical effect dose defined on the original animal 
        /// (or in vitro system) of the dose response model.
        /// Note that the critical effectdose is corrected.
        /// A correction for (Exposure) Per Person is needed because exposures are at kg per bw level or per person,
        /// For the margin of exposure this is not needed in principal.
        /// Maybe a correction as applied her is incorrect and there should be one value for all humans
        /// </summary>
        protected (Compound Substance, List<IndividualEffect> Effects) CalculateRisk(
            List<IndividualEffect> individualEffects,
            Compound substance,
            IHazardCharacterisationModel hazardCharacterisation,
            TargetUnit intakeUnit,
            bool isPerPerson
        ) {
            if (individualEffects.Any(c => c.IntraSpeciesDraw == 0)) {
                throw new Exception("Random draw contains zeros");
            }
            foreach (var item in individualEffects) {
                item.CriticalEffectDose = hazardCharacterisation.DrawIndividualHazardCharacterisation(item.IntraSpeciesDraw) * (isPerPerson ? item.CompartmentWeight : 1);
                item.EquivalentTestSystemDose = item.ExposureConcentration / hazardCharacterisation.CombinedAssessmentFactor;
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
            return (substance, individualEffects);
        }

        private int GetSimulatedId(T exposure) {
            if (ExposureType == ExposureType.Acute) {
                return (exposure as ITargetIndividualDayExposure).SimulatedIndividualDayId;
            } else {
                return exposure.SimulatedIndividualId;
            }
        }
    }
}
