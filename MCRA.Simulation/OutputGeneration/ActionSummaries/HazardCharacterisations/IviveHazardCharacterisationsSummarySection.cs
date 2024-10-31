using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IviveHazardCharacterisationsSummarySection : SummarySection {

        public List<IviveHazardCharacterisationsSummaryRecord> Records { get; set; }
        public TargetLevelType TargetLevelType { get; set; }
        public void Summarize(
            Effect effect,
            Compound referenceSubstance,
            ICollection<IviveHazardCharacterisation> iviveTargetDoseModels,
            TargetLevelType targetDoseLevelType
        ) {
            TargetLevelType = targetDoseLevelType;
            var referenceRecord = iviveTargetDoseModels.First(r => r.Substance == referenceSubstance);
            Records = iviveTargetDoseModels
                .Select(model => {
                    var internalRpf = referenceRecord.InternalHazardDose / model.InternalHazardDose;
                    var rpfSubstanceMolRatio = referenceRecord.Substance.MolecularMass / model.Substance.MolecularMass;
                    var massBasedRpf = referenceRecord.DoseUnit.SubstanceAmountUnit.IsInMoles()
                        ? internalRpf
                        : rpfSubstanceMolRatio * internalRpf;
                    var molBasedRpf = referenceRecord.DoseUnit.SubstanceAmountUnit.IsInMoles()
                        ? internalRpf * rpfSubstanceMolRatio
                        : internalRpf;
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
                        Unit = model.DoseUnit.ToString(),
                        ExposureRoute = model.TargetUnit.ExposureRoute != ExposureRoute.Undefined
                            ? model.TargetUnit.ExposureRoute.GetDisplayName() : null,
                        BiologicalMatrix = model.TargetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                            ? model.TargetUnit.BiologicalMatrix.GetDisplayName() : null,
                        GeometricStandardDeviation = model?.GeometricStandardDeviation ?? double.NaN,
                        TestSystemHazardCharacterisation = model?.InternalHazardDose ?? double.NaN,
                        InternalMassBasedRpf = massBasedRpf,
                        ExternalRpf = externalRpf,
                        MolecularMass = model?.Substance?.MolecularMass ?? double.NaN,
                        NominalInterSpeciesConversionFactor = model?.NominalInterSpeciesConversionFactor ?? double.NaN,
                        NominalIntraSpeciesConversionFactor = model?.NominalIntraSpeciesConversionFactor ?? double.NaN,
                        NominalKineticConversionFactor = model?.KineticConversionFactor ?? double.NaN,
                        AdditionalConversionFactor = model?.AdditionalConversionFactor ?? double.NaN,
                        InternalMolBasedRpf = molBasedRpf,
                        TestSystemDoseUnit = model.InternalTargetUnit.ExposureUnit.GetShortDisplayName(),
                        TestSystemBiologicalMatrix = model.InternalTargetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                            ? model.InternalTargetUnit.BiologicalMatrix.GetDisplayName() : null,
                    };
                    return record;
                })
                .OrderByDescending(r => r.IsReferenceSubstance)
                .OrderBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ToList();

        }
    }
}
