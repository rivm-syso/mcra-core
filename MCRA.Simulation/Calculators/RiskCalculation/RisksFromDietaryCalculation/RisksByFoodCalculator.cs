using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public class RisksByFoodCalculator : RiskCalculatorBase {

        public RisksByFoodCalculator(HealthEffectType healthEffectType)
            : base(healthEffectType) {
        }

        /// <summary>
        /// Calculates health effects for modelled foods.
        /// </summary>
        /// <param name="targetIndividualDayExposures"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="hazardCharacterisationUnit"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="referenceSubstance"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Dictionary<Food, List<IndividualEffect>> ComputeByModelledFood(
            ICollection<DietaryIndividualDayTargetExposureWrapper> targetIndividualDayExposures,
            TargetUnit exposureUnit,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            TargetUnit hazardCharacterisationUnit,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Food> modelledFoods,
            Compound referenceSubstance
        ) {
            var referenceHazardModel = hazardCharacterisations[referenceSubstance];

            var foodKeys = new ConcurrentBag<Food>(modelledFoods);
            var effectsPerFoodDict = new ConcurrentDictionary<Food, ConcurrentBag<IndividualEffect>>(
                foodKeys.ToDictionary(k => k, v => new ConcurrentBag<IndividualEffect>())
            );

            var rpfDict = new ConcurrentDictionary<Compound, double>(relativePotencyFactors);
            var mspDict = new ConcurrentDictionary<Compound, double>(membershipProbabilities);
            var isPerPerson = !exposureUnit.IsPerBodyWeight();

            Parallel.ForEach(targetIndividualDayExposures, idv => {
                var alignmentFactor = exposureUnit
                    .GetAlignmentFactor(
                        hazardCharacterisationUnit,
                        referenceSubstance.MolecularMass,
                        double.NaN
                    );

                var modelledFoodTotalExposures = idv
                    .GetModelledFoodTotalExposures(rpfDict, mspDict);

                var criticalEffectDoseZero = referenceHazardModel
                    .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                var idvEffectZero = new IndividualEffect {
                    SimulatedIndividualId = idv.SimulatedIndividualDayId,
                    SamplingWeight = idv.IndividualSamplingWeight,
                    Individual = idv.Individual,
                    Exposure = 0,
                    CriticalEffectDose = criticalEffectDoseZero,
                    IntraSpeciesDraw = idv.IntraSpeciesDraw,
                    EquivalentTestSystemDose = 0,
                    HazardExposureRatio = getHazardExposureRatio(HealthEffectType, criticalEffectDoseZero, 0),
                    ExposureHazardRatio = getExposureHazardRatio(HealthEffectType, criticalEffectDoseZero, 0),
                    IsPositive = false
                };

                foreach (var f in foodKeys) {
                    var idvEffect = idvEffectZero;
                    if(modelledFoodTotalExposures.TryGetValue(f, out var intake)) {
                        var criticalEffectDose = referenceHazardModel
                            .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                        var exposure = alignmentFactor * intake.Exposure;
                        idvEffect = new IndividualEffect {
                            SimulatedIndividualId = idv.SimulatedIndividualDayId,
                            SamplingWeight = idv.IndividualSamplingWeight,
                            Individual = idv.Individual,
                            Exposure = exposure,
                            CriticalEffectDose = criticalEffectDose,
                            IntraSpeciesDraw = idv.IntraSpeciesDraw,
                            EquivalentTestSystemDose = exposure / referenceHazardModel.CombinedAssessmentFactor,
                            HazardExposureRatio = getHazardExposureRatio(HealthEffectType, criticalEffectDose, exposure),
                            ExposureHazardRatio = getExposureHazardRatio(HealthEffectType, criticalEffectDose, exposure),
                            IsPositive = exposure > 0
                        };
                    }

                    if(effectsPerFoodDict.TryGetValue(f, out var effects)) {
                        effects.Add(idvEffect);
                    }
                }
            });

            var result = effectsPerFoodDict.ToDictionary(r => r.Key, v => v.Value.ToList());
            return result;
        }

        /// <summary>
        /// Calculates health effects for modelled foods.
        /// </summary>
        /// <param name="targetIndividualExposures"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="hazardCharacterisationUnit"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="referenceSubstance"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Dictionary<Food, List<IndividualEffect>> ComputeByModelledFood(
            ICollection<DietaryIndividualTargetExposureWrapper> targetIndividualExposures,
            TargetUnit exposureUnit,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            TargetUnit hazardCharacterisationUnit,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Food> modelledFoods,
            Compound referenceSubstance
        ) {
            var referenceHazardModel = hazardCharacterisations[referenceSubstance];
            var result = new Dictionary<Food, List<IndividualEffect>>();

            foreach (var idv in targetIndividualExposures) {
                var alignmentFactor = exposureUnit
                    .GetAlignmentFactor(
                        hazardCharacterisationUnit,
                        referenceSubstance.MolecularMass,
                        double.NaN
                    );
                var modelledFoodTotalExposures = idv.GetModelledFoodTotalExposures(
                    relativePotencyFactors,
                    membershipProbabilities);
                foreach (var kvp in modelledFoodTotalExposures) {
                    var exposure = alignmentFactor * kvp.Value.Exposure;
                    var criticalEffectDose = referenceHazardModel
                        .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                    var idvEffect = new IndividualEffect {
                        SimulatedIndividualId = idv.SimulatedIndividualId,
                        SamplingWeight = idv.IndividualSamplingWeight,
                        Individual = idv.Individual,
                        Exposure = exposure,
                        IntraSpeciesDraw = idv.IntraSpeciesDraw,
                        CriticalEffectDose = criticalEffectDose,
                        EquivalentTestSystemDose = exposure / referenceHazardModel.CombinedAssessmentFactor,
                        HazardExposureRatio = getHazardExposureRatio(HealthEffectType, criticalEffectDose, exposure),
                        ExposureHazardRatio = getExposureHazardRatio(HealthEffectType, criticalEffectDose, exposure),
                        IsPositive = exposure > 0
                    };
                    if (!result.TryGetValue(kvp.Key, out var effects)) {
                        effects = new List<IndividualEffect>();
                        result.Add(kvp.Key, effects);
                    }
                    effects.Add(idvEffect);
                }
                //Add foods without exposure
                var remainingFoods = modelledFoods.Except(modelledFoodTotalExposures.Keys).ToList();
                foreach (var food in remainingFoods) {
                    var criticalEffectDose = referenceHazardModel
                        .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                    var idvEffect = new IndividualEffect {
                        SimulatedIndividualId = idv.SimulatedIndividualId,
                        SamplingWeight = idv.IndividualSamplingWeight,
                        Individual = idv.Individual,
                        Exposure = 0,
                        CriticalEffectDose = criticalEffectDose,
                        IntraSpeciesDraw = idv.IntraSpeciesDraw,
                        EquivalentTestSystemDose = 0,
                        HazardExposureRatio = getHazardExposureRatio(HealthEffectType, criticalEffectDose, 0),
                        ExposureHazardRatio = getExposureHazardRatio(HealthEffectType, criticalEffectDose, 0),
                        IsPositive = false
                    };
                    if (!result.TryGetValue(food, out var effects)) {
                        effects = new List<IndividualEffect>();
                        result.Add(food, effects);
                    }
                    effects.Add(idvEffect);

                }
            };
            return result;
        }
    }
}
