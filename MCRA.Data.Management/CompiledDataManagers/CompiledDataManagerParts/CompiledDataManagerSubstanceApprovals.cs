using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Returns all substance approvals of the compiled datasource.
        /// </summary>
        public ICollection<SubstanceApproval> GetAllSubstanceApprovals() {
            if (_data.AllSubstanceApprovals == null) {
                LoadScope(SourceTableGroup.SubstanceApprovals);
                var allSubstanceApprovals = new List<SubstanceApproval>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.SubstanceApprovals);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawSubstanceApprovals>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawSubstanceApprovals.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var record = new SubstanceApproval() {
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            IsApproved = r.GetBooleanOrNull(RawSubstanceApprovals.IsApproved, fieldMap) ?? false
                                        };
                                        allSubstanceApprovals.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllSubstanceApprovals = allSubstanceApprovals;
            }
            return _data.AllSubstanceApprovals;
        }
    }
}
