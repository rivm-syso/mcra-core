using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationsSummarySection : ActionSummaryBase {

        private double _lowerVariabilityPecentile = 2.5;
        private double _upperVariabilityPecentile = 97.5;

        public List<HazardCharacterisationsSummaryRecord> Records { get; set; }
        public SerializableDictionary<TargetUnit, List<HazardCharacterisationsSummaryRecord>> ChartRecords { get; set; } = new();
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
        public int FailedRecordCount { get; set; }

        /// <summary>
        /// Summarizes the target doses.
        /// </summary>
        /// <param name="hazardCharacterisations"></param>
        public void Summarize(
            Effect effect,
            ICollection<Compound> substances,
            ICollection<HazardCharacterisationModelsCollection> hazardCharacterisationModelsCollections,
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

            // First, create the bins of substances per target unit, for the box plots. Second, out of these bins we create all records for the table.
            var chartRecords = hazardCharacterisationModelsCollections
                .ToDictionary(
                    c => c.TargetUnit, 
                    d => d.HazardCharacterisationModels
                    .Select(m =>
                        new HazardCharacterisationsSummaryRecord {
                            CompoundName = m.Key.Name,
                            CompoundCode = m.Key.Code,
                            EffectName = effect?.Name,
                            EffectCode = effect?.Code,
                            BiologicalMatrix = m.Value.Target.BiologicalMatrix.GetShortDisplayName(),
                            HazardCharacterisation = m.Value.Value,
                            Unit = d.TargetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                            GeometricStandardDeviation = m.Value.GeometricStandardDeviation,
                            TargetDoseUncertaintyValues = new List<double>(),
                            TargetDoseLowerBoundUncertaintyValues = new List<double>(),
                            TargetDoseUpperBoundUncertaintyValues = new List<double>(),
                            PotencyOrigin = m.Value.PotencyOrigin.GetShortDisplayName(),
                            HazardCharacterisationType = m.Value.HazardCharacterisationType.GetShortDisplayName(),
                            NominalInterSpeciesConversionFactor = m.Value.TestSystemHazardCharacterisation?.InterSystemConversionFactor ?? double.NaN,
                            NominalIntraSpeciesConversionFactor = m.Value.TestSystemHazardCharacterisation?.IntraSystemConversionFactor ?? double.NaN,
                            TargetDoseLowerBound = m.Value?.GetVariabilityDistributionPercentile(_lowerVariabilityPecentile) ?? double.NaN,
                            TargetDoseUpperBound = m.Value?.GetVariabilityDistributionPercentile(_upperVariabilityPecentile) ?? double.NaN,
                        }
                    )
                    .ToList()
                ); 
            ChartRecords = new SerializableDictionary<TargetUnit, List<HazardCharacterisationsSummaryRecord>>(chartRecords);

            Records = ChartRecords.SelectMany(r => r.Value.Select(r => r))
                .OrderBy(r => r.EffectName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.EffectCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.BiologicalMatrix, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                .ToList();

            FailedRecordCount = substances.Count - Records.Distinct(r => r.CompoundCode).Count();
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

        public void SummarizeUncertain(
            ICollection<HazardCharacterisationModelsCollection> hazardCharacterisationModelsCollections
        ) {
            // TODO: this summarizer method assumes only one substance per collection
            // this should be updated with exposure target keys in case of hazard characterisation
            // collections with multiple targets.
            var modelsLookup = hazardCharacterisationModelsCollections
                .SelectMany(r => r.HazardCharacterisationModels.Values)
                .ToDictionary(r => r.Substance.Code);
            foreach (var record in Records) {
                if (modelsLookup.ContainsKey(record.CompoundCode)) {
                    var model = modelsLookup[record.CompoundCode];
                    record.TargetDoseUncertaintyValues.Add(modelsLookup[record.CompoundCode].Value);
                    var plower = model.GetVariabilityDistributionPercentile(_lowerVariabilityPecentile);
                    var pupper = model.GetVariabilityDistributionPercentile(_upperVariabilityPecentile);
                    record.TargetDoseLowerBoundUncertaintyValues.Add(plower);
                    record.TargetDoseUpperBoundUncertaintyValues.Add(pupper);
                }
            }
        }
    }
}
