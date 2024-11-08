using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        public IDictionary<string, QsarMembershipModel> GetAllQsarMembershipModels() {
            if (_data.AllQsarMembershipModels == null) {
                LoadScope(SourceTableGroup.QsarMembershipModels);
                var qsarMembershipModels = new Dictionary<string, QsarMembershipModel>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.QsarMembershipModels);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllEffects();
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read QSAR models
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawQSARMembershipModels>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idModel = r.GetString(RawQSARMembershipModels.Id, fieldMap);
                                        var idEffect = r.GetString(RawQSARMembershipModels.IdEffect, fieldMap);

                                        var valid = IsCodeSelected(ScopingType.QsarMembershipModels, idModel)
                                                  & CheckLinkSelected(ScopingType.Effects, idEffect);
                                        if (valid) {
                                            var model = new QsarMembershipModel() {
                                                Code = idModel,
                                                Name = r.GetStringOrNull(RawQSARMembershipModels.Name, fieldMap),
                                                Description = r.GetStringOrNull(RawQSARMembershipModels.Description, fieldMap),
                                                Effect = _data.GetOrAddEffect(idEffect),
                                                Accuracy = r.GetDoubleOrNull(RawQSARMembershipModels.Accuracy, fieldMap),
                                                Sensitivity = r.GetDoubleOrNull(RawQSARMembershipModels.Sensitivity, fieldMap),
                                                Specificity = r.GetDoubleOrNull(RawQSARMembershipModels.Specificity, fieldMap),
                                                MembershipScores = new Dictionary<Compound, double>(),
                                            };
                                            qsarMembershipModels[idModel] = model;
                                        }
                                    }
                                }
                            }
                        }

                        // Read QSAR model memberships
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawQSARMembershipScores>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idSubstance = r.GetString(RawQSARMembershipScores.IdSubstance, fieldMap);
                                        var idModel = r.GetString(RawQSARMembershipScores.IdQSARMembershipModel, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.QsarMembershipModels, idModel)
                                                  & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                        if (valid) {
                                            var model = qsarMembershipModels[idModel];
                                            var substance = _data.GetOrAddSubstance(idSubstance);
                                            if (model.MembershipScores.ContainsKey(substance)) {
                                                throw new Exception($"Duplicate substance code {idSubstance} in table {RawDataSourceTableID.QsarMembershipScores} for QSAR membership model {idModel}.");
                                            }
                                            var membershipScore = r.GetDoubleOrNull(RawQSARMembershipScores.MembershipScore, fieldMap);
                                            if (membershipScore.HasValue) {
                                                model.MembershipScores.Add(substance, membershipScore.Value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllQsarMembershipModels = qsarMembershipModels;
            }
            return _data.AllQsarMembershipModels;
        }

        private static void writeQsarMembershipModelDataToCsv(string tempFolder, IEnumerable<QsarMembershipModel> models) {
            if (!models?.Any() ?? true) {
                return;
            }

            var tdm = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.QsarMembershipModels);
            var dtm = tdm.CreateDataTable();
            var tds = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.QsarMembershipScores);
            var dts = tds.CreateDataTable();

            foreach (var instance in models) {
                var row = dtm.NewRow();
                row.WriteNonEmptyString(RawQSARMembershipModels.Id, instance.Code);
                row.WriteNonEmptyString(RawQSARMembershipModels.IdEffect, instance.Effect?.Code);
                row.WriteNonEmptyString(RawQSARMembershipModels.Name, instance.Name);
                row.WriteNonEmptyString(RawQSARMembershipModels.Description, instance.Description);
                row.WriteNonEmptyString(RawQSARMembershipModels.Reference, instance.Reference);
                row.WriteNonNullDouble(RawQSARMembershipModels.Accuracy, instance.Accuracy);
                row.WriteNonNullDouble(RawQSARMembershipModels.Sensitivity, instance.Sensitivity);

                dtm.Rows.Add(row);
                if (instance.MembershipScores != null) {
                    foreach (var param in instance.MembershipScores) {
                        var rowb = dts.NewRow();
                        rowb.WriteNonEmptyString(RawQSARMembershipScores.IdQSARMembershipModel, instance.Code);
                        rowb.WriteNonEmptyString(RawQSARMembershipScores.IdSubstance, param.Key.Code);
                        rowb.WriteNonNaNDouble(RawQSARMembershipScores.MembershipScore, param.Value);
                        dts.Rows.Add(rowb);
                    }
                }
            }
            writeToCsv(tempFolder, tdm, dtm);
            writeToCsv(tempFolder, tds, dts);
        }
    }
}
