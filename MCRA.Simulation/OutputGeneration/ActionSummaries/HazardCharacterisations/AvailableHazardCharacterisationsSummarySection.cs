using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AvailableHazardCharacterisationsSummarySection : SummarySection {

        public string TargetOrgan { get; set; }
        public string PointOfDeparture { get; set; }

        public List<AvailableHazardCharacterisationsSummaryRecord> Records { get; set; }

        public void Summarize(
            Effect effect,
            ICollection<IHazardCharacterisationModel> availableTargetDoseModels
        ) {
            Records = availableTargetDoseModels
                .Select(model => {
                    return new AvailableHazardCharacterisationsSummaryRecord() {
                        ModelCode = model.Code,
                        CompoundName = model.Substance.Name,
                        CompoundCode = model.Substance.Code,
                        EffectName = effect?.Name ?? model.TestSystemHazardCharacterisation.Effect.Name,
                        EffectCode = effect?.Code ?? model.TestSystemHazardCharacterisation.Effect.Name,
                        HazardCharacterisation = model?.Value ?? double.NaN,
                        GeometricStandardDeviation = model?.GeometricStandardDeviation ?? double.NaN,
                        SystemDoseUnit = model?.TestSystemHazardCharacterisation?.DoseUnit != null 
                            ? model?.TestSystemHazardCharacterisation?.DoseUnit.GetShortDisplayName()
                            : null,
                        SystemHazardCharacterisation = model?.TestSystemHazardCharacterisation.HazardDose?? double.NaN,
                        Species = model?.TestSystemHazardCharacterisation?.Species,
                        Organ = model?.TestSystemHazardCharacterisation?.Organ,
                        ExposureRoute = model?.TestSystemHazardCharacterisation?.ExposureRoute.GetShortDisplayName(),
                        PotencyOrigin = model?.PotencyOrigin.GetShortDisplayName(),
                        UnitConversionFactor = model?.TestSystemHazardCharacterisation?.TargetUnitAlignmentFactor ?? double.NaN,
                        ExpressionTypeConversionFactor = model?.TestSystemHazardCharacterisation?.ExpressionTypeConversionFactor ?? double.NaN,
                        NominalInterSpeciesConversionFactor = model?.TestSystemHazardCharacterisation?.InterSystemConversionFactor ?? double.NaN,
                        NominalIntraSpeciesConversionFactor = model?.TestSystemHazardCharacterisation?.IntraSystemConversionFactor ?? double.NaN,
                        NominalKineticConversionFactor = model?.TestSystemHazardCharacterisation?.KineticConversionFactor ?? double.NaN,
                        AdditionalConversionFactor = model?.TestSystemHazardCharacterisation?.AdditionalConversionFactor ?? double.NaN,
                    };
                })
                .OrderBy(r => r.EffectName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.EffectCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
