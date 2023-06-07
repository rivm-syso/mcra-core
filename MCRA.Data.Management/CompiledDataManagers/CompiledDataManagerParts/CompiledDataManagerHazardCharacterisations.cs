using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        public ICollection<HazardCharacterisation> GetAllHazardCharacterisations() {
            if (_data.AllHazardCharacterisations == null) {
                LoadScope(SourceTableGroup.HazardCharacterisations);
                var hazardCharacterisations = new List<HazardCharacterisation>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.HazardCharacterisations);

                //if no data source specified: return immediately.
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllCompounds();
                    GetAllEffects();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read hazard doses
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHazardCharacterisations>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idHazardCharacterisation = r.GetString(RawHazardCharacterisations.IdHazardCharacterisation, fieldMap);
                                        var idEffect = r.GetStringOrNull(RawHazardCharacterisations.IdEffect, fieldMap);
                                        var idSubstance = r.GetString(RawHazardCharacterisations.IdSubstance, fieldMap);
                                        var valid = (string.IsNullOrEmpty(idEffect) || CheckLinkSelected(ScopingType.Effects, idEffect))
                                                  & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                        if (valid) {

                                            var targetLevelString = r.GetStringOrNull(RawHazardCharacterisations.TargetLevel, fieldMap);
                                            var targetLevel = !string.IsNullOrEmpty(targetLevelString)
                                                ? TargetLevelTypeConverter.FromString(targetLevelString)
                                                : TargetLevelType.External;

                                            var exposureRouteTypeString = r.GetStringOrNull(RawHazardCharacterisations.ExposureRoute, fieldMap);
                                            ExposureRouteType exposureRoute;
                                            if (!string.IsNullOrEmpty(exposureRouteTypeString)) {
                                                exposureRoute = ExposureRouteTypeConverter.FromString(exposureRouteTypeString);
                                            } else {
                                                exposureRoute = targetLevel == TargetLevelType.External
                                                    ? ExposureRouteType.Dietary
                                                    : ExposureRouteType.AtTarget;
                                            }

                                            var record = new HazardCharacterisation() {
                                                Code = idHazardCharacterisation,
                                                Effect = !string.IsNullOrEmpty(idEffect) ? _data.GetOrAddEffect(idEffect) : null,
                                                Substance = _data.GetOrAddSubstance(idSubstance),
                                                PopulationType = r.GetStringOrNull(RawHazardCharacterisations.IdPopulationType, fieldMap),
                                                ExposureTypeString = r.GetStringOrNull(RawHazardCharacterisations.ExposureType, fieldMap),
                                                ExposureRoute = exposureRoute,
                                                TargetLevel = targetLevel,
                                                TargetOrganString = r.GetStringOrNull(RawHazardCharacterisations.TargetOrgan, fieldMap),
                                                IsCriticalEffect = r.GetBooleanOrNull(RawHazardCharacterisations.IsCriticalEffect, fieldMap) ?? false,
                                                HazardCharacterisationTypeString = r.GetStringOrNull(RawHazardCharacterisations.HazardCharacterisationType, fieldMap),
                                                Qualifier = r.GetStringOrNull(RawHazardCharacterisations.Qualifier, fieldMap),
                                                Value = r.GetDouble(RawHazardCharacterisations.Value, fieldMap),
                                                DoseUnitString = r.GetStringOrNull(RawHazardCharacterisations.DoseUnit, fieldMap),
                                                IdPointOfDeparture = r.GetStringOrNull(RawHazardCharacterisations.IdPointOfDeparture, fieldMap),
                                                CombinedAssessmentFactor = r.GetDoubleOrNull(RawHazardCharacterisations.CombinedAssessmentFactor, fieldMap),
                                                PublicationAuthors = r.GetStringOrNull(RawHazardCharacterisations.PublicationAuthors, fieldMap),
                                                PublicationTitle = r.GetStringOrNull(RawHazardCharacterisations.PublicationTitle, fieldMap),
                                                PublicationUri = r.GetStringOrNull(RawHazardCharacterisations.PublicationUri, fieldMap),
                                                PublicationYear = r.GetIntOrNull(RawHazardCharacterisations.PublicationYear, fieldMap),
                                                Name = r.GetStringOrNull(RawHazardCharacterisations.Name, fieldMap),
                                                Description = r.GetStringOrNull(RawHazardCharacterisations.Description, fieldMap),
                                            };
                                            hazardCharacterisations.Add(record);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllHazardCharacterisations = hazardCharacterisations;
            }

            return _data.AllHazardCharacterisations;
        }

        private static void writeHazardCharacterisationsToCsv(string tempFolder, IEnumerable<HazardCharacterisation> hazardCharacterisations) {
            if (!hazardCharacterisations?.Any() ?? true) {
                return;
            }

            var mapper = new RawHazardCharacterisationsDataConverter();
            var rawData = mapper.ToRaw(hazardCharacterisations);
            var writer = new CsvRawDataWriter(tempFolder);
            writer.Set(rawData);
            writer.Store();
        }
    }
}
