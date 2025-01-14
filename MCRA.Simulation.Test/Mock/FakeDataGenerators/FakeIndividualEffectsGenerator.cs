using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock individual effects
    /// </summary>
    public static class FakeIndividualEffectsGenerator {

        /// <summary>
        /// Creates substance individual effects.
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="substances"></param>
        /// <param name="fractionZeroExposure"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static Dictionary<Compound, List<IndividualEffect>> Create(
            ICollection<SimulatedIndividual> individuals,
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
            ICollection<SimulatedIndividual> individuals,
            double fractionZeroExposure,
            IRandom random,
            double? ced = null
        ) {
            return create(individuals, fractionZeroExposure, random, ced);
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
                                SimulatedIndividual = r.SimulatedIndividual,
                                CriticalEffectDose = (.5 + random.Next()) * r.CriticalEffectDose,
                                Exposure = (.5 + random.Next()) * r.Exposure
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
            List<SimulatedIndividual> individuals,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            Compound reference
        ) {
            var individualEffects = new List<IndividualEffect>();
            for (int i = 0; i < individuals.Count; i++) {
                var cedRef = individualEffectsBySubstance[reference].ElementAt(i).CriticalEffectDose;
                var cumulativeRecord = new IndividualEffect() {
                    SimulatedIndividual = individuals[i],
                    CriticalEffectDose = cedRef,
                    Exposure = 0
                };
                foreach (var substance in individualEffectsBySubstance.Keys) {
                    var substanceIndividualEffect = individualEffectsBySubstance[substance].ElementAt(i);
                    var rpfSub = cedRef / substanceIndividualEffect.CriticalEffectDose;
                    cumulativeRecord.Exposure += rpfSub * substanceIndividualEffect.Exposure;
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
        /// <param name="ced"></param>
        /// <returns></returns>
        private static List<IndividualEffect> create(
            ICollection<SimulatedIndividual> individuals,
            double fractionZeroExposure,
            IRandom random,
            double? ced = null
        ) {
            var result = individuals
                .Select(r => {
                    var criticalEffectDose = ced ?? LogNormalDistribution.Draw(random, 0, 1);
                    var exposure = random.NextDouble() < fractionZeroExposure ? 0D : LogNormalDistribution.Draw(random, 0, 1);
                    return new IndividualEffect() {
                        SimulatedIndividual = r,
                        CriticalEffectDose = criticalEffectDose,
                        Exposure = exposure,
                        HazardExposureRatio = hazardExposureRatio(HealthEffectType.Risk, criticalEffectDose, exposure),
                        ExposureHazardRatio = exposureHazardRatio(HealthEffectType.Risk, criticalEffectDose, exposure)
                    };
                })
                .ToList();
            return result;
        }
        private static double hazardExposureRatio(HealthEffectType healthEffectType, double CriticalEffectDose, double ExposureConcentration) {
            var iced = CriticalEffectDose;
            var iexp = ExposureConcentration;
            if (healthEffectType == HealthEffectType.Benefit) {
                return iced > iexp / SimulationConstants.MOE_eps ? iexp / iced : SimulationConstants.MOE_eps;
            } else {
                return iexp > iced / SimulationConstants.MOE_eps ? iced / iexp : SimulationConstants.MOE_eps;
            }
        }

        private static double exposureHazardRatio(HealthEffectType healthEffectType, double CriticalEffectDose, double ExposureConcentration) {
            if (healthEffectType == HealthEffectType.Benefit) {
                return CriticalEffectDose / ExposureConcentration;
            } else {
                return ExposureConcentration / CriticalEffectDose;
            }
        }
    }
}
