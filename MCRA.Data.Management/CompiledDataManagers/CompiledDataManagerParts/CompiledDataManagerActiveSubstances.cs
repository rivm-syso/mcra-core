using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager
    {

        public IDictionary<string, ActiveSubstanceModel> GetAllActiveSubstanceModels()
        {
            if (_data.AllActiveSubstanceModels == null) {
                LoadScope(SourceTableGroup.AssessmentGroupMemberships);
                var allActiveSubstanceModels = new Dictionary<string, ActiveSubstanceModel>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.AssessmentGroupMemberships);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllEffects();
                    GetAllCompounds();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawAssessmentGroupMembershipModels>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idAssessmentGroupMembershipModel = r.GetString(RawAssessmentGroupMembershipModels.Id, fieldMap);
                                        var idEffect = r.GetString(RawAssessmentGroupMembershipModels.IdEffect, fieldMap);

                                        //check both links: don't use &&
                                        var valid = IsCodeSelected(ScopingType.ActiveSubstancesModels, idAssessmentGroupMembershipModel)
                                                  & CheckLinkSelected(ScopingType.Effects, idEffect);

                                        if (!valid) {
                                            continue;
                                        }

                                        var effect = _data.GetOrAddEffect(idEffect);

                                        var idIndexSubstance = r.GetStringOrNull(RawAssessmentGroupMembershipModels.idIndexSubstance, fieldMap);
                                        var indexSubstance = !string.IsNullOrEmpty(idIndexSubstance)
                                            ? _data.GetOrAddSubstance(idIndexSubstance)
                                            : null;

                                        var assessmentGroupMembershipModel = new ActiveSubstanceModel {
                                            Code = idAssessmentGroupMembershipModel,
                                            Name = r.GetStringOrNull(RawAssessmentGroupMembershipModels.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawAssessmentGroupMembershipModels.Description, fieldMap),
                                            Reference = r.GetStringOrNull(RawAssessmentGroupMembershipModels.Reference, fieldMap),
                                            Accuracy = r.GetDoubleOrNull(RawAssessmentGroupMembershipModels.Accuracy, fieldMap),
                                            Sensitivity = r.GetDoubleOrNull(RawAssessmentGroupMembershipModels.Sensitivity, fieldMap),
                                            Specificity = r.GetDoubleOrNull(RawAssessmentGroupMembershipModels.Specificity, fieldMap),
                                            Effect = effect,
                                            IndexSubstance = indexSubstance,
                                        };
                                        allActiveSubstanceModels[idAssessmentGroupMembershipModel] = assessmentGroupMembershipModel;
                                    }
                                    //add items by code from the scope where no matched items were found in the source
                                    var readingScope = GetCodesInScope(ScopingType.ActiveSubstancesModels);
                                    foreach (var code in readingScope.Except(allActiveSubstanceModels.Keys, StringComparer.OrdinalIgnoreCase)) {
                                        allActiveSubstanceModels[code] = new ActiveSubstanceModel { Code = code };
                                    }
                                }
                            }
                        }

                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawAssessmentGroupMemberships>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idSubstance = r.GetString(RawAssessmentGroupMemberships.IdCompound, fieldMap);
                                        var idAssessmentGroupMembershipModel = r.GetString(RawAssessmentGroupMemberships.IdGroupMembershipModel, fieldMap);

                                        //check both links: don't use &&
                                        var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance)
                                                  & CheckLinkSelected(ScopingType.ActiveSubstancesModels, idAssessmentGroupMembershipModel);

                                        if (valid) {
                                            var substance = _data.GetOrAddSubstance(idSubstance);
                                            var activeSubstance = allActiveSubstanceModels[idAssessmentGroupMembershipModel];
                                            var membershipProbability = r.GetDouble(RawAssessmentGroupMemberships.MembershipProbability, fieldMap);
                                            activeSubstance.MembershipProbabilities.Add(substance, membershipProbability);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllActiveSubstanceModels = allActiveSubstanceModels;
            }
            return _data.AllActiveSubstanceModels;
        }

        private static void writeActiveSubstanceDataToCsv(string tempFolder, IEnumerable<ActiveSubstanceModel> data) {
            if (!data?.Any() ?? true) {
                return;
            }

            var mapper = new RawActiveSubstancesDataConverter();
            var rawData = mapper.ToRaw(data);
            var writer = new CsvRawDataWriter(tempFolder);
            writer.Set(rawData);
            writer.Store();
        }
    }
}
