using MCRA.Utils.DataFileReading;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using System.Globalization;
using System.Reflection;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation {

    public class NoelHazardCharacterisationImputationCalculator : HazardCharacterisationImputationCalculatorBase {

        private static List<NoelRecord> _noelRecords = null;

        public NoelHazardCharacterisationImputationCalculator(
            Effect effect,
            int percentile,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            IKineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels
        ) : base(effect, percentile, interSpeciesFactorModels, kineticConversionFactorCalculator, intraSpeciesVariabilityModels) {
            _noelRecords = _noelRecords ?? readMunroNoelCollection();
        }

        /// <summary>
        /// Creates the hazard characterisation records used for imputation.
        /// </summary>
        /// <param name="substance"></param>
        /// <returns></returns>
        protected override List<IHazardCharacterisationModel> getImputationTargetDoseRecords(
            Compound substance,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetDoseUnit,
            IRandom kineticModelRandomGenerator
        ) {
            if (_imputationRecords == null) {
                _imputationRecords = createHazardCharacterisations(
                    _effect,
                    targetDoseUnit,
                    hazardDoseTypeConverter,
                    _interSpeciesFactorModels,
                    _kineticConversionFactorCalculator,
                    _intraSpeciesVariabilityModels,
                    kineticModelRandomGenerator
                );
            }
            if (substance.CramerClass != null) {
                return _imputationRecords
                    .Where(r => r.Substance.CramerClass == substance.CramerClass)
                    .ToList();
            } else {
                return _imputationRecords.ToList();
            }
        }

        private static List<IHazardCharacterisationModel> createHazardCharacterisations(
            Effect effect,
            TargetUnit targetDoseUnit,
            HazardDoseConverter hazardDoseTypeConverter,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            IKineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels,
            IRandom kineticModelRandomGenerator
        ) {
            var cramerClasses = new int[] { 1, 2, 3 };
            var substances = cramerClasses
                .Select(r => new Compound() { CramerClass = r })
                .ToDictionary(r => r.CramerClass);
            var result = _noelRecords
                .Select(noelRecord => createHazardCharacterisationRecord(
                    noelRecord, 
                    effect, 
                    targetDoseUnit, 
                    hazardDoseTypeConverter, 
                    interSpeciesFactorModels, 
                    kineticConversionFactorCalculator, 
                    intraSpeciesVariabilityModels, 
                    kineticModelRandomGenerator, 
                    substances)
                )
                .Where(r => !double.IsNaN(r.Value))
                .Cast<IHazardCharacterisationModel>()
                .ToList();
            return result;
        }

        private static HazardCharacterisationModel createHazardCharacterisationRecord(
            NoelRecord r,
            Effect effect,
            TargetUnit targetDoseUnit,
            HazardDoseConverter hazardDoseTypeConverter,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            IKineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels,
            IRandom kineticModelRandomGenerator,
            Dictionary<int?, Compound> substances
        ) {
            var expressionTypeConversionFactor = hazardDoseTypeConverter
                .GetExpressionTypeConversionFactor(PointOfDepartureType.Noael);
            var alignedTestSystemHazardDose = hazardDoseTypeConverter
                .ConvertToTargetUnit(r.DoseUnit, substances[r.CramerClass], r.Noel);
            var targetUnitAlignmentFactor = alignedTestSystemHazardDose / r.Noel;

            var interSpeciesFactor = InterSpeciesFactorModelsBuilder
                .GetInterSpeciesFactor(interSpeciesFactorModels, null, r.Species, null);

            var alignedHazardDoseUnit = new TargetUnit(
                r.TargetUnit.Target,
                targetDoseUnit.SubstanceAmountUnit,
                targetDoseUnit.ConcentrationMassUnit,
                targetDoseUnit.TimeScaleUnit
            );

            var kineticConversionFactor = kineticConversionFactorCalculator
                .ComputeKineticConversionFactor(
                    alignedTestSystemHazardDose * (1D / interSpeciesFactor),
                    alignedHazardDoseUnit,
                    substances[r.CramerClass],
                    ExposureType.Chronic,
                    targetDoseUnit,
                    kineticModelRandomGenerator
                );

            var intraSpeciesVariabilityModel = intraSpeciesVariabilityModels.Get(null);
            var intraSpeciesFactor = intraSpeciesVariabilityModel?.Factor ?? 1D;
            var intraSpeciesVariabilityGsd = intraSpeciesVariabilityModel?.GeometricStandardDeviation ?? double.NaN;

            var combinedAssessmentFactor = (1D / interSpeciesFactor)
                * (1D / intraSpeciesFactor)
                * kineticConversionFactor
                * expressionTypeConversionFactor;

            return new HazardCharacterisationModel() {
                Substance = substances[r.CramerClass],
                Effect = effect,
                TargetUnit = targetDoseUnit,
                Value = alignedTestSystemHazardDose * combinedAssessmentFactor,
                GeometricStandardDeviation = intraSpeciesVariabilityGsd,
                HazardCharacterisationType = HazardCharacterisationType.Unspecified,
                PotencyOrigin = PotencyOrigin.Munro,
                CombinedAssessmentFactor = combinedAssessmentFactor,
                TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() {
                    Species = r.Species,
                    ExposureRoute = r.ExposureRoute,
                    HazardDose = r.Noel,
                    DoseUnit = DoseUnit.mgPerKgBWPerDay,
                    TargetUnitAlignmentFactor = targetUnitAlignmentFactor,
                    ExpressionTypeConversionFactor = expressionTypeConversionFactor,
                    InterSystemConversionFactor = 1D / interSpeciesFactor,
                    KineticConversionFactor = kineticConversionFactor,
                    IntraSystemConversionFactor = 1D / intraSpeciesFactor,
                }
            };
        }

        private static List<NoelRecord> readMunroNoelCollection() {
            var records = new List<NoelRecord>();
            using (var stream = Assembly.Load("MCRA.Simulation").GetManifestResourceStream("MCRA.Simulation.Resources.MunroTTCs.MunroTTCs.csv")) {
                using (var csvReader = new CsvDataReader(stream)) {
                    var headers = csvReader.Header
                        .Select((h, ix) => (h, ix))
                        .ToDictionary(r => r.h, r => r.ix, StringComparer.InvariantCultureIgnoreCase);

                    while (csvReader.Read()) {
                        var species = Convert.ToString(csvReader.GetValue(headers["Species"]));
                        var administrationType = Convert.ToString(csvReader.GetValue(headers["AdministrationType"]));
                        var exposureRoute = ExposureRouteConverter.FromString(Convert.ToString(csvReader.GetValue(headers["ExposureRoute"])));
                        var noel = Convert.ToDouble(csvReader.GetValue(headers["NOEL"]), CultureInfo.InvariantCulture);
                        var cramerClass = Convert.ToInt32(csvReader.GetValue(headers["StructuralClass"]), CultureInfo.InvariantCulture);
                        var record = new NoelRecord() {
                            Species = species,
                            ExposureRoute = exposureRoute,
                            AdministrationType = administrationType,
                            Noel = noel,
                            CramerClass = cramerClass,
                        };
                        records.Add(record);
                    }
                }
            }

            return records;
        }

        #region NOELS

        public List<double> NoelsCramerClassI {
            get {
                return _noelRecords.Where(r => r.CramerClass == 1).Select(r => r.Noel).ToList();
            }
        }

        public List<double> NoelsCramerClassII {
            get {
                return _noelRecords.Where(r => r.CramerClass == 2).Select(r => r.Noel).ToList();
            }
        }

        public List<double> NoelsCramerClassIII {
            get {
                return _noelRecords.Where(r => r.CramerClass == 3).Select(r => r.Noel).ToList();
            }
        }


        public List<double> NoelsCramerClassUnknown {
            get {
                return _noelRecords.Select(r => r.Noel).ToList();
            }
        }

        #endregion
    }
}
