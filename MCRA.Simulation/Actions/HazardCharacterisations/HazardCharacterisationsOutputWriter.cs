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
            var rawDataConverter = new RawHazardCharacterisationsDataConverter();
            var hazardCharacterisationModels = data.HazardCharacterisationModelsCollections
                .SelectMany(c => c.HazardCharacterisationModels.Select(r => r.Value))
                .ToList();

            var records = hazardCharacterisationModels
                .Select(r => new HazardCharacterisation() {
                    Code = r.Code,
                    Substance = r.Substance,
                    Effect = data.SelectedEffect,
                    Value = r.Value,
                    DoseUnitString = r.DoseUnit.ToString(),
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
