using System.Collections.Concurrent;
using System.Diagnostics;
using DocumentFormat.OpenXml.Office2013.Excel;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public class RisksByModelledFoodSubstanceCalculator : RiskCalculatorBase {

        public RisksByModelledFoodSubstanceCalculator(HealthEffectType healthEffectType)
            : base(healthEffectType) {
        }

        /// <summary>
        /// Calculates health effects for modelled foods and substances.
        /// </summary>
        /// <param name="targetIndividualDayExposures"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="hazardCharacterisationUnit"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="modelledFoods"></param>
        /// <param name="referenceSubstance"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), List<IndividualEffect>> ComputeByModelledFoodSubstance(
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

            var foodsSubstancesKeys = new ConcurrentBag<(Food, Compound)>(
                modelledFoods.SelectMany(f => hazardCharacterisations.Keys.Select(s => (f, s)))
            );

            var effectsPerFoodCompoundDict = new ConcurrentDictionary<(Food, Compound), ConcurrentBag<IndividualEffect>>(
                foodsSubstancesKeys.ToDictionary(k => k, v => new ConcurrentBag<IndividualEffect>())
            );

            var rpfDict =  new ConcurrentDictionary<Compound, double>(relativePotencyFactors);
            var mspDict = new ConcurrentDictionary<Compound, double>(membershipProbabilities);
            var isPerPerson = !exposureUnit.IsPerBodyWeight();

            Parallel.ForEach(targetIndividualDayExposures, idv => {
                var alignmentFactor = exposureUnit
                    .GetAlignmentFactor(
                        hazardCharacterisationUnit,
                        referenceSubstance.MolecularMass,
                        idv.CompartmentWeight
                    );

                var modelledFoodSubstanceTotalExposures = idv
                    .GetModelledFoodSubstanceTotalExposures(rpfDict, mspDict, isPerPerson);

                var criticalEffectDoseZero = referenceHazardModel
                    .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                var idvEffectZero = new IndividualEffect {
                    SamplingWeight = idv.IndividualSamplingWeight,
                    SimulatedIndividualId = idv.SimulatedIndividualDayId,
                    Exposure = 0,
                    IntraSpeciesDraw = idv.IntraSpeciesDraw,
                    CriticalEffectDose = criticalEffectDoseZero,
                    EquivalentTestSystemDose = 0,
                    HazardExposureRatio = getHazardExposureRatio(HealthEffectType, criticalEffectDoseZero, 0),
                    ExposureHazardRatio = getExposureHazardRatio(HealthEffectType, criticalEffectDoseZero, 0),
                    IsPositive = false
                };

                foreach (var fs in foodsSubstancesKeys) {
                    var idvEffect = idvEffectZero;
                    if(modelledFoodSubstanceTotalExposures.TryGetValue(fs, out var intake)) {
                        var criticalEffectDose = referenceHazardModel
                            .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                        var exposure = alignmentFactor * intake.Exposure;
                        idvEffect = new IndividualEffect {
                            SamplingWeight = idv.IndividualSamplingWeight,
                            SimulatedIndividualId = idv.SimulatedIndividualDayId,
                            Exposure = exposure,
                            IntraSpeciesDraw = idv.IntraSpeciesDraw,
                            CriticalEffectDose = criticalEffectDose,
                            EquivalentTestSystemDose = exposure / referenceHazardModel.CombinedAssessmentFactor,
                            HazardExposureRatio = getHazardExposureRatio(HealthEffectType, criticalEffectDose, exposure),
                            ExposureHazardRatio = getExposureHazardRatio(HealthEffectType, criticalEffectDose, exposure),
                            IsPositive = exposure > 0
                        };
                    }

                    if (effectsPerFoodCompoundDict.TryGetValue(fs, out var effects)) {
                        effects.Add(idvEffect);
                    }
                }
            });

            var result = effectsPerFoodCompoundDict.ToDictionary(r => r.Key, v => v.Value.ToList());
            return result;
        }

        /// <summary>
        /// Calculates health effects for modelled foods and substances.
        /// </summary>
        /// <param name="targetIndividualExposures"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="hazardCharacterisationUnit"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="modelledFoods"></param>
        /// <param name="referenceSubstance"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), List<IndividualEffect>> ComputeByModelledFoodSubstance(
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

            var substances = hazardCharacterisations.Keys;
            var foodSubstanceCombinations = new List<(Food, Compound)>();
            foreach (var food in modelledFoods) {
                foreach (var substance in substances) {
                    foodSubstanceCombinations.Add((food, substance));
                }
            }

            var result = new Dictionary<(Food, Compound), List<IndividualEffect>>();
            foreach (var idv in targetIndividualExposures) {
                var alignmentFactor = exposureUnit
                    .GetAlignmentFactor(
                        hazardCharacterisationUnit,
                        referenceSubstance.MolecularMass,
                        idv.CompartmentWeight
                    );

                var modelledFoodSubstanceTotalExposures = idv
                    .GetModelledFoodSubstanceTotalExposures(
                        relativePotencyFactors,
                        membershipProbabilities,
                        !exposureUnit.IsPerBodyWeight()
                    );
                foreach (var kvp in modelledFoodSubstanceTotalExposures) {
                    var criticalEffectDose = referenceHazardModel
                        .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                    var exposure = alignmentFactor * kvp.Value.Exposure;

                    var idvEffect = new IndividualEffect {
                        SamplingWeight = idv.IndividualSamplingWeight,
                        SimulatedIndividualId = idv.SimulatedIndividualId,
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

                //Add foods x substances without exposure
                var remainingFoodSubstances = foodSubstanceCombinations.Except(modelledFoodSubstanceTotalExposures.Keys).ToList(); ;
                foreach (var item in remainingFoodSubstances) {
                    var criticalEffectDose = referenceHazardModel
                        .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                    var idvEffect = new IndividualEffect {
                        SamplingWeight = idv.IndividualSamplingWeight,
                        SimulatedIndividualId = idv.SimulatedIndividualId,
                        Exposure = 0,
                        IntraSpeciesDraw = idv.IntraSpeciesDraw,
                        CriticalEffectDose = criticalEffectDose,
                        EquivalentTestSystemDose = 0,
                        HazardExposureRatio = getHazardExposureRatio(HealthEffectType, criticalEffectDose, 0),
                        ExposureHazardRatio = getExposureHazardRatio(HealthEffectType, criticalEffectDose, 0),
                        IsPositive = false
                    };

                    if (!result.TryGetValue(item, out var effects)) {
                        effects = new List<IndividualEffect>();
                        result.Add(item, effects);
                    }
                    effects.Add(idvEffect);

                }
            };
            return result;
        }
    }
}
