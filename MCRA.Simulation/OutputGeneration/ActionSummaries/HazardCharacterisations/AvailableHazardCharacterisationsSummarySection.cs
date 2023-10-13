using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AvailableHazardCharacterisationsSummarySection : SummarySection {

        public bool AllHazardsAtTarget { get; set; }

        public List<AvailableHazardCharacterisationsSummaryRecord> Records {
            get {
                return ChartRecords.SelectMany(r => r.Value.Select(r => r))
                    .OrderBy(r => r.EffectName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.EffectCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.BiologicalMatrix, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        public SerializableDictionary<TargetUnit, List<AvailableHazardCharacterisationsSummaryRecord>> ChartRecords { get; set; } = new();

        public void Summarize(
            Effect effect,
            ICollection<HazardCharacterisationModelsCollection> availableHazardCharacterisationModels
        ) {
            AllHazardsAtTarget = availableHazardCharacterisationModels.All(p => p.TargetUnit.ExposureRoute == ExposureRouteType.AtTarget);

            // Create bins of substances per target unit, for the box plots. Second, out of these bins we create all records for the table.
            var chartRecords = availableHazardCharacterisationModels
                .ToDictionary(
                    c => c.TargetUnit,
                    d => d.HazardCharacterisationModels
                    .Select(m =>
                        new AvailableHazardCharacterisationsSummaryRecord() {
                            ModelCode = m.Value.Code,
                            CompoundName = m.Key.Name,
                            CompoundCode = m.Key.Code,
                            EffectName = effect?.Name ?? m.Value.TestSystemHazardCharacterisation.Effect.Name,
                            EffectCode = effect?.Code ?? m.Value.TestSystemHazardCharacterisation.Effect.Name,
                            BiologicalMatrix = d.TargetUnit.BiologicalMatrix.GetShortDisplayName(),
                            HazardCharacterisation = m.Value?.Value ?? double.NaN,
                            Unit = d.TargetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                            GeometricStandardDeviation = m.Value?.GeometricStandardDeviation ?? double.NaN,
                            SystemDoseUnit = m.Value?.TestSystemHazardCharacterisation?.DoseUnit != null
                                ? m.Value?.TestSystemHazardCharacterisation?.DoseUnit.GetShortDisplayName()
                                : null,
                            SystemExpressionType = m.Value?.TestSystemHazardCharacterisation.ExpressionType.GetShortDisplayName().ToLower(),
                            SystemHazardCharacterisation = m.Value?.TestSystemHazardCharacterisation.HazardDose ?? double.NaN,
                            Species = m.Value?.TestSystemHazardCharacterisation?.Species,
                            Organ = m.Value?.TestSystemHazardCharacterisation?.Organ,
                            ExposureRoute = m.Value?.TestSystemHazardCharacterisation?.ExposureRoute.GetShortDisplayName(),
                            PotencyOrigin = m.Value?.PotencyOrigin.GetShortDisplayName(),
                            UnitConversionFactor = m.Value?.TestSystemHazardCharacterisation?.TargetUnitAlignmentFactor ?? double.NaN,
                            ExpressionTypeConversionFactor = m.Value?.TestSystemHazardCharacterisation?.ExpressionTypeConversionFactor ?? double.NaN,
                            NominalInterSpeciesConversionFactor = m.Value?.TestSystemHazardCharacterisation?.InterSystemConversionFactor ?? double.NaN,
                            NominalIntraSpeciesConversionFactor = m.Value?.TestSystemHazardCharacterisation?.IntraSystemConversionFactor ?? double.NaN,
                            NominalKineticConversionFactor = m.Value?.TestSystemHazardCharacterisation?.KineticConversionFactor ?? double.NaN,
                            AdditionalConversionFactor = m.Value?.TestSystemHazardCharacterisation?.AdditionalConversionFactor ?? double.NaN,
                        }
                    )
                    .ToList()
                );
            ChartRecords = new SerializableDictionary<TargetUnit, List<AvailableHazardCharacterisationsSummaryRecord>>(chartRecords);
        }
    }
}
