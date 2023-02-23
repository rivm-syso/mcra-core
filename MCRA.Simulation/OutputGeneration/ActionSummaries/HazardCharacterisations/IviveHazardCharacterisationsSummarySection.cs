using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IviveHazardCharacterisationsSummarySection : SummarySection {

        public List<IviveHazardCharacterisationsSummaryRecord> Records { get; set; }

        public void Summarize(
            Effect effect,
            Compound referenceSubstance,
            ICollection<IviveHazardCharacterisation> iviveTargetDoseModels,
            TargetLevelType targetDoseLevelType
        ) {
            var referenceRecord = iviveTargetDoseModels.First(r => r.Substance == referenceSubstance);
            Records = iviveTargetDoseModels
                .Select(model => {
                    var internalRpf = referenceRecord.InternalHazardDose / model.InternalHazardDose;
                    var externalRpf = double.NaN;
                    if (targetDoseLevelType == TargetLevelType.External) {
                        externalRpf = referenceRecord.Value / model.Value;
                    }
                    var record = new IviveHazardCharacterisationsSummaryRecord() {
                        CompoundName = model.Substance.Name,
                        CompoundCode = model.Substance.Code,
                        EffectName = effect?.Name,
                        EffectCode = effect?.Code,
                        IsReferenceSubstance = model == referenceRecord,
                        HazardCharacterisation = model?.Value ?? double.NaN,
                        GeometricStandardDeviation = model?.GeometricStandardDeviation ?? double.NaN,
                        SystemHazardCharacterisation = model?.InternalHazardDose ?? double.NaN,
                        InternalRpf = internalRpf,
                        ExternalRpf = externalRpf,
                        MolecularMass = model?.Substance?.MolecularMass ?? double.NaN,
                        NominalInterSpeciesConversionFactor = model?.NominalInterSpeciesConversionFactor ?? double.NaN,
                        NominalIntraSpeciesConversionFactor = model?.NominalIntraSpeciesConversionFactor ?? double.NaN,
                        NominalKineticConversionFactor = model?.KineticConversionFactor ?? double.NaN,
                        AdditionalConversionFactor = model?.AdditionalConversionFactor ?? double.NaN,
                        MolBasedRpf = model?.MolBasedRpf ?? double.NaN,
                    };
                    return record;
                })
                .OrderByDescending(r => r.IsReferenceSubstance)
                .OrderBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ToList();

        }
    }
}
