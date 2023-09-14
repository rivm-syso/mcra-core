using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.HazardCharacterisations {
    public sealed class HazardCharacterisationsOutputWriter {
        public void WriteOutputData(
            ProjectDto project,
            ActionData data,
            IRawDataWriter rawDataWriter
        ) {
            var targetLevel = project.EffectSettings.TargetDoseLevelType;
            var exposureType = project.AssessmentSettings.ExposureType;
            var targetUnit = data.HazardCharacterisationsUnit;
            var rawDataConverter = new RawHazardCharacterisationsDataConverter();
            var records = data.HazardCharacterisationModels.Values
                .Select(r => new HazardCharacterisation() {
                    Code = r.Code,
                    Substance = r.Substance,
                    Effect = data.SelectedEffect,
                    Value = r.Value,
                    DoseUnitString = targetUnit.GetShortDisplayName(targetLevel == TargetLevelType.External ? TargetUnit.DisplayOption.AppendBiologicalMatrix : TargetUnit.DisplayOption.UnitOnly),
                    TargetLevel = targetLevel,
                    ExposureTypeString = exposureType.GetShortDisplayName(),
                    ExposureRoute = r.Target.ExposureRoute,
                    BiologicalMatrix = r.Target.BiologicalMatrix,
                    ExpressionType = r.Target.ExpressionType,
                    HazardCharacterisationTypeString = r.HazardCharacterisationType != HazardCharacterisationType.Unspecified
                        ? r.HazardCharacterisationType.ToString()
                        : null,
                    CombinedAssessmentFactor = !double.IsNaN(r.CombinedAssessmentFactor) ? r.CombinedAssessmentFactor : null,
                    IdPointOfDeparture = r.TestSystemHazardCharacterisation?.PoD?.Code,
                    PublicationAuthors = r.Reference?.PublicationAuthors,
                    PublicationTitle = r.Reference?.PublicationTitle,
                    PublicationUri = r.Reference?.PublicationUri,
                    PublicationYear = r.Reference?.PublicationYear
                })
                .ToList();
            if (records != null) {
                var rawData = rawDataConverter.ToRaw(records);
                rawDataWriter.Set(rawData);
            }
        }
    }
}
