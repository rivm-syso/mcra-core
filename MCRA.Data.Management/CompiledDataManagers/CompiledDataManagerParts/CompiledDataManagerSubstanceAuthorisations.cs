using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Returns all authorised uses of the compiled datasource.
        /// </summary>
        public ICollection<SubstanceAuthorisation> GetAllSubstanceAuthorisations() {
            if (_data.AllSubstanceAuthorisations == null) {
                LoadScope(SourceTableGroup.AuthorisedUses);
                var allSubstanceAuthorisations = new List<SubstanceAuthorisation>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.AuthorisedUses);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllFoods();
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawAuthorisedUses>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawAuthorisedUses.IdFood, fieldMap);
                                    var idSubstance = r.GetString(RawAuthorisedUses.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var record = new SubstanceAuthorisation() {
                                            Food = getOrAddFood(idFood),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Reference = r.GetStringOrNull(RawAuthorisedUses.Reference, fieldMap)
                                        };
                                        allSubstanceAuthorisations.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllSubstanceAuthorisations = allSubstanceAuthorisations;
            }
            return _data.AllSubstanceAuthorisations;
        }

        private static void writeSubstanceAuthorisationsDataToCsv(string tempFolder, IEnumerable<SubstanceAuthorisation> authorizedUses) {
            if (!authorizedUses?.Any() ?? true) {
                return;
            }

            var tdSubstanceAuthorisations = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.AuthorisedUses);
            var dtASubstanceAuthorisations = tdSubstanceAuthorisations.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawAuthorisedUses)).Length];
            foreach (var t in authorizedUses) {
                var r = dtASubstanceAuthorisations.NewRow();
                r.WriteNonEmptyString(RawAuthorisedUses.IdFood, t.Food?.Code, ccr);
                r.WriteNonEmptyString(RawAuthorisedUses.IdSubstance, t.Substance?.Code, ccr);
                r.WriteNonEmptyString(RawAuthorisedUses.Reference, t.Reference, ccr);
                dtASubstanceAuthorisations.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdSubstanceAuthorisations, dtASubstanceAuthorisations);
        }
    }
}
