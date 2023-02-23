using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ImputedHazardCharacterisationsSummarySection : SummarySection {

        public List<ImputedHazardCharacterisationSummaryRecord> Records { get; set; }

        public void Summarize(
            Effect effect,
            ICollection<IHazardCharacterisationModel> imputedHazardCharacterisationModels
        ) {
            Records = imputedHazardCharacterisationModels
                .Select(model => {
                    var nominalIntraSpeciesConversionFactor = model?.TestSystemHazardCharacterisation?.IntraSystemConversionFactor ?? double.NaN;
                    var targetDose = model?.Value ?? double.NaN;
                    return new ImputedHazardCharacterisationSummaryRecord() {
                        CompoundName = model.Substance.Name,
                        CompoundCode = model.Substance.Code,
                        CramerClass = model.Substance?.CramerClass,
                        EffectName = effect?.Name,
                        EffectCode = effect?.Code,
                        HazardCharacterisation = targetDose,
                        GeometricStandardDeviation = model?.GeometricStandardDeviation ?? double.NaN,
                        NominalIntraSpeciesConversionFactor = model?.TestSystemHazardCharacterisation?.IntraSystemConversionFactor ?? double.NaN
                    };
                })
                .OrderBy(r => r.EffectName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
