using System.Collections.Concurrent;
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
                        double.NaN
                    );

                var modelledFoodSubstanceTotalExposures = idv
                    .GetModelledFoodSubstanceTotalExposures(rpfDict, mspDict);

                var criticalEffectDoseZero = referenceHazardModel
                    .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                var idvEffectZero = new IndividualEffect {
                    SimulatedIndividualId = idv.SimulatedIndividualDayId,
                    SimulatedIndividual = idv.SimulatedIndividual,
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
                            SimulatedIndividualId = idv.SimulatedIndividualDayId,
                            SimulatedIndividual = idv.SimulatedIndividual,
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

            var result = effectsPerFoodCompoundDict.ToDictionary(v => v.Key, v => v.Value.ToList());
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

            var foodsSubstancesKeys = new ConcurrentBag<(Food, Compound)>(
                modelledFoods.SelectMany(f => hazardCharacterisations.Keys.Select(s => (f, s)))
            );

            var effectsPerFoodCompoundDict = new ConcurrentDictionary<(Food, Compound), ConcurrentBag<IndividualEffect>>(
                foodsSubstancesKeys.ToDictionary(k => k, v => new ConcurrentBag<IndividualEffect>())
            );

            var rpfDict = new ConcurrentDictionary<Compound, double>(relativePotencyFactors);
            var mspDict = new ConcurrentDictionary<Compound, double>(membershipProbabilities);
            var isPerPerson = !exposureUnit.IsPerBodyWeight();

            Parallel.ForEach(targetIndividualExposures, idv => {
                var alignmentFactor = exposureUnit
                    .GetAlignmentFactor(
                        hazardCharacterisationUnit,
                        referenceSubstance.MolecularMass,
                        double.NaN
                    );

                var modelledFoodSubstanceTotalExposures = idv
                    .GetModelledFoodSubstanceTotalExposures(rpfDict, mspDict);

                var criticalEffectDoseZero = referenceHazardModel
                    .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                var idvEffectZero = new IndividualEffect {
                    SimulatedIndividual = idv.SimulatedIndividual,
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
                    if (modelledFoodSubstanceTotalExposures.TryGetValue(fs, out var intake)) {
                        var criticalEffectDose = referenceHazardModel
                            .DrawIndividualHazardCharacterisation(idv.IntraSpeciesDraw);
                        var exposure = alignmentFactor * intake.Exposure;
                        idvEffect = new IndividualEffect {
                            SimulatedIndividual = idv.SimulatedIndividual,
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

            var result = effectsPerFoodCompoundDict.ToDictionary(v => v.Key, v => v.Value.ToList());
            return result;
        }
    }
}
