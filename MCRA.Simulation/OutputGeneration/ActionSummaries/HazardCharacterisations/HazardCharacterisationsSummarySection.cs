using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationsSummarySection : ActionSummarySectionBase {

        private readonly double _lowerVariabilityPecentile = 2.5;
        private readonly double _upperVariabilityPecentile = 97.5;

        public SerializableDictionary<TargetUnit, List<HazardCharacterisationsSummaryRecord>> ChartRecords { get; set; } = new();
        public string TargetDoseLevel { get; set; }
        public ExposureType ExposureType { get; set; }
        public TargetLevelType TargetDoseLevelType { get; set; }
        public bool UseDoseResponseModels { get; set; }
        public bool UseBMDL { get; set; }
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
        public bool AllHazardsAtTarget { get; set; }

        public List<HazardCharacterisationsSummaryRecord> Records { 
            get {
                return ChartRecords
                    .SelectMany(r => r.Value.Select(r => r))
                    .OrderBy(r => r.EffectName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.EffectCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.BiologicalMatrix, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        /// <summary>
        /// Summarizes the target doses.
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="substances"></param>
        /// <param name="hazardCharacterisationModelsCollections"></param>
        /// <param name="targetDoseLevelType"></param>
        /// <param name="exposureType"></param>
        /// <param name="targetDosesCalculationMethod"></param>
        /// <param name="useDoseResponseModels"></param>
        /// <param name="useAdditionalAssessmentFactor"></param>
        /// <param name="additionalAssessmentFactor"></param>
        /// <param name="convertToSingleMatrix"></param>
        /// <param name="isCompute"></param>
        /// <param name="hasUncertainty"></param>
        public void Summarize(
            Effect effect,
            ICollection<Compound> substances,
            ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationModelsCollections,
            TargetLevelType targetDoseLevelType,
            ExposureType exposureType,
            TargetDosesCalculationMethod targetDosesCalculationMethod,
            bool useDoseResponseModels,
            bool useAdditionalAssessmentFactor,
            double additionalAssessmentFactor,
            bool convertToSingleMatrix,
            bool isCompute,
            bool useBmdl,
            bool hasUncertainty
        ) {
            ExposureType = exposureType;
            TargetDoseLevelType = targetDoseLevelType;
            IsCompute = isCompute;
            UseDoseResponseModels = useDoseResponseModels;
            UseBMDL = useBmdl;
            TargetDosesCalculationMethod = targetDosesCalculationMethod;
            UseKineticModel = convertToSingleMatrix
                || targetDosesCalculationMethod == TargetDosesCalculationMethod.InVitroBmds;
            UseAssessmentFactor = useAdditionalAssessmentFactor;
            AdditionalAssessmentFactor = additionalAssessmentFactor;
            AllHazardsAtTarget = hazardCharacterisationModelsCollections.All(p => p.TargetUnit.ExposureRoute == ExposureRouteType.AtTarget);

            // First, create the bins of substances per target unit, for the box plots.
            // Second, out of these bins we create all records for the table.
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
                            TargetDoseLowerBound = m.Value.GetVariabilityDistributionPercentile(_lowerVariabilityPecentile),
                            TargetDoseUpperBound = m.Value.GetVariabilityDistributionPercentile(_upperVariabilityPecentile),
                        }
                    )
                    .ToList()
                );
            ChartRecords = new SerializableDictionary<TargetUnit, List<HazardCharacterisationsSummaryRecord>>(chartRecords);

            FailedRecordCount = substances.Count - Records.Distinct(r => r.CompoundCode).Count();
            IsDistributionIntraSpecies = Records.Any(c => c.GeometricStandardDeviation > 1);
            IsDistributionInterSpecies = Records.Where(c => c.NominalInterSpeciesConversionFactor != 1)
                .Select(c => c.NominalInterSpeciesConversionFactor)
                .Distinct().Count() > 1;
            UseInterSpeciesConversionFactors = Records
                .Any(c => !double.IsNaN(c.NominalInterSpeciesConversionFactor) && c.NominalInterSpeciesConversionFactor != 1);
            UseIntraSpeciesConversionFactors = Records
                .Any(c => !double.IsNaN(c.NominalIntraSpeciesConversionFactor) && c.NominalIntraSpeciesConversionFactor != 1);
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
            ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationModelsCollections
        ) {
            var modelLookup = hazardCharacterisationModelsCollections
                .SelectMany(c => c.HazardCharacterisationModels.Select(m => new {
                    c.TargetUnit,
                    Substance = m.Key,
                    Model = m.Value
                }))
                .ToDictionary(r => (r.Substance.Code, r.TargetUnit.Target), r => r.Model);

            foreach (var chartRecords in ChartRecords) {
                var target = chartRecords.Key.Target;
                foreach (var record in chartRecords.Value) {
                    if (modelLookup.ContainsKey((record.CompoundCode, target))) {
                        var model = modelLookup[(record.CompoundCode, target)];
                        record.TargetDoseUncertaintyValues.Add(model.Value);
                        var plower = model.GetVariabilityDistributionPercentile(_lowerVariabilityPecentile);
                        var pupper = model.GetVariabilityDistributionPercentile(_upperVariabilityPecentile);
                        record.TargetDoseLowerBoundUncertaintyValues.Add(plower);
                        record.TargetDoseUpperBoundUncertaintyValues.Add(pupper);
                    }
                }
            }
        }
    }
}
