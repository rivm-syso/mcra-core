using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public sealed class ChronicRiskCalculator : RiskCalculatorBase {

        /// <summary>
        /// Calculates health effects.
        /// 1a) Single Compound Exposure Assessment: for a single compound;
        /// 1b) Cumulative Exposure Assessement: for the cumulative RPF weighted equivalent ;
        /// 2)  Cumulative Exposure Assessement: for individual compounds (multiple compounds)
        ///     and the inverse of the harmonic hazard index.
        /// </summary>
        /// <param name="targetIndividualExposures"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="intraSpeciesRandomGenerator"></param>
        public List<IndividualEffect> ComputeCumulative(
            ICollection<ITargetIndividualExposure> targetIndividualExposures,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceSubstance,
            TargetUnit exposureUnit,
            bool isPerPerson
        ) {
            var individualCumulativeEffects = targetIndividualExposures
                .Select(c => new IndividualEffect() {
                    ExposureConcentration = c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson),
                    SamplingWeight = c.IndividualSamplingWeight,
                    SimulationId = c.SimulatedIndividualId,
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
            ).Item2;
        }

        /// <summary>
        /// Calculates health effects.
        /// 1a) Single Compound Exposure Assessment: for a single compound;
        /// 1b) Cumulative Exposure Assessement: for the cumulative RPF weighted equivalent ;
        /// 2)  Cumulative Exposure Assessement: for individual compounds (multiple compounds)
        ///     and the inverse of the harmonic hazard index.
        /// </summary>
        /// <param name="targetIndividualExposures"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="substances"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="isPerPerson"></param>
        /// 
        /// <returns></returns>
        public Dictionary<Compound, List<IndividualEffect>> ComputeBySubstance(
                ICollection<ITargetIndividualExposure> targetIndividualExposures,
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
                                SimulationId = c.SimulatedIndividualId,
                                ExposureConcentration = c.TargetExposuresBySubstance.ContainsKey(substance)
                                    ? c.TargetExposuresBySubstance[substance].SubstanceAmount / (isPerPerson ? 1 : c.CompartmentWeight)
                                    : 0D,
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
            //To show your results
            var individualEffectsDictionary = results.ToDictionary(c => c.Item1, c => c.Item2);
            return individualEffectsDictionary;
        }
    }
}
