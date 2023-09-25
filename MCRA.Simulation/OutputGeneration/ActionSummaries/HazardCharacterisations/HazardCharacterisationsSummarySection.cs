using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationsSummarySection : ActionSummaryBase {

        public List<HazardCharacterisationsSummaryRecord> Records { get; set; }
        public string TargetDoseLevel { get; set; }
        public ExposureType ExposureType { get; set; }
        public TargetLevelType TargetDoseLevelType { get; set; }
        public bool UseDoseResponseModels { get; set; }
        public string PotencyOrigins { get; set; }
        public bool IsCompute { get; set; }
        public TargetDosesCalculationMethod TargetDosesCalculationMethod { get; set; }

        public bool IsDistributionInterSpecies { get; set; }
        public bool IsDistributionIntraSpecies { get; set; }

        public bool UseInterSpeciesConversionFactors { get; set; }
        public bool UseIntraSpeciesConversionFactors { get; set; }
        public bool UseAssessmentFactor { get; set; }
        public double AdditionalAssessmentFactor { get; set; }

        public double InterSpeciesConversionFactor { get; set; } = double.NaN;
        public double IntraSpeciesConversionFactor { get; set; } = double.NaN;

        public bool UseKineticModel { get; set; }
        /// <summary>
        /// Summarizes the target doses.
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="substances"></param>
        /// <param name="hazardCharacterisations"></param>
        public void Summarize(
            Effect effect,
            ICollection<Compound> substances,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            TargetLevelType targetDoseLevelType,
            ExposureType exposureType,
            TargetDosesCalculationMethod targetDosesCalculationMethod,
            bool useDoseResponseModels,
            bool useAdditionalAssessmentFactor,
            double additionalAssessmentFactor,
            bool isCompute
        ) {
            ExposureType = exposureType;
            TargetDoseLevelType = targetDoseLevelType;
            IsCompute = isCompute;
            UseDoseResponseModels = useDoseResponseModels;
            TargetDosesCalculationMethod = targetDosesCalculationMethod;
            UseKineticModel = targetDoseLevelType == TargetLevelType.Internal || targetDosesCalculationMethod == TargetDosesCalculationMethod.InVitroBmds;
            UseAssessmentFactor = useAdditionalAssessmentFactor;
            AdditionalAssessmentFactor = additionalAssessmentFactor;
            Records = substances
                .Select(substance => {
                    hazardCharacterisations.TryGetValue(substance, out var model);
                    var record = new HazardCharacterisationsSummaryRecord() {
                        CompoundName = substance.Name,
                        CompoundCode = substance.Code,
                        EffectName = effect?.Name,
                        EffectCode = effect?.Code,
                        HazardCharacterisation = model?.Value ?? double.NaN,
                        GeometricStandardDeviation = model?.GeometricStandardDeviation ?? double.NaN,
                        TargetDoseUncertaintyValues = new List<double>(),
                        PotencyOrigin = model?.PotencyOrigin.GetShortDisplayName(),
                        HazardCharacterisationType = model?.HazardCharacterisationType.GetShortDisplayName(),
                        NominalInterSpeciesConversionFactor = model?.TestSystemHazardCharacterisation?.InterSystemConversionFactor ?? double.NaN,
                        NominalIntraSpeciesConversionFactor = model?.TestSystemHazardCharacterisation?.IntraSystemConversionFactor ?? double.NaN,
                    };
                    return record;
                })
                .OrderBy(r => r.EffectName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.EffectCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                .ToList();

            PotencyOrigins = string.Join(",", Records.Select(c => c.PotencyOrigin).Distinct());
            IsDistributionIntraSpecies = Records.Any(c => c.GeometricStandardDeviation > 1);
            IsDistributionInterSpecies = Records.Where(c => c.NominalInterSpeciesConversionFactor != 1)
                .Select(c => c.NominalInterSpeciesConversionFactor)
                .Distinct().Count() > 1;
            UseInterSpeciesConversionFactors = !Records.All(c => c.NominalInterSpeciesConversionFactor == 1);
            UseIntraSpeciesConversionFactors = !Records.All(c => c.NominalIntraSpeciesConversionFactor == 1);
            var interSpeciesConversionFactors = Records.Select(c => c.NominalInterSpeciesConversionFactor).Distinct();
            var intraSpeciesConversionFactors = Records.Select(c => c.NominalIntraSpeciesConversionFactor).Distinct();
            if (interSpeciesConversionFactors.Count() == 1) {
                InterSpeciesConversionFactor = interSpeciesConversionFactors.First();
            }
            if (intraSpeciesConversionFactors.Count() == 1) {
                IntraSpeciesConversionFactor = intraSpeciesConversionFactors.First();
            }
        }
    }
}
