using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public class RisksByModelledFoodSubstanceCalculator {

        /// <summary>
        /// Calculates health effects for modelled foods and substances.
        /// </summary>
        /// <param name="targetIndividualDayExposures"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), List<IndividualEffect>> ComputeByModelledFoodSubstance(
            ICollection<DietaryIndividualDayTargetExposureWrapper> targetIndividualDayExposures,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceSubstance,
            TargetUnit exposureUnit,
            bool isPerPerson
        ) {
            var referenceHazardModel = hazardCharacterisations[referenceSubstance];
            var individualEffectsDictionary = new Dictionary<(Food, Compound), List<IndividualEffect>>();

            foreach (var idv in targetIndividualDayExposures) {
                var dict = idv.GetModelledFoodSubstanceTotalExposures(relativePotencyFactors, membershipProbabilities, isPerPerson);
                foreach (var kvp in dict) {
                    if (idv.IntraSpeciesDraw == 0) {
                        throw new System.Exception("Random draw contains zeros");
                    }

                    var criticalEffectDose = referenceHazardModel.DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                    if (isPerPerson) {
                        criticalEffectDose *= idv.CompartmentWeight;
                    }

                    var idvEffect = new IndividualEffect {
                        SamplingWeight = idv.IndividualSamplingWeight,
                        SimulationId = idv.SimulatedIndividualDayId,
                        ExposureConcentration = kvp.Value.Exposure,
                        CompartmentWeight = idv.CompartmentWeight,
                        IntraSpeciesDraw = idv.IntraSpeciesDraw,
                        CriticalEffectDose = criticalEffectDose,
                        EquivalentTestSystemDose = kvp.Value.Exposure / referenceHazardModel.CombinedAssessmentFactor
                    };

                    if (!individualEffectsDictionary.TryGetValue(kvp.Key, out var effects)) {
                        effects = new List<IndividualEffect>();
                        individualEffectsDictionary.Add(kvp.Key, effects);
                    }
                    effects.Add(idvEffect);
                }
            };

            return individualEffectsDictionary;
        }

        /// <summary>
        /// Calculates health effects for modelled foods and substances.
        /// </summary>
        /// <param name="targetIndividualExposures"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), List<IndividualEffect>> ComputeByModelledFoodSubstance(
            ICollection<DietaryIndividualTargetExposureWrapper> targetIndividualExposures,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceSubstance,
            TargetUnit exposureUnit,
            bool isPerPerson
        ) {
            var referenceHazardModel = hazardCharacterisations[referenceSubstance];
            var individualEffectsDictionary = new Dictionary<(Food, Compound), List<IndividualEffect>>();

            foreach (var idv in targetIndividualExposures) {
                var dict = idv.GetModelledFoodSubstanceTotalExposures(relativePotencyFactors, membershipProbabilities, isPerPerson);

                foreach (var kvp in dict) {
                    if (idv.IntraSpeciesDraw == 0) {
                        throw new System.Exception("Random draw contains zeros");
                    }

                    var criticalEffectDose = referenceHazardModel.DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                    if (isPerPerson) {
                        criticalEffectDose *= idv.CompartmentWeight;
                    }

                    var idvEffect = new IndividualEffect {
                        SamplingWeight = idv.IndividualSamplingWeight,
                        SimulationId = idv.SimulatedIndividualId,
                        ExposureConcentration = kvp.Value.Exposure,
                        CompartmentWeight = idv.CompartmentWeight,
                        IntraSpeciesDraw = idv.IntraSpeciesDraw,
                        CriticalEffectDose = criticalEffectDose,
                        EquivalentTestSystemDose = kvp.Value.Exposure / referenceHazardModel.CombinedAssessmentFactor
                    };

                    if (!individualEffectsDictionary.TryGetValue(kvp.Key, out var effects)) {
                        effects = new List<IndividualEffect>();
                        individualEffectsDictionary.Add(kvp.Key, effects);
                    }
                    effects.Add(idvEffect);
                }
            };

            return individualEffectsDictionary;
        }
    }
}
