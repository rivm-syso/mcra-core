using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw.Objects.RawTableGroups;
using MCRA.General.TableDefinitions.RawTableObjects;
using MCRA.General;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public sealed class RawHazardCharacterisationsDataConverter : RawTableGroupDataConverterBase<RawHazardCharacterisationsData> {

        public override RawHazardCharacterisationsData FromCompiledData(CompiledData data) {
            throw new NotImplementedException();
        }

        public RawHazardCharacterisationsData ToRaw(IEnumerable<HazardCharacterisation> hazardCharacterisations) {
            var result = new RawHazardCharacterisationsData();
            foreach (var hazardCharacterisation in hazardCharacterisations) {
                var rawRecord = new RawHazardCharacterisation() {
                    idHazardCharacterisation = hazardCharacterisation.Code,
                    idEffect = hazardCharacterisation.Effect?.Code,
                    idSubstance = hazardCharacterisation.Substance.Code,
                    DoseUnit = hazardCharacterisation.DoseUnit,
                    CombinedAssessmentFactor = hazardCharacterisation.CombinedAssessmentFactor,
                    ExposureRoute = hazardCharacterisation.ExposureRoute != ExposureRoute.Undefined
                        ? hazardCharacterisation.ExposureRoute
                        : null,
                    ExposureType = hazardCharacterisation.ExposureType,
                    HazardCharacterisationType = hazardCharacterisation.HazardCharacterisationType,
                    idPointOfDeparture = hazardCharacterisation.IdPointOfDeparture,
                    idPopulationType = hazardCharacterisation.PopulationType,
                    IsCriticalEffect = hazardCharacterisation.IsCriticalEffect,
                    Qualifier = Enum.TryParse<ValueQualifier>(hazardCharacterisation.Qualifier, true, out var vq) ? vq : null,
                    TargetLevel = hazardCharacterisation.TargetLevel,
                    TargetOrgan = hazardCharacterisation.BiologicalMatrix,
                    ExpressionType = hazardCharacterisation.ExpressionType,
                    Value = hazardCharacterisation.Value,
                    PublicationAuthors = hazardCharacterisation.PublicationAuthors,
                    PublicationTitle = hazardCharacterisation.PublicationTitle,
                    PublicationUri = hazardCharacterisation.PublicationUri,
                    PublicationYear = hazardCharacterisation.PublicationYear
                };
                result.HazardCharacterisations.Add(rawRecord);

                if (hazardCharacterisation?.HazardCharacterisationsUncertains?.Any() ?? false) {
                    foreach (var uncertainRecord in hazardCharacterisation.HazardCharacterisationsUncertains) {
                        var rawUncertainRecord = new RawHazardCharacterisationUncertain() {
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
