using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation {
    public class AvailableHazardCharacterisationsImputationCalculator : HazardCharacterisationImputationCalculatorBase {

        /// <summary>
        /// Imputation of hazard characterisations for substances by means of
        /// sampling from the hazard characterisations available for other
        /// substances.
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="imputationRecords"></param>
        /// <param name="percentile"></param>
        /// <param name="interSpeciesFactorModels"></param>
        /// <param name="kineticConversionFactorCalculator"></param>
        /// <param name="intraSpeciesVariabilityModels"></param>
        public AvailableHazardCharacterisationsImputationCalculator(
            Effect effect,
            ICollection<IHazardCharacterisationModel> imputationRecords,
            int percentile,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            IKineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels
        ) : base (effect, percentile, interSpeciesFactorModels, kineticConversionFactorCalculator, intraSpeciesVariabilityModels) {
            _imputationRecords = imputationRecords;
        }

        /// <summary>
        /// Creates the hazard characterisation records used for imputation.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="hazardDoseTypeConverter"></param>
        /// <param name="targetDoseUnit"></param>
        /// <param name="kineticModelRandomGenerator"></param>
        /// <returns></returns>
        protected override List<IHazardCharacterisationModel> getImputationTargetDoseRecords(
            Compound substance,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetDoseUnit,
            IRandom kineticModelRandomGenerator
        ) {
            var cramerClass = substance.CramerClass != null && substance.CramerClass >= 1 && substance.CramerClass <= 3 ? substance.CramerClass : -1;
            var records = cramerClass > 0 && _imputationRecords.Any(r => r.Substance.CramerClass == cramerClass)
                ? _imputationRecords.Where(r => r.Substance.CramerClass == cramerClass).ToList()
                : _imputationRecords;
            return records
                .Where(r => !double.IsNaN(r.Value))
                .ToList();
        }
    }
}
