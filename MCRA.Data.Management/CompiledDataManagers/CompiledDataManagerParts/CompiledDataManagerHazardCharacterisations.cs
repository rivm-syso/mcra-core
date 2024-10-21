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
                                                  & CheckLinkSelected(ScopingType.Compounds, idSubstance)
                                                  & CheckLinkSelected(ScopingType.HazardCharacterisations, idHazardCharacterisation);
                                        if (valid) {

                                            var targetLevelString = r.GetStringOrNull(RawHazardCharacterisations.TargetLevel, fieldMap);
                                            var targetLevel = !string.IsNullOrEmpty(targetLevelString)
                                                ? TargetLevelTypeConverter.FromString(targetLevelString)
                                                : TargetLevelType.External;

                                            var exposureRouteTypeString = r.GetStringOrNull(RawHazardCharacterisations.ExposureRoute, fieldMap);
                                            ExposureRoute exposureRoute;
                                            if (!string.IsNullOrEmpty(exposureRouteTypeString)) {
                                                exposureRoute = ExposureRouteConverter.FromString(exposureRouteTypeString);
                                            } else {
                                                exposureRoute = targetLevel == TargetLevelType.External
                                                    ? ExposureRoute.Oral
                                                    : ExposureRoute.Undefined;
                                            }

                                            var exposureType = ExposureTypeConverter.FromString(
                                                r.GetStringOrNull(RawHazardCharacterisations.ExposureType, fieldMap),
                                                ExposureType.Chronic
                                            );
                                            var hazardCharacterisationType = HazardCharacterisationTypeConverter.FromString(
                                                r.GetStringOrNull(RawHazardCharacterisations.HazardCharacterisationType, fieldMap),
                                                HazardCharacterisationType.Unspecified
                                            );
                                            var doseUnit = DoseUnitConverter.FromString(
                                                r.GetStringOrNull(RawHazardCharacterisations.DoseUnit, fieldMap),
                                                DoseUnit.mgPerKgBWPerDay
                                            );
                                            var record = new HazardCharacterisation() {
                                                Code = idHazardCharacterisation,
                                                Effect = !string.IsNullOrEmpty(idEffect) ? _data.GetOrAddEffect(idEffect) : null,
                                                Substance = _data.GetOrAddSubstance(idSubstance),
                                                PopulationType = r.GetStringOrNull(RawHazardCharacterisations.IdPopulationType, fieldMap),
                                                ExposureType = exposureType,
                                                ExposureRoute = exposureRoute,
                                                TargetLevel = targetLevel,
                                                ExpressionType = r.GetEnum(RawHazardCharacterisations.ExpressionType, fieldMap, ExpressionType.None),
                                                BiologicalMatrix = r.GetEnum(
                                                    RawHazardCharacterisations.TargetOrgan,
                                                    fieldMap,
                                                    BiologicalMatrix.Undefined,
                                                    allowInvalidString: true
                                                ),
                                                IsCriticalEffect = r.GetBooleanOrNull(RawHazardCharacterisations.IsCriticalEffect, fieldMap) ?? false,
                                                HazardCharacterisationType = hazardCharacterisationType,
                                                Qualifier = r.GetStringOrNull(RawHazardCharacterisations.Qualifier, fieldMap),
                                                Value = r.GetDouble(RawHazardCharacterisations.Value, fieldMap),
                                                DoseUnit = doseUnit,
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

                        // Create lookup based on combined keys
                        var lookup = hazardCharacterisations
                            .ToDictionary(r => (r.Code.ToLowerInvariant(), r.Substance.Code.ToLowerInvariant()));

                        // Read hazard characterisations uncertainties
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHazardCharacterisationsUncertain>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idHazardCharacterisation = r.GetString(RawHazardCharacterisationsUncertain.IdHazardCharacterisation, fieldMap);
                                        var idSubstance = r.GetString(RawHazardCharacterisationsUncertain.IdSubstance, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance)
                                            & CheckLinkSelected(ScopingType.HazardCharacterisations, idHazardCharacterisation);
                                        var idLookup = (idHazardCharacterisation.ToLowerInvariant(), idSubstance.ToLowerInvariant());
                                        if (valid && lookup.TryGetValue(idLookup, out var hazardCharacterisation)) {
                                            var recordUncertain = new HazardCharacterisationUncertain {
                                                IdHazardCharacterisation = idHazardCharacterisation,
                                                Substance = _data.GetOrAddSubstance(idSubstance),
                                                Value = r.GetDouble(RawHazardCharacterisationsUncertain.Value, fieldMap)
                                            };
                                            hazardCharacterisation.HazardCharacterisationsUncertains.Add(recordUncertain);
                                        }
                                    }
                                }
                            }
                        }

                        // Read hazard characterisations subgroups uncertainty sets
                        var uncertainties = new List<HCSubgroupsUncertain>();
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHCSubgroupsUncertain>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idHazardCharacterisation = r.GetString(RawHCSubgroupsUncertain.IdHazardCharacterisation, fieldMap);
                                        var idSubgroup = r.GetString(RawHCSubgroupsUncertain.IdSubgroup, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.HazardCharacterisations, idHazardCharacterisation);
                                        if (valid) {
                                            var record = new HCSubgroupsUncertain {
                                                IdHazardCharacterisation = idHazardCharacterisation,
                                                IdSubgroup = r.GetString(RawHCSubgroupsUncertain.IdSubgroup, fieldMap),
                                                IdUncertaintySet = r.GetString(RawHCSubgroupsUncertain.IdUncertaintySet, fieldMap),
                                                Value = r.GetDouble(RawHCSubgroupsUncertain.Value, fieldMap)
                                            };
                                            uncertainties.Add(record);
                                        }
                                    }
                                }
                            }
                        }

                        var uncertaintyLookup = uncertainties.ToLookup(c => (c.IdHazardCharacterisation, c.IdSubgroup));

                        // Read hazard characterisations subgroups
                        var hcSubgroups = new List<HCSubgroup>();
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHCSubgroups>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idHazardCharacterisation = r.GetString(RawHCSubgroups.IdHazardCharacterisation, fieldMap);
                                        var idSubstance = r.GetString(RawHCSubgroups.IdSubstance, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance)
                                            & CheckLinkSelected(ScopingType.HazardCharacterisations, idHazardCharacterisation);
                                        var idLookup = (idHazardCharacterisation.ToLowerInvariant(), idSubstance.ToLowerInvariant());
                                        if (valid && lookup.TryGetValue(idLookup, out var hazardCharacterisation)) {
                                            var record = new HCSubgroup {
                                                IdHazardCharacterisation = idHazardCharacterisation,
                                                IdSubgroup = r.GetString(RawHCSubgroups.IdSubgroup, fieldMap),
                                                Substance = _data.GetOrAddSubstance(idSubstance),
                                                AgeLower = r.GetDoubleOrNull(RawHCSubgroups.AgeLower, fieldMap),
                                                Gender = r.GetStringOrNull(RawHCSubgroups.Gender, fieldMap),
                                                Value = r.GetDouble(RawHCSubgroups.Value, fieldMap),
                                            };
                                            var sets = uncertaintyLookup?[(idHazardCharacterisation, record.IdSubgroup)].ToList();
                                            record.HCSubgroupsUncertains = sets.Any() ? sets : null;
                                            hazardCharacterisation.HCSubgroups.Add(record);
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
