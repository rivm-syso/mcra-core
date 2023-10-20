using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw.Objects.HazardCharacterisations;
using MCRA.General;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public sealed class RawHazardCharacterisationsDataConverter : RawTableGroupDataConverterBase<RawHazardCharacterisationsData> {

        public override RawHazardCharacterisationsData FromCompiledData(CompiledData data) {
            throw new NotImplementedException();
        }

        public RawHazardCharacterisationsData ToRaw(IEnumerable<HazardCharacterisation> hazardCharacterisations) {
            var result = new RawHazardCharacterisationsData();
            foreach (var hazardCharacterisation in hazardCharacterisations) {
                var rawRecord = new RawHazardCharacterisationRecord() {
                    idHazardCharacterisation = hazardCharacterisation.Code,
                    idEffect = hazardCharacterisation.Effect?.Code,
                    idSubstance = hazardCharacterisation.Substance.Code,
                    DoseUnit = hazardCharacterisation.DoseUnitString,
                    CombinedAssessmentFactor = hazardCharacterisation.CombinedAssessmentFactor,
                    ExposureRoute = hazardCharacterisation.ExposureRoute != ExposureRouteType.AtTarget
                        && hazardCharacterisation.ExposureRoute != ExposureRouteType.Undefined
                        ? hazardCharacterisation.ExposureRoute.ToString()
                        : null,
                    ExposureType = hazardCharacterisation.ExposureType.ToString(),
                    HazardCharacterisationType = hazardCharacterisation.HazardCharacterisationType != HazardCharacterisationType.Unspecified
                        ? hazardCharacterisation.HazardCharacterisationType.ToString()
                        : null,
                    idPointOfDeparture = hazardCharacterisation.IdPointOfDeparture,
                    idPopulationType = hazardCharacterisation.PopulationType,
                    IsCriticalEffect = hazardCharacterisation.IsCriticalEffect,
                    Qualifier = hazardCharacterisation.Qualifier,
                    TargetLevel = hazardCharacterisation.TargetLevel.ToString(),
                    TargetOrgan = hazardCharacterisation.BiologicalMatrix.ToString(),
                    ExpressionType = hazardCharacterisation.ExpressionType.ToString(),
                    Value = hazardCharacterisation.Value,
                    PublicationAuthors = hazardCharacterisation.PublicationAuthors,
                    PublicationTitle = hazardCharacterisation.PublicationTitle,
                    PublicationUri = hazardCharacterisation.PublicationUri,
                    PublicationYear = hazardCharacterisation.PublicationYear
                };
                result.HazardCharacterisations.Add(rawRecord);

                if (hazardCharacterisation?.HazardCharacterisationsUncertains?.Any() ?? false) {
                    foreach (var uncertainRecord in hazardCharacterisation.HazardCharacterisationsUncertains) {
                        var rawUncertainRecord = new RawHazardCharacterisationUncertainRecord() {
                            idHazardCharacterisation = hazardCharacterisation.Code,
                            idSubstance = uncertainRecord.Substance.Code,
                            Value = uncertainRecord.Value
                        };
                        result.HazardCharacterisationsUncertain.Add(rawUncertainRecord);
                    }
                }
            }
            return result;
        }
    }
}
