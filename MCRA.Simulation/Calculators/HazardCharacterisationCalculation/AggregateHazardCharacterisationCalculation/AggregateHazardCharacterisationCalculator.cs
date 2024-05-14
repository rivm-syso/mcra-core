using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.AggregateHazardCharacterisationCalculation {

    /// <summary>
    /// Class for computing aggregate hazard characterisations from
    /// multiple candidates.
    /// </summary>
    public class AggregateHazardCharacterisationCalculator {

        /// <summary>
        /// Computes a dictionary with the selected target doses per substance.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="effect"></param>
        /// <param name="availableHazardCharacterisations"></param>
        /// <param name="targetDoseSelectionMethod"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public Dictionary<Compound, IHazardCharacterisationModel> SelectTargetDoses(
            ICollection<Compound> substances,
            Effect effect,
            ICollection<IHazardCharacterisationModel> availableHazardCharacterisations,
            TargetDoseSelectionMethod targetDoseSelectionMethod,
            IRandom generator
        ) {
            // Select target doses from available doses
            var result = new Dictionary<Compound, IHazardCharacterisationModel>();
            var groupedAvailableTargetDoses = availableHazardCharacterisations
                .GroupBy(r => r.Substance)
                .ToDictionary(r => r.Key, r => r.ToList());
            foreach (var substance in substances) {
                if (groupedAvailableTargetDoses.ContainsKey(substance) && groupedAvailableTargetDoses[substance].Any()) {
                    switch (targetDoseSelectionMethod) {
                        case TargetDoseSelectionMethod.Draw:
                            var imputedTargetDose = groupedAvailableTargetDoses[substance][generator.Next(0, groupedAvailableTargetDoses[substance].Count)];
                            result.Add(substance, imputedTargetDose);
                            break;
                        case TargetDoseSelectionMethod.MostToxic:
                            result.Add(substance, groupedAvailableTargetDoses[substance].MinBy(r => r.Value));
                            break;
                        case TargetDoseSelectionMethod.Aggregate:
                            var aggregatedTargetDose = computeAggregatedTargetDoseModel(
                                groupedAvailableTargetDoses[substance],
                                effect
                            );
                            result.Add(substance, aggregatedTargetDose);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Computes an aggregated target dose model, using the median target dose
        /// of the presented available models.
        /// </summary>
        /// <param name="targetHazardDoseModels"></param>
        /// <param name="effect"></param>
        /// <returns></returns>
        private IHazardCharacterisationModel computeAggregatedTargetDoseModel(
            ICollection<IHazardCharacterisationModel> targetHazardDoseModels,
            Effect effect
        ) {
            if (targetHazardDoseModels.Count == 1) {
                return targetHazardDoseModels.First();
            } else {
                if (targetHazardDoseModels.Distinct(r => r.Substance).Count() > 1) {
                    throw new Exception("Cannot aggregate hazard characterisations with different substances.");
                }
                if (targetHazardDoseModels.Distinct(r => r.Target).Count() > 1) {
                    throw new Exception("Cannot aggregate hazard characterisations with different targets.");
                }
                if (targetHazardDoseModels.Distinct(r => r.DoseUnit).Count() > 1) {
                    throw new Exception("Cannot aggregate hazard characterisations with different dose units.");
                }
                var substance = targetHazardDoseModels.First().Substance;
                var targetUnit = targetHazardDoseModels.First().TargetUnit;
                var aggregatedHazardCharacterisation = 1D / targetHazardDoseModels.Select(r => 1 / r.Value).Average();
                var result = new AggregateHazardCharacterisation() {
                    Code = $"Aggregate-HC-{substance.Code}",
                    Effect = effect,
                    Substance = substance,
                    TargetUnit = targetUnit,
                    Value = aggregatedHazardCharacterisation,
                    PotencyOrigin = PotencyOrigin.Aggregated,
                    HazardCharacterisationType = HazardCharacterisationType.Unspecified,
                    Sources = targetHazardDoseModels,
                };
                return result;
            }
        }
    }
}
