using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        public IList<NonDietaryExposureSet> GetAllNonDietaryExposureSets() {
            if (_data.NonDietaryExposureSets == null) {
                LoadScope(SourceTableGroup.NonDietary);
                var nonDietarySurveys = new Dictionary<string, NonDietarySurvey>(StringComparer.OrdinalIgnoreCase);
                _data.NonDietaryExposureSets = new List<NonDietaryExposureSet>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.NonDietary);
                //if no data source specified: return immediately.
                if (rawDataSourceIds?.Any() ?? false) {

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
                                            NonDietaryExposureUnitString = r.GetString(RawNonDietarySurveys.NonDietaryIntakeUnit, fieldMap),
                                            ProportionZeros = proportionZero == null ? 0 : (double)proportionZero,
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
                                        var prop = new NonDietarySurveyProperty();
                                        if (_data.AllDietaryIndividualProperties.TryGetValue(propertyName, out IndividualProperty property)) {
                                            prop = new NonDietarySurveyProperty {
                                                IndividualProperty = property,
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
                                        fillNonDietaryExposureData(nonDietarySurveys[idSurvey],
                                            ndSets, string.Empty,
                                            idIndividual,
                                            substance,
                                            r.GetDouble(RawNonDietaryExposures.Dermal, fieldMap),
                                            r.GetDouble(RawNonDietaryExposures.Oral, fieldMap),
                                            r.GetDouble(RawNonDietaryExposures.Inhalation, fieldMap));
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
                                        fillNonDietaryExposureData(nonDietarySurveys[idSurvey],
                                            ndSets, idSet,
                                            idIndividual,
                                            substance,
                                            r.GetDouble(RawNonDietaryExposuresUncertain.Dermal, fieldMap),
                                            r.GetDouble(RawNonDietaryExposuresUncertain.Oral, fieldMap),
                                            r.GetDouble(RawNonDietaryExposuresUncertain.Inhalation, fieldMap));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return _data.NonDietaryExposureSets;
        }

        private void fillNonDietaryExposureData(NonDietarySurvey survey,
            IDictionary<string, NonDietaryExposureSet> ndSets,
            string idSet, string idIndividual, Compound compound,
            double dermal, double oral, double inhalation) {

            //an empty idSet variable denotes raw dietary exposures otherwise uncertainties
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
                NonDietarySetCode = set.Code,
                Inhalation = inhalation,
                IdIndividual = set.IndividualCode,
            };

            set.NonDietaryExposures.Add(ndExposure);
        }

        private static void writeNonDietaryDataToCsv(string tempFolder, IEnumerable<NonDietaryExposureSet> sets) {
            if (!sets?.Any() ?? true) {
                return;
            }

            var tdsv = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.NonDietarySurveys);
            var dtsv = tdsv.CreateDataTable();
            var tdsp = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.NonDietarySurveyProperties);
            var dtsp = tdsp.CreateDataTable();
            var tde = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.NonDietaryExposures);
            var dte = tde.CreateDataTable();
            var tdu = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.NonDietaryExposuresUncertain);
            var dtu = tdu.CreateDataTable();

            var surveyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var set in sets) {
                var survey = set.NonDietarySurvey;
                if (survey != null && !surveyIds.Contains(survey.Code)) {
                    var rowsv = dtsv.NewRow();
                    rowsv.WriteNonEmptyString(RawNonDietarySurveys.IdNonDietarySurvey, survey.Code);
                    rowsv.WriteNonEmptyString(RawNonDietarySurveys.IdPopulation, survey.IdPopulation);
                    rowsv.WriteNonEmptyString(RawNonDietarySurveys.Description, survey.Description);
                    rowsv.WriteNonEmptyString(RawNonDietarySurveys.Location, survey.Location);
                    rowsv.WriteNonEmptyString(RawNonDietarySurveys.NonDietaryIntakeUnit, survey.NonDietaryExposureUnitString);
                    rowsv.WriteNonNaNDouble(RawNonDietarySurveys.ProportionZeros, survey.ProportionZeros);
                    dtsv.Rows.Add(rowsv);
                    surveyIds.Add(survey.Code);

                    var svProperties = survey.NonDietarySurveyProperties;
                    if (svProperties?.Any() ?? false) {
                        foreach (var svProperty in svProperties) {
                            var rowsp = dtsp.NewRow();
                            rowsp.WriteNonEmptyString(RawNonDietarySurveyProperties.IdNonDietarySurvey, survey.Code);
                            rowsp.WriteNonEmptyString(RawNonDietarySurveyProperties.IndividualPropertyName, svProperty.IndividualProperty.Name);
                            rowsp.WriteNonEmptyString(RawNonDietarySurveyProperties.IndividualPropertyTextValue, svProperty.IndividualPropertyTextValue);
                            rowsp.WriteNonNullDouble(RawNonDietarySurveyProperties.IndividualPropertyDoubleValueMin, svProperty.IndividualPropertyDoubleValueMin);
                            rowsp.WriteNonNullDouble(RawNonDietarySurveyProperties.IndividualPropertyDoubleValueMax, svProperty.IndividualPropertyDoubleValueMax);
                            dtsp.Rows.Add(rowsp);
                        }
                    }
                }

                var exposures = set.NonDietaryExposures;
                foreach (var exp in exposures) {
                    //use the nondietary exposure set's ID to find out whether it's
                    //an 'uncertainty' exposure, these have a non empty setCode
                    if (string.IsNullOrEmpty(set.Code)) {
                        var rowe = dte.NewRow();
                        rowe.WriteNonEmptyString(RawNonDietaryExposures.IdNonDietarySurvey, survey.Code);
                        rowe.WriteNonEmptyString(RawNonDietaryExposures.IdIndividual, set.IndividualCode);
                        rowe.WriteNonEmptyString(RawNonDietaryExposures.IdCompound, exp.Compound.Code);
                        rowe.WriteNonNaNDouble(RawNonDietaryExposures.Dermal, exp.Dermal);
                        rowe.WriteNonNaNDouble(RawNonDietaryExposures.Oral, exp.Oral);
                        rowe.WriteNonNaNDouble(RawNonDietaryExposures.Inhalation, exp.Inhalation);
                        dte.Rows.Add(dte);
                    } else {
                        var rowu = dtu.NewRow();
                        rowu.WriteNonEmptyString(RawNonDietaryExposuresUncertain.Id, set.Code);
                        rowu.WriteNonEmptyString(RawNonDietaryExposuresUncertain.IdNonDietarySurvey, survey.Code);
                        rowu.WriteNonEmptyString(RawNonDietaryExposuresUncertain.IdIndividual, set.IndividualCode);
                        rowu.WriteNonEmptyString(RawNonDietaryExposuresUncertain.IdCompound, exp.Compound.Code);
                        rowu.WriteNonNaNDouble(RawNonDietaryExposuresUncertain.Dermal, exp.Dermal);
                        rowu.WriteNonNaNDouble(RawNonDietaryExposuresUncertain.Oral, exp.Oral);
                        rowu.WriteNonNaNDouble(RawNonDietaryExposuresUncertain.Inhalation, exp.Inhalation);
                        dtu.Rows.Add(dtu);
                    }
                }
            }
            writeToCsv(tempFolder, tdsv, dtsv);
            writeToCsv(tempFolder, tdsp, dtsp);
            writeToCsv(tempFolder, tde, dte);
            writeToCsv(tempFolder, tdu, dtu);
        }
    }
}
