using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.HazardCharacterisations {
    public sealed class HazardCharacterisationsOutputWriter {
        public void WriteOutputData(
            HazardCharacterisationsModuleConfig config,
            ActionData data,
            IRawDataWriter rawDataWriter
        ) {
            var targetLevel = config.TargetDoseLevelType;
            var exposureType = config.ExposureType;
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
                    DoseUnit = DoseUnitConverter.FromString(r.DoseUnit.ToString()),
                    TargetLevel = targetLevel,
                    ExposureType = exposureType,
                    ExposureRoute = r.Target.ExposureRoute,
                    BiologicalMatrix = r.Target.BiologicalMatrix,
                    ExpressionType = r.Target.ExpressionType,
                    HazardCharacterisationType = r.HazardCharacterisationType,
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
