using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        public IList<NonDietaryExposureSet> GetAllNonDietaryExposureSets() {
            if (_data.NonDietaryExposureSets == null) {
                LoadScope(SourceTableGroup.NonDietary);
                var nonDietarySurveys = new Dictionary<string, NonDietarySurvey>(StringComparer.OrdinalIgnoreCase);
                _data.NonDietaryExposureSets = [];
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.NonDietary);
                //if no data source specified: return immediately.
                if (rawDataSourceIds?.Count > 0) {

                    //Prerequisites
                    GetAllCompounds();
                    var allIndividuals = GetAllIndividuals();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read non-dietary surveys
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawNonDietarySurveys>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSurvey = r.GetString(RawNonDietarySurveys.IdNonDietarySurvey, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.NonDietarySurveys, idSurvey);
                                    if (valid) {
                                        var proportionZero = r.GetDoubleOrNull(RawNonDietarySurveys.ProportionZeros, fieldMap);
                                        var survey = new NonDietarySurvey() {
                                            Code = idSurvey,
                                            Description = r.GetStringOrNull(RawNonDietarySurveys.Description, fieldMap),
                                            Name = r.GetStringOrNull(RawNonDietarySurveys.Name, fieldMap),
                                            Location = r.GetStringOrNull(RawNonDietarySurveys.Location, fieldMap),
                                            Date = r.GetDateTimeOrNull(RawNonDietarySurveys.Date, fieldMap),
                                            ExposureUnit = r.GetEnum<ExternalExposureUnit>(RawNonDietarySurveys.NonDietaryIntakeUnit, fieldMap),
                                            ProportionZeros = proportionZero ?? 0D,
                                            IdPopulation = r.GetStringOrNull(RawNonDietarySurveys.IdPopulation, fieldMap),
                                        };
                                        nonDietarySurveys.Add(idSurvey, survey);
                                    }
                                }
                            }
                        }

                        // Add items by code from the scope where no matched items were found in the source
                        foreach (var code in GetCodesInScope(ScopingType.NonDietarySurveys).Except(nonDietarySurveys.Keys, StringComparer.OrdinalIgnoreCase)) {
                            nonDietarySurveys[code] = new NonDietarySurvey { Code = code };
                        }

                        // Read non-dietary survey properties
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawNonDietarySurveyProperties>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSurvey = r.GetString(RawNonDietarySurveyProperties.IdNonDietarySurvey, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.NonDietarySurveys, idSurvey);
                                    if (valid) {
                                        var survey = nonDietarySurveys[idSurvey];
                                        var propertyName = r.GetString(RawNonDietarySurveyProperties.IndividualPropertyName, fieldMap);
                                        var prop = new NonDietarySurveyProperty {
                                            Name = propertyName,
                                            NonDietarySurvey = survey,
                                            IndividualPropertyDoubleValueMax = r.GetDoubleOrNull(RawNonDietarySurveyProperties.IndividualPropertyDoubleValueMax, fieldMap),
                                            IndividualPropertyDoubleValueMin = r.GetDoubleOrNull(RawNonDietarySurveyProperties.IndividualPropertyDoubleValueMin, fieldMap),
                                            IndividualPropertyTextValue = r.GetStringOrNull(RawNonDietarySurveyProperties.IndividualPropertyTextValue, fieldMap)
                                        };
                                        if (nonDietarySurveys.TryGetValue(idSurvey, out var ndSurvey)) {
                                            ndSurvey.NonDietarySurveyProperties.Add(prop);
                                        }
                                    }
                                }
                            }
                        }

                        // Read non-dietary exposures
                        var ndSets = new Dictionary<string, NonDietaryExposureSet>(StringComparer.OrdinalIgnoreCase);
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawNonDietaryExposures>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSurvey = r.GetString(RawNonDietaryExposures.IdNonDietarySurvey, fieldMap);
                                    var idSubstance = r.GetString(RawNonDietaryExposures.IdCompound, fieldMap);
                                    var idIndividual = r.GetStringOrNull(RawNonDietaryExposures.IdIndividual, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.NonDietarySurveys, idSurvey)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var substance = _data.GetOrAddSubstance(idSubstance);
                                        fillNonDietaryExposureData(
                                            nonDietarySurveys[idSurvey],
                                            ndSets,
                                            string.Empty,
                                            idIndividual,
                                            substance,
                                            r.GetDouble(RawNonDietaryExposures.Dermal, fieldMap),
                                            r.GetDouble(RawNonDietaryExposures.Oral, fieldMap),
                                            r.GetDouble(RawNonDietaryExposures.Inhalation, fieldMap)
                                        );
                                    }
                                }
                            }
                        }

                        // Read non-dietary exposure uncertainties
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawNonDietaryExposuresUncertain>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSurvey = r.GetString(RawNonDietaryExposuresUncertain.IdNonDietarySurvey, fieldMap);
                                    var idSubstance = r.GetString(RawNonDietaryExposuresUncertain.IdCompound, fieldMap);
                                    var idIndividual = r.GetStringOrNull(RawNonDietaryExposuresUncertain.IdIndividual, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.NonDietarySurveys, idSurvey)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var substance = _data.GetOrAddSubstance(idSubstance);
                                        var idSet = r.GetString(RawNonDietaryExposuresUncertain.Id, fieldMap);
                                        fillNonDietaryExposureData(
                                            nonDietarySurveys[idSurvey],
                                            ndSets,
                                            idSet,
                                            idIndividual,
                                            substance,
                                            r.GetDouble(RawNonDietaryExposuresUncertain.Dermal, fieldMap),
                                            r.GetDouble(RawNonDietaryExposuresUncertain.Oral, fieldMap),
                                            r.GetDouble(RawNonDietaryExposuresUncertain.Inhalation, fieldMap)
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return _data.NonDietaryExposureSets;
        }

        private void fillNonDietaryExposureData(
            NonDietarySurvey survey,
            IDictionary<string, NonDietaryExposureSet> ndSets,
            string idSet,
            string idIndividual,
            Compound compound,
            double dermal,
            double oral,
            double inhalation
        ) {
            // An empty idSet variable denotes raw dietary exposures otherwise uncertainties
            var isExposure = string.IsNullOrEmpty(idSet);
            var setKey = idSet + _sep + survey.Code + _sep + idIndividual;

            if (!ndSets.TryGetValue(setKey, out NonDietaryExposureSet set) && (isExposure || idIndividual != null)) {
                ndSets[setKey] = set = new NonDietaryExposureSet {
                    Code = idSet,
                    IndividualCode = idIndividual,
                    NonDietarySurvey = survey
                };
                _data.NonDietaryExposureSets.Add(set);
            }

            if (set == null) {
                return;
            }
            var ndExposure = new NonDietaryExposure {
                Compound = compound,
                Dermal = dermal,
                Oral = oral,
                Inhalation = inhalation,
                IdIndividual = set.IndividualCode,
            };

            set.NonDietaryExposures.Add(ndExposure);
        }
    }
}
