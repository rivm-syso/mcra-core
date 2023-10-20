using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationsFromDataSummarySection : ActionSummaryBase {

        private readonly double _lowerVariabilityPecentile = 2.5;
        private readonly double _upperVariabilityPecentile = 97.5;

        public SerializableDictionary<TargetUnit, List<HazardCharacterisationsFromDataSummaryRecord>> ChartRecords { get; set; } = new();

        public bool AllHazardsAtTarget { get; set; }

        public List<HazardCharacterisationsFromDataSummaryRecord> Records { 
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
        /// Summarizes the hazard characterisations obtained from data.
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="hazardCharacterisationModelsCollections"></param>
        public void Summarize(
            Effect effect,
            ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationModelsCollections
        ) {
            AllHazardsAtTarget = hazardCharacterisationModelsCollections
                .All(p => p.TargetUnit.ExposureRoute == ExposureRouteType.AtTarget);

            // First, create the bins of substances per target unit, for the box plots.
            // Second, out of these bins we create all records for the table.
            var chartRecords = hazardCharacterisationModelsCollections
                .ToDictionary(
                    c => c.TargetUnit,
                    d => d.HazardCharacterisationModels
                    .Select(m =>
                        new HazardCharacterisationsFromDataSummaryRecord {
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
                            TargetDoseLowerBound = m.Value.GetVariabilityDistributionPercentile(_lowerVariabilityPecentile),
                            TargetDoseUpperBound = m.Value.GetVariabilityDistributionPercentile(_upperVariabilityPecentile),
                            NumberOfUncertaintySets = (m.Value.HazardCharacterisationsUncertains?.Any() ?? false) 
                                ? m.Value.HazardCharacterisationsUncertains.Count 
                                : null,
                            Median = (m.Value.HazardCharacterisationsUncertains?.Any() ?? false) 
                                ? m.Value.HazardCharacterisationsUncertains.Select(c => c.Value).Percentile(50)
                                : double.NaN,
                            Minimum = (m.Value.HazardCharacterisationsUncertains?.Any() ?? false) 
                                ? m.Value.HazardCharacterisationsUncertains.Min(c => c.Value) 
                                : double.NaN,
                            Maximum = (m.Value.HazardCharacterisationsUncertains?.Any() ?? false) 
                                ? m.Value.HazardCharacterisationsUncertains.Max(c => c.Value) 
                                : double.NaN,
                        }
                    )
                    .ToList()
                );
            ChartRecords = new SerializableDictionary<TargetUnit, List<HazardCharacterisationsFromDataSummaryRecord>>(chartRecords);
        }
    }
}
