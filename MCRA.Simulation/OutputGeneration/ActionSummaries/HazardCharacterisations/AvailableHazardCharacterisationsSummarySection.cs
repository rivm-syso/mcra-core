﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AvailableHazardCharacterisationsSummarySection : SummarySection {

        public List<AvailableHazardCharacterisationsSummaryRecord> Records {
            get {
                return ChartRecords.SelectMany(r => r.Value)
                    .OrderBy(r => r.EffectName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.EffectCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.BiologicalMatrix, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        public SerializableDictionary<TargetUnit, List<AvailableHazardCharacterisationsSummaryRecord>> ChartRecords { get; set; } = [];

        public void Summarize(
            Effect effect,
            ICollection<HazardCharacterisationModelsCollection> availableHazardCharacterisationModels
        ) {
            // Create bins of substances per target unit, for the box plots. Second, out of these bins we create all records for the table.
            var chartRecords = availableHazardCharacterisationModels
                .ToDictionary(
                    c => c.TargetUnit,
                    d => d.HazardCharacterisationModels
                    .Select(m =>
                        new AvailableHazardCharacterisationsSummaryRecord() {
                            ModelCode = m.Code,
                            CompoundName = m.Substance.Name,
                            CompoundCode = m.Substance.Code,
                            EffectName = effect?.Name ?? m.TestSystemHazardCharacterisation.Effect.Name,
                            EffectCode = effect?.Code ?? m.TestSystemHazardCharacterisation.Effect.Name,
                            BiologicalMatrix = d.TargetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                                ? d.TargetUnit.BiologicalMatrix.GetDisplayName()
                                : null,
                            HazardCharacterisation = m.Value,
                            Unit = d.TargetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                            GeometricStandardDeviation = m.GeometricStandardDeviation,
                            SystemDoseUnit = m.TestSystemHazardCharacterisation?.DoseUnit != null
                                ? m.TestSystemHazardCharacterisation?.DoseUnit.GetShortDisplayName()
                                : null,
                            SystemExpressionType = m.TestSystemHazardCharacterisation.ExpressionType != ExpressionType.None
                                ? m.TestSystemHazardCharacterisation.ExpressionType.GetDisplayName()
                                : null,
                            SystemHazardCharacterisation = m.TestSystemHazardCharacterisation.HazardDose,
                            Species = m.TestSystemHazardCharacterisation?.Species,
                            Organ = m.TestSystemHazardCharacterisation?.Organ,
                            ExposureRoute = m.TestSystemHazardCharacterisation != null && m.TestSystemHazardCharacterisation.ExposureRoute != ExposureRoute.Undefined
                                ? m.TestSystemHazardCharacterisation.ExposureRoute.GetDisplayName()
                                : null,
                            PotencyOrigin = m.PotencyOrigin.GetDisplayName(),
                            UnitConversionFactor = m.TestSystemHazardCharacterisation?.TargetUnitAlignmentFactor ?? double.NaN,
                            ExpressionTypeConversionFactor = m.TestSystemHazardCharacterisation?.ExpressionTypeConversionFactor ?? double.NaN,
                            NominalInterSpeciesConversionFactor = m.TestSystemHazardCharacterisation?.InterSystemConversionFactor ?? double.NaN,
                            NominalIntraSpeciesConversionFactor = m.TestSystemHazardCharacterisation?.IntraSystemConversionFactor ?? double.NaN,
                            NominalKineticConversionFactor = m.TestSystemHazardCharacterisation?.KineticConversionFactor ?? double.NaN,
                            AdditionalConversionFactor = m.TestSystemHazardCharacterisation?.AdditionalConversionFactor ?? double.NaN,
                        }
                    )
                    .ToList()
                );
            ChartRecords = new SerializableDictionary<TargetUnit, List<AvailableHazardCharacterisationsSummaryRecord>>(chartRecords);
        }
    }
}
