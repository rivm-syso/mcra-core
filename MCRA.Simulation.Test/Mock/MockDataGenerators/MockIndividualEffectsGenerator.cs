using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock individual effects
    /// </summary>
    public static class MockIndividualEffectsGenerator {

        private const double _eps = 10E7D;

        /// <summary>
        /// Creates substance individual effects.
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="substances"></param>
        /// <param name="fractionZeroExposure"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static Dictionary<Compound, List<IndividualEffect>> Create(
            ICollection<Individual> individuals,
            ICollection<Compound> substances,
            IRandom random,
            double[] fractionZeroExposure = null
        ) {
            var result = substances
                .Select((substance, ix) => {
                    var fractionZeros = fractionZeroExposure != null ? fractionZeroExposure[ix] : random.NextDouble();
                    var substanceIndividualEffects = create(individuals, fractionZeros, random);
                    return (substance, substanceIndividualEffects);
                })
                .ToDictionary(r => r.substance, r => r.substanceIndividualEffects);
            return result;
        }

        /// <summary>
        /// Creates a list of individual effects with fixed seed
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="fractionZeroExposure"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<IndividualEffect> Create(
            ICollection<Individual> individuals,
            double fractionZeroExposure,
            IRandom random
        ) {
            return create(individuals, fractionZeroExposure, random);
        }

        /// <summary>
        /// Creates bootstrap records for the provided substance individual effects.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="individualEffects"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static Dictionary<Compound, List<IndividualEffect>> CreateUncertain(
            List<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> individualEffects,
            IRandom random
        ) {
            return substances
                .ToDictionary(
                    substance => substance,
                    substance => {
                        var nominal = individualEffects[substance];
                        var unc = nominal
                            .Select(r => new IndividualEffect() {
                                SimulatedIndividualId = r.SimulatedIndividualId,
                                CompartmentWeight = r.CompartmentWeight,
                                SamplingWeight = r.SamplingWeight,
                                CriticalEffectDose = (.5 + random.Next()) * r.CriticalEffectDose,
                                ExposureConcentration = (.5 + random.Next()) * r.ExposureConcentration
                            })
                            .ToList();
                        return unc;
                    }
                );
        }

        /// <summary>
        /// Computes/generates cumulative individual effects from the individual effects by substance.
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static List<IndividualEffect> ComputeCumulativeIndividualEffects(
            List<Individual> individuals,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            Compound reference
        ) {
            var individualEffects = new List<IndividualEffect>();
            for (int i = 0; i < individuals.Count; i++) {
                var cedRef = individualEffectsBySubstance[reference].ElementAt(i).CriticalEffectDose;
                var cumulativeRecord = new IndividualEffect() {
                    SamplingWeight = individuals[i].SamplingWeight,
                    CriticalEffectDose = cedRef,
                    ExposureConcentration = 0
                };
                foreach (var substance in individualEffectsBySubstance.Keys) {
                    var substanceIndividualEffect = individualEffectsBySubstance[substance].ElementAt(i);
                    var rpfSub = cedRef / substanceIndividualEffect.CriticalEffectDose;
                    cumulativeRecord.ExposureConcentration += rpfSub * substanceIndividualEffect.ExposureConcentration;
                }
                individualEffects.Add(cumulativeRecord);
            }
            return individualEffects;
        }

        /// <summary>
        /// Creates a list of individual effects with a random generator
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="fractionZeroExposure"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static List<IndividualEffect> create(
            ICollection<Individual> individuals,
            double fractionZeroExposure,
            IRandom random
        ) {
            var result = individuals
                .Select(r => {
                    var criticalEffectDose = LogNormalDistribution.Draw(random, 0, 1);
                    var exposure = random.NextDouble() < fractionZeroExposure ? 0D : LogNormalDistribution.Draw(random, 0, 1);
                    return new IndividualEffect() {
                        SimulatedIndividualId = r.Id,
                        CriticalEffectDose = criticalEffectDose,
                        ExposureConcentration = exposure,
                        SamplingWeight = r.SamplingWeight,
                        ThresholdExposureRatio = thresholdExposureRatio(HealthEffectType.Risk, criticalEffectDose, exposure),
                        ExposureThresholdRatio = exposureThresholdRatio(HealthEffectType.Risk, criticalEffectDose, exposure)
                    };
                })
                .ToList();
            return result;
        }
        private static double thresholdExposureRatio(HealthEffectType healthEffectType, double CriticalEffectDose, double ExposureConcentration) {
            var iced = CriticalEffectDose;
            var iexp = ExposureConcentration;
            if (healthEffectType == HealthEffectType.Benefit) {
                return iced > iexp / _eps ? iexp / iced : _eps;
            } else {
                return iexp > iced / _eps ? iced / iexp : _eps;
            }
        }

        private static double exposureThresholdRatio(HealthEffectType healthEffectType, double CriticalEffectDose, double ExposureConcentration) {
            if (healthEffectType == HealthEffectType.Benefit) {
                return CriticalEffectDose / ExposureConcentration;
            } else {
                return ExposureConcentration / CriticalEffectDose;
            }
        }
    }
}
