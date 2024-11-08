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
            foreach (var hc in hazardCharacterisations) {
                var rawRecord = new RawHazardCharacterisation() {
                    idHazardCharacterisation = hc.Code,
                    idEffect = hc.Effect?.Code,
                    idSubstance = hc.Substance.Code,
                    DoseUnit = hc.DoseUnit.ToString(),
                    CombinedAssessmentFactor = hc.CombinedAssessmentFactor,
                    ExposureRoute = hc.ExposureRoute != ExposureRoute.Undefined
                        ? hc.ExposureRoute.ToString()
                        : null,
                    ExposureType = hc.ExposureType.ToString(),
                    HazardCharacterisationType = hc.HazardCharacterisationType.ToString(),
                    idPointOfDeparture = hc.IdPointOfDeparture,
                    idPopulationType = hc.PopulationType,
                    IsCriticalEffect = hc.IsCriticalEffect,
                    Qualifier = hc.Qualifier,
                    TargetLevel = hc.TargetLevel.ToString(),
                    TargetOrgan = hc.BiologicalMatrix.ToString(),
                    ExpressionType = hc.ExpressionType.ToString(),
                    Value = hc.Value,
                    PublicationAuthors = hc.PublicationAuthors,
                    PublicationTitle = hc.PublicationTitle,
                    PublicationUri = hc.PublicationUri,
                    PublicationYear = hc.PublicationYear
                };
                result.HazardCharacterisations.Add(rawRecord);

                if (hc.HazardCharacterisationsUncertains?.Count > 0) {
                    foreach (var uncertainRecord in hc.HazardCharacterisationsUncertains) {
                        var rawUncertainRecord = new RawHazardCharacterisationUncertain() {
                            idHazardCharacterisation = hc.Code,
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
