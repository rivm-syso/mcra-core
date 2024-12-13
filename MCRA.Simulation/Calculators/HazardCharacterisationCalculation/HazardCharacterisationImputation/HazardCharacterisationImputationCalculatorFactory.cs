using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation {
    public static class HazardCharacterisationImputationCalculatorFactory {

        /// <summary>
        /// Creates a hazard characterisation imputation calculator instance.
        /// </summary>
        /// <param name="imputationMethod"></param>
        /// <param name="effect"></param>
        /// <param name="imputationCandidates"></param>
        /// <param name="interSpeciesFactorModels"></param>
        /// <param name="kineticConversionFactorCalculator"></param>
        /// <param name="intraSpeciesVariabilityModels"></param>
        /// <returns></returns>
        public static IHazardCharacterisationImputationCalculator Create(
            HazardDoseImputationMethodType imputationMethod,
            Effect effect,
            ICollection<IHazardCharacterisationModel> imputationCandidates,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            IKineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels
        ) {
            return imputationMethod switch {
                HazardDoseImputationMethodType.MunroP5 => new NoelHazardCharacterisationImputationCalculator(effect, 5, interSpeciesFactorModels, kineticConversionFactorCalculator, intraSpeciesVariabilityModels),
                HazardDoseImputationMethodType.MunroUnbiased => new NoelHazardCharacterisationImputationCalculator(effect, 50, interSpeciesFactorModels, kineticConversionFactorCalculator, intraSpeciesVariabilityModels),
                HazardDoseImputationMethodType.HazardDosesP5 => new AvailableHazardCharacterisationsImputationCalculator(effect, imputationCandidates, 5, interSpeciesFactorModels, kineticConversionFactorCalculator, intraSpeciesVariabilityModels),
                HazardDoseImputationMethodType.HazardDosesUnbiased => new AvailableHazardCharacterisationsImputationCalculator(effect, imputationCandidates, 50, interSpeciesFactorModels, kineticConversionFactorCalculator, intraSpeciesVariabilityModels),
                _ => throw new NotImplementedException(message: $"Imputation method {imputationMethod} is not yet implemented!"),
            };
        }
    }
}
