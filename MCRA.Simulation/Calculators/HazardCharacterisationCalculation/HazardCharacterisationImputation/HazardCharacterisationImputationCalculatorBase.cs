using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation {
    public abstract class HazardCharacterisationImputationCalculatorBase : IHazardCharacterisationImputationCalculator {

        protected Effect _effect;

        protected int _percentile;

        protected ICollection<IHazardCharacterisationModel> _imputationRecords;

        protected IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> _interSpeciesFactorModels;
        protected IKineticConversionFactorCalculator _kineticConversionFactorCalculator;
        protected IDictionary<(Effect, Compound), IntraSpeciesFactorModel> _intraSpeciesVariabilityModels;

        public HazardCharacterisationImputationCalculatorBase(
            Effect effect,
            int percentile,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            IKineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels
        ) {
            _effect = effect;
            _percentile = percentile;
            _interSpeciesFactorModels = interSpeciesFactorModels;
            _kineticConversionFactorCalculator = kineticConversionFactorCalculator;
            _intraSpeciesVariabilityModels = intraSpeciesVariabilityModels;
        }

        /// <summary>
        /// Imputes the hazard dose based on the set of available hazard doses.
        /// In the nominal runs, the specified percentile will be used for
        /// imputation of the hazard doses of new compounds.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="hazardDoseTypeConverter"></param>
        /// <param name="targetUnit"></param>
        /// <param name="kineticModelRandomGenerator"></param>
        /// <returns></returns>
        public IHazardCharacterisationModel ImputeNominal(
            Compound substance,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetUnit,
            IRandom kineticModelRandomGenerator
        ) {
            var imputationRecords = getImputationTargetDoseRecords(
                    substance,
                    hazardDoseTypeConverter,
                    targetUnit,
                    kineticModelRandomGenerator
                )
                .Where(r => !double.IsNaN(r.Value))
                .ToList();

            var intraSpeciesFactorSource = _intraSpeciesVariabilityModels
                .Get(_effect, substance)?.Factor ?? 1D;

            // Get imputation values
            var imputationValues = imputationRecords
                .Select(r => r.Value * intraSpeciesFactorSource)
                .ToList();

            // Get nominal imputation value
            double imputedTargetDose;
            if (_percentile == 50) {
                imputedTargetDose = 1D / imputationValues.Select(r => 1D / r).Average();
            } else {
                imputedTargetDose = imputationValues.Percentile(_percentile);
            };

            var intraSpeciesFactorModel = _intraSpeciesVariabilityModels.Get(_effect, substance);
            var intraSpeciesFactor = intraSpeciesFactorModel?.Factor ?? 1D;
            var intraSpeciesGsd = intraSpeciesFactorModel?.GeometricStandardDeviation ?? double.NaN;

            // Create a new, aggregated hazard characterisation using the imputed hazard characterisations
            var hazardDosesResponseModel = new AggregateHazardCharacterisation() {
                Code = $"Imputed-HC-{substance.Code}",
                Substance = substance,
                Effect = _effect,
                PotencyOrigin = PotencyOrigin.Imputed,
                TargetUnit = targetUnit,
                Sources = imputationRecords,
                Value = imputedTargetDose * (1D / intraSpeciesFactor),
                HazardCharacterisationType = HazardCharacterisationType.Unspecified,
                GeometricStandardDeviation = intraSpeciesGsd,
                TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() {
                    IntraSystemConversionFactor = (1D / intraSpeciesFactor)
                }
            };

            return hazardDosesResponseModel;
        }

        /// <summary>
        /// Nominal imputation of hazard characterisations for all specified substances.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="hazardDoseConverter"></param>
        /// <param name="targetDoseUnit"></param>
        /// <param name="kineticModelRandomGenerator"></param>
        /// <returns></returns>
        public ICollection<IHazardCharacterisationModel> ImputeNominal(
            ICollection<Compound> substances,
            HazardDoseConverter hazardDoseConverter,
            TargetUnit targetDoseUnit,
            IRandom kineticModelRandomGenerator
        ) {
            var result = new List<IHazardCharacterisationModel>();
            foreach (var substance in substances) {
                var record = ImputeNominal(
                    substance,
                    hazardDoseConverter,
                    targetDoseUnit,
                    kineticModelRandomGenerator
                );
                result.Add(record);
            }
            return result;
        }

        /// <summary>
        /// Imputes the hazard dose based on the set of available hazard doses.
        /// In the nominal runs, the specified percentile will be used for imputation
        /// of the hazard doses of new substances.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="hazardDoseTypeConverter"></param>
        /// <param name="targetUnit"></param>
        /// <param name="hazardDosesRandomGenerator"></param>
        /// <param name="kineticModelRandomGenerator"></param>
        /// <returns></returns>
        public IHazardCharacterisationModel ImputeUncertaintyRun(
            Compound substance,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetUnit,
            IRandom hazardDosesRandomGenerator,
            IRandom kineticModelRandomGenerator
        ) {
            var imputationRecords = getImputationTargetDoseRecords(
                    substance,
                    hazardDoseTypeConverter,
                    targetUnit,
                    kineticModelRandomGenerator
                )
                .Where(r => !double.IsNaN(r.Value))
                .ToList();
            var imputationRecord = imputationRecords[hazardDosesRandomGenerator.Next(0, imputationRecords.Count)];

            var intraSpeciesFactorSource = _intraSpeciesVariabilityModels.Get(_effect, imputationRecord.Substance)?.Factor ?? 1D;

            var intraSpeciesFactorModel = _intraSpeciesVariabilityModels.Get(_effect, imputationRecord.Substance);
            var intraSpeciesFactor = intraSpeciesFactorModel?.Factor ?? 1D;
            var intraSpeciesGsd = intraSpeciesFactorModel?.GeometricStandardDeviation ?? double.NaN;

            var imputedHazardCharacterisation = new HazardCharacterisationModel() {
                Code = $"{imputationRecord.Code}-{substance.Code}",
                Substance = substance,
                Effect = _effect,
                TargetUnit = targetUnit,
                Value = (imputationRecord.Value * intraSpeciesFactorSource) * (1D / intraSpeciesFactor),
                PotencyOrigin = PotencyOrigin.Imputed,
                GeometricStandardDeviation = intraSpeciesGsd,
                TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() {
                    IntraSystemConversionFactor = (1D / intraSpeciesFactor)
                }
            };

            return imputedHazardCharacterisation;
        }

        /// <summary>
        /// Imputation of hazard characterisations of the specifies substances
        /// in the bootstrap run.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="hazardDoseConverter"></param>
        /// <param name="targetDoseUnit"></param>
        /// <param name="generator"></param>
        /// <param name="kineticModelRandomGenerator"></param>
        /// <returns></returns>
        public ICollection<IHazardCharacterisationModel> ImputeUncertaintyRun(
            ICollection<Compound> substances,
            HazardDoseConverter hazardDoseConverter,
            TargetUnit targetDoseUnit,
            IRandom generator,
            IRandom kineticModelRandomGenerator
        ) {
            var result = new List<IHazardCharacterisationModel>();
            foreach (var substance in substances) {
                var record = ImputeUncertaintyRun(
                    substance,
                    hazardDoseConverter,
                    targetDoseUnit,
                    generator,
                    kineticModelRandomGenerator
                );
                result.Add(record);
            }
            return result;
        }

        /// <summary>
        /// Returns the collection of records used for imputation.
        /// </summary>
        /// <returns></returns>
        public ICollection<IHazardCharacterisationModel> GetImputationRecords() {
            return _imputationRecords;
        }

        /// <summary>
        /// Creates the hazard characterisation records used for imputation.
        /// </summary>
        /// <param name="compound"></param>
        /// <param name="hazardDoseTypeConverter"></param>
        /// <param name="targetIntakeUnit"></param>
        /// <param name="kineticModelRandomGenerator"></param>
        /// <returns></returns>
        protected abstract List<IHazardCharacterisationModel> getImputationTargetDoseRecords(
            Compound compound,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetIntakeUnit,
            IRandom kineticModelRandomGenerator
        );
    }
}
