using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// GetAllDoseResponseExperiments
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, DoseResponseExperiment> GetAllDoseResponseExperiments() {
            if (_data.AllDoseResponseExperiments == null) {
                LoadScope(SourceTableGroup.DoseResponseData);
                var allExperiments = new Dictionary<string, DoseResponseExperiment>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DoseResponseData);
                if (rawDataSourceIds?.Any() ?? false) {

                    GetAllResponses();
                    GetAllCompounds();

                    // Read all dose response experiments
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDoseResponseExperiments>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idExperiment = r.GetString(RawDoseResponseExperiments.IdExperiment, fieldMap);
                                    var substanceCodes = r.GetString(RawDoseResponseExperiments.Substances, fieldMap).Split(',').Select(c => c.Trim()).ToArray();
                                    var responseCodes = r.GetString(RawDoseResponseExperiments.Responses, fieldMap).Split(',').Select(c => c.Trim()).ToArray();
                                    var valid = IsCodeSelected(ScopingType.DoseResponseExperiments, idExperiment)
                                              & CheckLinkSelected(ScopingType.Compounds, substanceCodes)
                                              & CheckLinkSelected(ScopingType.Responses, true, responseCodes);

                                    if (valid) {
                                        var substances = substanceCodes.Select(code => _data.GetOrAddSubstance(code));
                                        var responses = responseCodes
                                            .Where(code => _data.AllResponses.ContainsKey(code))
                                            .Select(code => _data.AllResponses[code]);

                                        // Read covariates
                                        var covariatesString = r.GetStringOrNull(RawDoseResponseExperiments.Covariates, fieldMap);
                                        var covariates = (!string.IsNullOrEmpty(covariatesString)) ?
                                            covariatesString.Split(',').Select(c => c.Trim()).ToList() : new List<string>();

                                        // Read design factors
                                        var designFactorsString = r.GetStringOrNull(RawDoseResponseExperiments.ExperimentalUnit, fieldMap);
                                        var designFactors = !string.IsNullOrEmpty(designFactorsString) ?
                                            designFactorsString.Split(':').Select(c => c.Trim()).ToList() : new List<string>();

                                        allExperiments.Add(idExperiment,
                                            new DoseResponseExperiment() {
                                                Code = idExperiment,
                                                Name = r.GetStringOrNull(RawDoseResponseExperiments.Name, fieldMap),
                                                Description = r.GetStringOrNull(RawDoseResponseExperiments.Description, fieldMap),
                                                Date = r.GetDateTimeOrNull(RawDoseResponseExperiments.Date, fieldMap),
                                                DoseRoute = r.GetStringOrNull(RawDoseResponseExperiments.DoseRoute, fieldMap),
                                                DoseUnit = r.GetEnum<DoseUnit>(RawDoseResponseExperiments.DoseUnit, fieldMap),
                                                TimeUnit = r.GetStringOrNull(RawDoseResponseExperiments.TimeUnit, fieldMap),
                                                Covariates = covariates,
                                                Reference = r.GetStringOrNull(RawDoseResponseExperiments.Reference, fieldMap),
                                                Substances = substances.ToList(),
                                                Time = r.GetStringOrNull(RawDoseResponseExperiments.Time, fieldMap),
                                                ExperimentalUnits = [],
                                                Responses = responses.ToList(),
                                                Design = designFactors,
                                            });
                                    }
                                }
                            }
                        }

                        // Read all dose response measurements
                        var experimentalUnits = new Dictionary<string, ExperimentalUnit>(StringComparer.OrdinalIgnoreCase);
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDoseResponseExperimentMeasurements>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idExperiment = r.GetString(RawDoseResponseExperimentMeasurements.IdExperiment, fieldMap);
                                    var idResponse = r.GetString(RawDoseResponseExperimentMeasurements.IdResponse, fieldMap);

                                    var valid = CheckLinkSelected(ScopingType.DoseResponseExperiments, idExperiment)
                                              & CheckLinkSelected(ScopingType.Responses, idResponse);

                                    if (valid) {
                                        var idExperimentalUnit = r.GetString(RawDoseResponseExperimentMeasurements.IdExperimentalUnit, fieldMap);
                                        var measurement = new DoseResponseExperimentMeasurement() {
                                            idResponse = idResponse,
                                            IdExperiment = idExperiment,
                                            IdExperimentalUnit = idExperimentalUnit,
                                            ResponseValue = r.GetDouble(RawDoseResponseExperimentMeasurements.ResponseValue, fieldMap),
                                            ResponseSD = r.GetDoubleOrNull(RawDoseResponseExperimentMeasurements.ResponseSD, fieldMap),
                                            ResponseCV = r.GetDoubleOrNull(RawDoseResponseExperimentMeasurements.ResponseCV, fieldMap),
                                            ResponseN = r.GetDoubleOrNull(RawDoseResponseExperimentMeasurements.ResponseN, fieldMap),
                                            ResponseUncertaintyUpper = r.GetDoubleOrNull(RawDoseResponseExperimentMeasurements.ResponseUncertaintyUpper, fieldMap),
                                        };

                                        var keyExperimentUnit = string.Join("\a", idExperiment, idExperimentalUnit);
                                        if (!experimentalUnits.TryGetValue(keyExperimentUnit, out ExperimentalUnit experimentalUnit)) {
                                            experimentalUnit = new ExperimentalUnit() {
                                                Code = idExperimentalUnit,
                                                Doses = new Dictionary<Compound, double>(),
                                                Responses = new Dictionary<Response, DoseResponseExperimentMeasurement>(),
                                                Covariates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                                                DesignFactors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                                            };
                                            experimentalUnits.Add(keyExperimentUnit, experimentalUnit);
                                        }
                                        experimentalUnit.Responses.Add(_data.AllResponses[idResponse], measurement);
                                    }
                                }
                            }
                        }

                        // Read all dose response doses
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDoseResponseExperimentDoses>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idExperiment = r.GetString(RawDoseResponseExperimentDoses.IdExperiment, fieldMap);
                                    var idSubstance = r.GetString(RawDoseResponseExperimentDoses.IdSubstance, fieldMap);

                                    var valid = CheckLinkSelected(ScopingType.DoseResponseExperiments, idExperiment)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        // Get experimental unit
                                        var idExperimentalUnit = r.GetString(RawDoseResponseExperimentDoses.IdExperimentalUnit, fieldMap);
                                        var keyExperimentUnit = string.Join("\a", idExperiment, idExperimentalUnit);
                                        if (!experimentalUnits.TryGetValue(keyExperimentUnit, out ExperimentalUnit experimentalUnit)) {
                                            experimentalUnit = new ExperimentalUnit() {
                                                Code = idExperimentalUnit,
                                                Doses = new Dictionary<Compound, double>(),
                                                Responses = new Dictionary<Response, DoseResponseExperimentMeasurement>(),
                                                Covariates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                                                DesignFactors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                                            };
                                            experimentalUnits.Add(keyExperimentUnit, experimentalUnit);
                                        }
                                        experimentalUnit.Doses.Add(_data.GetOrAddSubstance(idSubstance), r.GetFloat(RawDoseResponseExperimentDoses.Dose, fieldMap));
                                        experimentalUnit.Times = r.GetDoubleOrNull(RawDoseResponseExperimentDoses.Time, fieldMap);
                                    }
                                }
                            }
                        }

                        // Read all dose response experimental properties
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExperimentalUnitProperties>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {

                                    // Get experiment
                                    var idExperiment = r.GetString(RawExperimentalUnitProperties.IdExperiment, fieldMap);
                                    var idExperimentalUnit = r.GetString(RawExperimentalUnitProperties.IdExperimentalUnit, fieldMap);
                                    var keyExperimentUnit = string.Join("\a", idExperiment, idExperimentalUnit);

                                    var valid = CheckLinkSelected(ScopingType.DoseResponseExperiments, idExperiment)
                                              & experimentalUnits.ContainsKey(keyExperimentUnit);
                                    if (!valid) {
                                        continue;
                                    }

                                    var experiment = allExperiments[idExperiment];
                                    var experimentalUnit = experimentalUnits[keyExperimentUnit];

                                    var propertyName = r.GetStringOrNull(RawExperimentalUnitProperties.PropertyName, fieldMap);
                                    // Get covariate
                                    var covariate = experiment.Covariates.FirstOrDefault(c => c.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                                    if (covariate != null) {
                                        experimentalUnit.Covariates.Add(covariate, r.GetStringOrNull(RawExperimentalUnitProperties.Value, fieldMap));
                                    }

                                    // Get design factor
                                    var designFactor = experiment.Design.FirstOrDefault(c => c.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                                    if (designFactor != null) {
                                        experimentalUnit.DesignFactors.Add(designFactor, r.GetStringOrNull(RawExperimentalUnitProperties.Value, fieldMap));
                                    }
                                }
                            }
                        }
                        foreach (var experiment in allExperiments.Values) {
                            experiment.ExperimentalUnits = experimentalUnits
                                .Where(c => c.Key.Split('\a')[0] == experiment.Code)
                                .Select(c => c.Value)
                                .ToList();
                        }
                    }
                }
                _data.AllDoseResponseExperiments = allExperiments;
            }
            return _data.AllDoseResponseExperiments;
        }

        private static void writeDoseResponseDataToCsv(string tempFolder, IEnumerable<DoseResponseExperiment> experiments) {
            if (!experiments?.Any() ?? true) {
                return;
            }

            var tdx = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.DoseResponseExperiments);
            var dtx = tdx.CreateDataTable();
            var tdm = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.DoseResponseExperimentMeasurements);
            var dtm = tdm.CreateDataTable();
            var tdd = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.DoseResponseExperimentDoses);
            var dtd = tdd.CreateDataTable();
            var tdp = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ExperimentalUnitProperties);
            var dtp = tdp.CreateDataTable();

            foreach (var exp in experiments) {
                var rowx = dtx.NewRow();

                rowx.WriteNonEmptyString(RawDoseResponseExperiments.IdExperiment, exp.Code);
                rowx.WriteNonEmptyString(RawDoseResponseExperiments.Name, exp.Name);
                rowx.WriteNonEmptyString(RawDoseResponseExperiments.Description, exp.Description);
                rowx.WriteNonNullDateTime(RawDoseResponseExperiments.Date, exp.Date);
                rowx.WriteNonEmptyString(RawDoseResponseExperiments.DoseRoute, exp.DoseRoute);
                rowx.WriteNonEmptyString(RawDoseResponseExperiments.DoseUnit, exp.DoseUnit.ToString());
                rowx.WriteNonEmptyString(RawDoseResponseExperiments.TimeUnit, exp.TimeUnit);
                rowx.WriteNonEmptyString(
                    RawDoseResponseExperiments.Covariates,
                    (exp.Covariates?.Any() ?? false)
                        ? string.Join(",", exp.Covariates)
                        : null);
                rowx.WriteNonEmptyString(
                    RawDoseResponseExperiments.Substances,
                    (exp.Substances?.Any() ?? false)
                        ? string.Join(",", exp.Substances.Select(s => s.Code))
                        : null);
                rowx.WriteNonEmptyString(RawDoseResponseExperiments.Time, exp.Time);
                rowx.WriteNonEmptyString(
                    RawDoseResponseExperiments.ExperimentalUnit,
                    (exp.ExperimentalUnits?.Any() ?? false)
                        ? string.Join(":", exp.ExperimentalUnits.Select(s => s.Code))
                        : null);

                dtx.Rows.Add(rowx);
                if (exp.ExperimentalUnits != null) {
                    foreach (var r in exp.ExperimentalUnits.SelectMany(u => u.Responses.Values)) {
                        var rowm = dtm.NewRow();

                        rowm.WriteNonEmptyString(RawDoseResponseExperimentMeasurements.IdResponse, r.idResponse);
                        rowm.WriteNonEmptyString(RawDoseResponseExperimentMeasurements.IdExperiment, r.IdExperiment);
                        rowm.WriteNonEmptyString(RawDoseResponseExperimentMeasurements.IdExperimentalUnit, r.IdExperimentalUnit);
                        rowm.WriteNonNaNDouble(RawDoseResponseExperimentMeasurements.ResponseValue, r.ResponseValue);
                        rowm.WriteNonNullDouble(RawDoseResponseExperimentMeasurements.ResponseSD, r.ResponseSD);
                        rowm.WriteNonNullDouble(RawDoseResponseExperimentMeasurements.ResponseCV, r.ResponseCV);
                        rowm.WriteNonNullDouble(RawDoseResponseExperimentMeasurements.ResponseN, r.ResponseN);
                        rowm.WriteNonNullDouble(RawDoseResponseExperimentMeasurements.ResponseUncertaintyUpper, r.ResponseUncertaintyUpper);
                        rowm.WriteNonNaNDouble(RawDoseResponseExperimentMeasurements.Time, r.Time);

                        dtm.Rows.Add(rowm);
                    }

                    foreach (var u in exp.ExperimentalUnits) {
                        foreach (var d in u.Doses) {
                            var rowd = dtd.NewRow();

                            rowd.WriteNonEmptyString(RawDoseResponseExperimentDoses.IdExperiment, exp.Code);
                            rowd.WriteNonEmptyString(RawDoseResponseExperimentDoses.IdExperimentalUnit, u.Code);
                            rowd.WriteNonEmptyString(RawDoseResponseExperimentDoses.IdSubstance, d.Key.Code);
                            rowd.WriteNonNaNDouble(RawDoseResponseExperimentDoses.Dose, d.Value);
                            rowd.WriteNonNullDouble(RawDoseResponseExperimentDoses.Time, u.Times);

                            dtd.Rows.Add(rowd);
                        }

                        var props = new HashSet<string>();
                        foreach (var cv in u.Covariates) {
                            if (!props.Contains(cv.Key)) {
                                var rowp = dtp.NewRow();
                                rowp.WriteNonEmptyString(RawExperimentalUnitProperties.IdExperiment, exp.Code);
                                rowp.WriteNonEmptyString(RawExperimentalUnitProperties.IdExperimentalUnit, u.Code);
                                rowp.WriteNonEmptyString(RawExperimentalUnitProperties.PropertyName, cv.Key);
                                rowp.WriteNonEmptyString(RawExperimentalUnitProperties.Value, cv.Value);
                                dtp.Rows.Add(rowp);
                                props.Add(cv.Key);
                            }
                        }
                        foreach (var cv in u.DesignFactors) {
                            if (!props.Contains(cv.Key)) {
                                var rowp = dtp.NewRow();
                                rowp.WriteNonEmptyString(RawExperimentalUnitProperties.IdExperiment, exp.Code);
                                rowp.WriteNonEmptyString(RawExperimentalUnitProperties.IdExperimentalUnit, u.Code);
                                rowp.WriteNonEmptyString(RawExperimentalUnitProperties.PropertyName, cv.Key);
                                rowp.WriteNonEmptyString(RawExperimentalUnitProperties.Value, cv.Value);
                                dtp.Rows.Add(rowp);
                                props.Add(cv.Key);
                            }
                        }
                    }
                }
            }

            writeToCsv(tempFolder, tdx, dtx);
            writeToCsv(tempFolder, tdm, dtm);
            writeToCsv(tempFolder, tdd, dtd);
            writeToCsv(tempFolder, tdp, dtp);
        }
    }
}
